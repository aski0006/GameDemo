using System;
using System.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AsakiFramework
{
    /// <summary>
    /// 通用协程工具类
    /// 提供便捷的协程管理、延迟执行、条件等待等功能
    /// </summary>
    public static class CoroutineUtility
    {
        private static CoroutineRunner _runner;
        
        /// <summary>
        /// 协程运行器，确保有GameObject可以运行协程
        /// </summary>
        private static CoroutineRunner Runner
        {
            get
            {
                if (_runner == null)
                {
                    GameObject go = new GameObject("[CoroutineRunner]");
                    _runner = go.AddComponent<CoroutineRunner>();
                    Object.DontDestroyOnLoad(go);
                }
                return _runner;
            }
        }

        #region 基础协程方法

        /// <summary>
        /// 开始一个协程
        /// </summary>
        /// <param name="routine">协程方法</param>
        /// <returns>协程实例</returns>
        public static Coroutine StartCoroutine(IEnumerator routine)
        {
            return Runner.StartCoroutine(routine);
        }

        /// <summary>
        /// 停止一个协程
        /// </summary>
        /// <param name="coroutine">要停止的协程</param>
        public static void StopCoroutine(Coroutine coroutine)
        {
            if (_runner != null && coroutine != null)
            {
                _runner.StopCoroutine(coroutine);
            }
        }

        /// <summary>
        /// 停止所有协程
        /// </summary>
        public static void StopAllCoroutines()
        {
            if (_runner != null)
            {
                _runner.StopAllCoroutines();
            }
        }

        #endregion

        #region 延迟执行方法

        /// <summary>
        /// 延迟指定秒数后执行动作
        /// </summary>
        /// <param name="delay">延迟时间（秒）</param>
        /// <param name="action">要执行的动作</param>
        /// <returns>协程实例</returns>
        public static Coroutine Delay(float delay, Action action)
        {
            return StartCoroutine(DelayCoroutine(delay, action));
        }

        /// <summary>
        /// 延迟指定帧数后执行动作
        /// </summary>
        /// <param name="frames">延迟帧数</param>
        /// <param name="action">要执行的动作</param>
        /// <returns>协程实例</returns>
        public static Coroutine DelayFrames(int frames, Action action)
        {
            return StartCoroutine(DelayFramesCoroutine(frames, action));
        }

        /// <summary>
        /// 延迟到下一帧执行动作
        /// </summary>
        /// <param name="action">要执行的动作</param>
        /// <returns>协程实例</returns>
        public static Coroutine DelayOneFrame(Action action)
        {
            return DelayFrames(1, action);
        }

        /// <summary>
        /// 延迟到指定条件满足后执行动作
        /// </summary>
        /// <param name="condition">条件函数</param>
        /// <param name="action">要执行的动作</param>
        /// <param name="checkInterval">检查间隔（秒）</param>
        /// <returns>协程实例</returns>
        public static Coroutine DelayUntil(Func<bool> condition, Action action, float checkInterval = 0.1f)
        {
            return StartCoroutine(DelayUntilCoroutine(condition, action, checkInterval));
        }

        /// <summary>
        /// 延迟到指定条件为false后执行动作
        /// </summary>
        /// <param name="condition">条件函数</param>
        /// <param name="action">要执行的动作</param>
        /// <param name="checkInterval">检查间隔（秒）</param>
        /// <returns>协程实例</returns>
        public static Coroutine DelayWhile(Func<bool> condition, Action action, float checkInterval = 0.1f)
        {
            return StartCoroutine(DelayWhileCoroutine(condition, action, checkInterval));
        }

        #endregion

        #region 重复执行方法

        /// <summary>
        /// 重复执行动作指定次数
        /// </summary>
        /// <param name="action">要执行的动作</param>
        /// <param name="count">重复次数</param>
        /// <param name="interval">间隔时间（秒）</param>
        /// <returns>协程实例</returns>
        public static Coroutine Repeat(Action action, int count, float interval = 0f)
        {
            return StartCoroutine(RepeatCoroutine(action, count, interval));
        }

        /// <summary>
        /// 无限重复执行动作（直到手动停止）
        /// </summary>
        /// <param name="action">要执行的动作</param>
        /// <param name="interval">间隔时间（秒）</param>
        /// <returns>协程实例</returns>
        public static Coroutine RepeatForever(Action action, float interval = 0f)
        {
            return StartCoroutine(RepeatForeverCoroutine(action, interval));
        }

        /// <summary>
        /// 每帧执行动作，持续指定秒数
        /// </summary>
        /// <param name="action">要执行的动作</param>
        /// <param name="duration">持续时间（秒）</param>
        /// <returns>协程实例</returns>
        public static Coroutine ExecuteForDuration(Action<float> action, float duration)
        {
            return StartCoroutine(ExecuteForDurationCoroutine(action, duration));
        }

        #endregion

        #region 协程实现

        private static IEnumerator DelayCoroutine(float delay, Action action)
        {
            yield return new WaitForSeconds(delay);
            action?.Invoke();
        }

        private static IEnumerator DelayFramesCoroutine(int frames, Action action)
        {
            for (int i = 0; i < frames; i++)
            {
                yield return null;
            }
            action?.Invoke();
        }

        private static IEnumerator DelayUntilCoroutine(Func<bool> condition, Action action, float checkInterval)
        {
            yield return new WaitUntil(condition);
            action?.Invoke();
        }

        private static IEnumerator DelayWhileCoroutine(Func<bool> condition, Action action, float checkInterval)
        {
            yield return new WaitWhile(condition);
            action?.Invoke();
        }

        private static IEnumerator RepeatCoroutine(Action action, int count, float interval)
        {
            for (int i = 0; i < count; i++)
            {
                action?.Invoke();
                if (i < count - 1 && interval > 0)
                {
                    yield return new WaitForSeconds(interval);
                }
                else if (i < count - 1)
                {
                    yield return null;
                }
            }
        }

        private static IEnumerator RepeatForeverCoroutine(Action action, float interval)
        {
            while (true)
            {
                action?.Invoke();
                if (interval > 0)
                {
                    yield return new WaitForSeconds(interval);
                }
                else
                {
                    yield return null;
                }
            }
        }

        private static IEnumerator ExecuteForDurationCoroutine(Action<float> action, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                action?.Invoke(elapsed);
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        #endregion
    }

    /// <summary>
    /// 协程运行器组件
    /// </summary>
    public class CoroutineRunner : MonoBehaviour
    {
        private void Awake()
        {
            // 确保不会被销毁
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            // 清理所有协程
            StopAllCoroutines();
        }
    }
}