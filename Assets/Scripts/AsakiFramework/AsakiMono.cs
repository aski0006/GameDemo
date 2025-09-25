using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AsakiFramework
{
    public class AsakiMono : MonoBehaviour
    {
        [Header("Asaki 框架脚本")]
        [Header("作者：Asaki")]
        [Header("启用日志开关，调试时打开，发布版关闭")][SerializeField]
        private bool isVerbose;

        #region 日志方法

        public bool IsLogEnabled
        {
            get => isVerbose;
            set => isVerbose = value;
        }
        public static void SetAllLogsEnabled(bool enabled)
        {
            var allMonos = FindObjectsByType<AsakiMono>(FindObjectsSortMode.InstanceID);
            foreach (var mono in allMonos)
            {
                mono.isVerbose = enabled;
            }
        }
        /* 发布版彻底剔除 */
#if UNITY_EDITOR
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public void LogInfo(object message, Object context = null)
        {
            if (isVerbose) Debug.Log($"[{name}] [INFO]  {message}", context);
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public void LogWarning(object message, Object context = null)
        {
            if (isVerbose) Debug.LogWarning($"[{name}] [WARN]  {message}", context);
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public void LogError(object message, Object context = null)
        {
            /* Error 一般总是需要看的，不再受 isVerbose 限制 */
            Debug.LogError($"[{name}] [ERROR] {message}", context);
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public void LogAssert(bool condition, object message, Object context = null)
        {
            if (!condition)
                Debug.LogError($"[{name}] [ASSERT] {message}", context);
        }

#else
        /* 发布版空实现，JIT 会内联掉，零开销 */
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public void LogInfo(object message, Object context = null) { }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public void LogWarning(object message, Object context = null) { }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public void LogError(object message, Object context = null) { }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public void LogAssert(bool condition, object message, Object context = null) { }
#endif

        #endregion

        #region 组件方法

        public enum FindComponentMode { Self, Parent, Children, Scene }

        /// <summary>
        /// 获取或添加组件。
        /// mode = Self   :  本 GameObject 上找，找不到就 OnAdd。
        /// mode = Parent :  在父级链上找，找不到返回 null（不会自动添加）。
        /// mode = Children: 在子级链上找，找不到返回 null（不会自动添加）。
        /// mode = Scene  :  在整个场景里找第一个，找不到返回 null（不会自动添加）。
        /// </summary>
        protected T GetOrAddComponent<T>(FindComponentMode mode = FindComponentMode.Self) where T : Component
        {
            T t = null;

            switch (mode)
            {
                case FindComponentMode.Self:
                    if (!gameObject.TryGetComponent(out t))
                        t = gameObject.AddComponent<T>();
                    break;

                case FindComponentMode.Parent:
                    t = GetComponentInParent<T>();
                    break;

                case FindComponentMode.Children:
                    t = GetComponentInChildren<T>(true);
                    break;

                case FindComponentMode.Scene:
                    t = FindObjectOfType<T>();
                    break;
            }

            return t;
        }

        /// <summary>
        /// 检查带有 [NotNullComponent] 特性的字段是否已赋值。
        /// 编辑器下若缺失则直接抛异常，方便快速定位；发布版仅返回 false。
        /// </summary>
        protected bool HasNotNullComponent<T>() where T : Component
        {
            T t = GetComponent<T>();
            // 不会屏蔽日志
            if (t == null)
                Debug.LogError("[" + name + "] 缺少必需的组件：" + typeof(T).Name, this);
            return t != null;
        }

        /// <summary>
        /// 检查“手动拖上去的引用”到底有没有值。
        /// 编辑器内缺失立即抛异常；发布版仅返回 false。
        /// 用法：HasNotNullComponent(cardRenderer);
        /// </summary>
        protected bool HasNotNullComponent<T>(T reference) where T : Object
        {
            if (reference != null) return true;

#if UNITY_EDITOR
            throw new MissingReferenceException(
                $"[NotNullComponent] {typeof(T).Name} 在 {name} 上未赋值！");
#else
    LogError($"缺少必需的组件/引用：{typeof(T).Name}");
    return false;
#endif
        }

        /// <summary>
        /// 带缓存的 GetComponent。
        /// 第一次调用时填充 cache，后续直接返回，避免重复查询。
        /// 注意：cache 必须是类的字段（ref 传递）。
        /// </summary>
        protected T GetCachedComponent<T>(ref T cache) where T : Component
        {
            if (cache == null) cache = GetComponent<T>();
            return cache;
        }

        protected T FromScene<T>() where T : Component
        {
            return GetOrAddComponent<T>(FindComponentMode.Scene);
        }

        #endregion

        #region 常用协程快捷方法

        /// <summary>
        /// 延迟指定秒数后执行
        /// </summary>
        protected Coroutine DelayTime(float seconds, Action action)
        {
            return CoroutineUtility.Delay(seconds, action);
        }

        /// <summary>
        /// 下一帧执行
        /// </summary>
        protected Coroutine RunNextFrame(Action action)
        {
            return CoroutineUtility.DelayOneFrame(action);
        }

        /// <summary>
        /// 延迟指定帧数后执行
        /// </summary>
        protected Coroutine DelayFrames(int frames, Action action)
        {
            return CoroutineUtility.DelayFrames(frames, action);
        }

        /// <summary>
        /// 条件满足后执行
        /// </summary>
        protected Coroutine DelayUntil(Func<bool> condition, Action action, float checkInterval = 0.1f)
        {
            return CoroutineUtility.DelayUntil(condition, action, checkInterval);
        }

        /// <summary>
        /// 条件为 false 期间一直等待，直到条件变为 true 后执行
        /// </summary>
        protected Coroutine DelayWhile(Func<bool> condition, Action action, float checkInterval = 0.1f)
        {
            return CoroutineUtility.DelayWhile(condition, action, checkInterval);
        }

        /// <summary>
        /// 循环执行 count 次，每次间隔 interval 秒
        /// </summary>
        protected Coroutine Loop(int count, float interval, Action<int> action)
        {
            return CoroutineUtility.StartCoroutine(_Loop(count, interval, action));
        }

        /// <summary>
        /// 无限循环，每次间隔 interval 秒，直到手动 StopCoroutine
        /// </summary>
        protected Coroutine LoopForever(float interval, Action action)
        {
            return CoroutineUtility.RepeatForever(action, interval);
        }

        /// <summary>
        /// 持续执行 action(elapsed) 指定秒数，每帧调用
        /// </summary>
        protected Coroutine ExecuteForDuration(float duration, Action<float> action)
        {
            return CoroutineUtility.ExecuteForDuration(action, duration);
        }

/* 内部实现：带索引的 Loop */
        private IEnumerator _Loop(int count, float interval, Action<int> action)
        {
            for (int i = 0; i < count; i++)
            {
                action?.Invoke(i);
                if (i < count - 1)
                {
                    if (interval > 0) yield return new WaitForSeconds(interval);
                    else yield return null;
                }
            }
        }

        #endregion

        #region 线程安全锁封装

        /// <summary>
        /// 使用静态锁执行线程安全操作（适合保护全局单例）
        /// </summary>
        protected void LockStatic(Action action, int timeoutMillis = -1)
        {
            AsakiLock.LockStatic(action, timeoutMillis);
        }

        /// <summary>
        /// 使用当前实例作为锁对象执行线程安全操作（适合保护当前组件状态）
        /// </summary>
        protected void Lock(Action action, int timeoutMillis = -1)
        {
            AsakiLock.Lock(this, action, timeoutMillis);
        }

        /// <summary>
        /// 使用指定锁对象执行线程安全操作
        /// </summary>
        protected void Lock(object lockObj, Action action, int timeoutMillis = -1)
        {
            AsakiLock.Lock(lockObj, action, timeoutMillis);
        }

        #endregion

        #region 分帧创建框架

        /// <summary>
        /// 分帧创建通用接口
        /// </summary>
        /// <typeparam name="TSource">原始数据类型</typeparam>
        /// <typeparam name="TResult">产出结果类型</typeparam>
        public interface IFrameCreationHandler<in TSource, out TResult>
        {
            /// <summary>
            /// 真正创建逻辑，可能抛出异常
            /// </summary>
            TResult Create(TSource data);

            /// <summary>
            /// 创建失败时回调，参数为 data 与异常
            /// </summary>
            void OnError(TSource data, Exception e);
        }

        /// <summary>
        /// 分帧创建驱动
        /// </summary>
        /// <param name="source">原始数据列表</param>
        /// <param name="handler">创建器</param>
        /// <param name="perFrame">每帧最大创建个数（默认 1）</param>
        /// <param name="maxMillisPerFrame">单帧最大耗时（毫秒），<=0 表示不限制（默认 0）</param>
        /// <param name="onProgress">进度回调 (current, total)</param>
        /// <param name="onComplete">全部完成回调</param>
        /// <typeparam name="TSource">原始数据类型</typeparam>
        /// <typeparam name="TResult">产出结果类型</typeparam>
        /// <returns>协程句柄</returns>
        protected Coroutine CreateOverFrames<TSource, TResult>(
            IList<TSource> source,
            IFrameCreationHandler<TSource, TResult> handler,
            int perFrame = 1,
            float maxMillisPerFrame = 0f,
            Action<int, int> onProgress = null,
            Action<IList<TResult>> onComplete = null)
        {
            return CoroutineUtility.StartCoroutine(_CreateOverFrames(source, handler, perFrame, maxMillisPerFrame, onProgress, onComplete));
        }

        private IEnumerator _CreateOverFrames<TSource, TResult>(
            IList<TSource> source,
            IFrameCreationHandler<TSource, TResult> handler,
            int perFrame,
            float maxMillisPerFrame,
            Action<int, int> onProgress,
            Action<IList<TResult>> onComplete)
        {
            int total = source.Count;
            var ret = new List<TResult>(total);
            var sw = System.Diagnostics.Stopwatch.StartNew();

            for (int i = 0; i < total;)
            {
                int createdInThisFrame = 0;
                while (createdInThisFrame < perFrame && i < total)
                {
                    var item = source[i];
                    try
                    {
                        var result = handler.Create(item);
                        ret.Add(result);
                    }
                    catch (Exception e)
                    {
                        handler.OnError(item, e);
                    }

                    ++i;
                    ++createdInThisFrame;

                    // 时间片兜底
                    if (maxMillisPerFrame > 0 && sw.ElapsedMilliseconds >= maxMillisPerFrame)
                        break;
                }

                onProgress?.Invoke(i, total);
                sw.Restart();
                yield return null;
            }

            onComplete?.Invoke(ret);
        }

#endregion

        #region 自动注册到服务定位器

        protected void AutoRegister<TService>(float maxWaitSeconds = 3f) where TService : AsakiMono
        {
            CoroutineUtility.StartCoroutine(AutoRegisterCoroutine<TService>(maxWaitSeconds));
        }

        private IEnumerator AutoRegisterCoroutine<TService>(float maxWaitSeconds) where TService : AsakiMono
        {
            float elapsedTime = 0f;
            while (AsakiMonoServiceLocator.Instance == null && elapsedTime < maxWaitSeconds)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            if (AsakiMonoServiceLocator.Instance == null)
            {
                LogError($"服务定位器初始化超时（{maxWaitSeconds}秒），无法注册服务: {typeof(TService).Name}");
                yield break;
            }
            if (this is TService service)
            {
                if (!AsakiMonoServiceLocator.Instance.TryRegisterService(service))
                {
                    LogError($"服务注册失败: {typeof(TService).Name}");
                }
                else
                {
                    LogInfo($"服务自动注册成功: {typeof(TService).Name}");
                }
            }
            else
            {
                LogError($"类型不匹配：当前对象不是 {typeof(TService).Name} 类型");
            }
        }

        #endregion

    }

    #region 属性特性

    public class NotNullComponentAttribute : PropertyAttribute
    { }

    #endregion
}
