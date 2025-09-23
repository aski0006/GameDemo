// DOTweenUtility.cs
using DG.Tweening;
using UnityEngine;

namespace AsakiFramework
{
    public static class DOTweenUtility
    {
        #region 震动相关方法
        
        /// <summary>
        /// 执行震动效果
        /// </summary>
        /// <param name="transform">要震动的Transform</param>
        /// <param name="strength">震动强度</param>
        /// <param name="duration">持续时间</param>
        /// <param name="vibrato">震动频率</param>
        /// <param name="randomness">随机性</param>
        /// <param name="fadeOut">是否淡出</param>
        public static Tween Shake(this Transform transform, 
            float strength = 1f, 
            float duration = 0.5f, 
            int vibrato = 10, 
            float randomness = 90f, 
            bool fadeOut = true)
        {
            return transform.DOShakePosition(duration, strength, vibrato, randomness, fadeOut);
        }

        /// <summary>
        /// 执行旋转震动效果
        /// </summary>
        public static Tween ShakeRotation(this Transform transform,
            float strength = 10f,
            float duration = 0.5f,
            int vibrato = 10,
            float randomness = 90f,
            bool fadeOut = true)
        {
            return transform.DOShakeRotation(duration, strength, vibrato, randomness, fadeOut);
        }

        /// <summary>
        /// 执行缩放震动效果
        /// </summary>
        public static Tween ShakeScale(this Transform transform,
            float strength = 0.2f,
            float duration = 0.5f,
            int vibrato = 10,
            float randomness = 90f,
            bool fadeOut = true)
        {
            return transform.DOShakeScale(duration, strength, vibrato, randomness, fadeOut);
        }

        /// <summary>
        /// 执行UI元素的震动效果（包含位置、旋转、缩放）
        /// </summary>
        public static Sequence ShakeUI(this Transform transform,
            float positionStrength = 3f,
            float rotationStrength = 5f,
            float scaleStrength = 0.1f,
            float duration = 0.5f)
        {
            var sequence = DOTween.Sequence();
            
            // 位置震动
            sequence.Join(transform.Shake(positionStrength, duration));
            // 旋转震动
            sequence.Join(transform.ShakeRotation(rotationStrength, duration));
            // 缩放震动
            sequence.Join(transform.ShakeScale(scaleStrength, duration));
            
            return sequence;
        }

        /// <summary>
        /// 执行脉冲缩放效果
        /// </summary>
        public static Sequence PulseScale(this Transform transform,
            float scaleTo = 1.2f,
            float duration = 0.3f,
            Ease ease = Ease.OutBack)
        {
            var sequence = DOTween.Sequence();
            sequence.Append(transform.DOScale(scaleTo, duration / 2).SetEase(ease));
            sequence.Append(transform.DOScale(1f, duration / 2).SetEase(ease));
            return sequence;
        }

        #endregion

        #region 数值动画方法
        
        /// <summary>
        /// 数值变化动画（推荐）：通过回调接收更新值
        /// 说明：不能在 lambda 内捕获并修改 ref 参数，因此将起始值作为普通参数传入，并通过 onValueUpdate 回调把每次更新值交给调用者。
        /// </summary>
        public static Tween AnimateFloat(float from, float to, float duration, System.Action<float> onValueUpdate, Ease ease = Ease.Linear)
        {
            // 使用局部变量捕获起始值（允许被 lambda 捕获）
            // 每次更新时通过回调通知调用者去更新自己的变量或 UI 等
            return DOTween.To(() => from, x => { from = x; onValueUpdate?.Invoke(x); }, to, duration).SetEase(ease);
        }

        /// <summary>
        /// 整数变化动画（推荐）：通过回调接收更新值
        /// </summary>
        public static Tween AnimateInt(int from, int to, float duration, System.Action<int> onValueUpdate, Ease ease = Ease.Linear)
        {
            return DOTween.To(() => from, x => { from = x; onValueUpdate?.Invoke(x); }, to, duration).SetEase(ease);
        }

        /// <summary>
        /// 带回调的数值动画（保留，仅改名为更明确的签名）
        /// </summary>
        public static Tween AnimateIntWithCallback(int from, int to, float duration, 
            System.Action<int> onValueUpdate, Ease ease = Ease.Linear)
        {
            return DOTween.To(() => from, x => onValueUpdate?.Invoke(x), to, duration).SetEase(ease);
        }

        #endregion

        #region 颜色动画方法
        
        /// <summary>
        /// 颜色变化动画
        /// </summary>
        public static Tween AnimateColor(this UnityEngine.UI.Graphic graphic, 
            Color toColor, float duration, Ease ease = Ease.Linear)
        {
            return graphic.DOColor(toColor, duration).SetEase(ease);
        }

        /// <summary>
        /// 颜色闪烁动画
        /// </summary>
        public static Sequence BlinkColor(this UnityEngine.UI.Graphic graphic, 
            Color blinkColor, float duration, int blinkCount = 2)
        {
            var sequence = DOTween.Sequence();
            var originalColor = graphic.color;
            
            for (int i = 0; i < blinkCount; i++)
            {
                sequence.Append(graphic.DOColor(blinkColor, duration / (blinkCount * 2)));
                sequence.Append(graphic.DOColor(originalColor, duration / (blinkCount * 2)));
            }
            
            return sequence;
        }

        #endregion

        #region 工具方法
        
        /// <summary>
        /// 安全停止Tween（避免空引用）
        /// </summary>
        public static void SafeKill(this Tween tween)
        {
            if (tween != null && tween.IsActive())
            {
                tween.Kill();
            }
        }

        /// <summary>
        /// 安全停止Sequence（避免空引用）
        /// </summary>
        public static void SafeKill(this Sequence sequence)
        {
            if (sequence != null && sequence.IsActive())
            {
                sequence.Kill();
            }
        }

        /// <summary>
        /// 重置Transform状态
        /// </summary>
        public static void ResetTweenState(this Transform transform)
        {
            transform.DOKill();
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        #endregion
    }
}