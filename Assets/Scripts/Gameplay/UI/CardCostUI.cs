// CardCostUI.cs

using AsakiFramework;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Gameplay.UI
{
    public class CardCostUI : AsakiMono
    {
        [Header("卡牌费用 UI")]
        [SerializeField] private GameObject wrapper;
        [SerializeField] private TMP_Text costText;

        private Sequence _costUpdateSequence;
        private Sequence _shakeSequence;

        // 事件
        public struct CostChangedEvent
        {
            public int oldCost;
            public int newCost;
        }

        public struct CostInsufficientEvent
        { }
        private void OnEnable()
        {
            EventBus.Instance.Subscribe<CostChangedEvent>(UpdateCostTextHandler);
            EventBus.Instance.Subscribe<CostInsufficientEvent>(ShakeCostWrapper);
        }

        private void OnDisable()
        {
            EventBus.Instance?.Unsubscribe<CostChangedEvent>(UpdateCostTextHandler);
            EventBus.Instance?.Unsubscribe<CostInsufficientEvent>(ShakeCostWrapper);
        }

        // 事件处理方法
        public void UpdateCostTextHandler(CostChangedEvent e)
        {
            UpdateCostWithAnimation(e.oldCost, e.newCost);
        }

        /// <summary>
        /// 使用工具类更新费用文本动画
        /// </summary>
        private void UpdateCostWithAnimation(int startCost, int targetCost, float duration = 0.5f)
        {
            // 安全停止之前的动画
            _costUpdateSequence.SafeKill();

            _costUpdateSequence = DOTween.Sequence();

            // 数值变化动画
            _costUpdateSequence.Append(
                DOTweenUtility.AnimateIntWithCallback(
                    startCost,
                    targetCost,
                    duration,
                    value => costText.text = value.ToString(),
                    Ease.OutQuad
                )
            );

            // 同时播放缩放脉冲效果
            _costUpdateSequence.Join(costText.transform.PulseScale(1.3f, duration));

            // 如果费用减少，播放颜色闪烁效果
            if (targetCost < startCost)
            {
                _costUpdateSequence.Join(costText.BlinkColor(Color.green, duration, 2));
            }
            // 如果费用增加，播放红色闪烁
            else if (targetCost > startCost)
            {
                _costUpdateSequence.Join(costText.BlinkColor(Color.red, duration, 2));
            }
        }

        /// <summary>
        /// 震动费用包装器
        /// </summary>
        public void ShakeCostWrapper(CostInsufficientEvent e)
        {
            ShakeCostWrapperWithEffect();
        }

        /// <summary>
        /// 使用工具类实现震动效果
        /// </summary>
        private void ShakeCostWrapperWithEffect(float duration = 0.5f)
        {
            // 安全停止之前的震动
            _shakeSequence.SafeKill();

            // 使用工具类的UI震动方法
            _shakeSequence = wrapper.transform.ShakeUI(
                positionStrength: 5f,
                rotationStrength: 8f,
                scaleStrength: 0.2f,
                duration: duration
            );

            // 添加回调
            _shakeSequence.OnComplete(() =>
            {
                LogInfo("费用UI震动完成");
            });
        }

        /// <summary>
        /// 费用不足时的警告效果
        /// </summary>
        public void PlayCostInsufficientWarning()
        {
            _shakeSequence.SafeKill();

            _shakeSequence = DOTween.Sequence();

            // 红色闪烁
            _shakeSequence.Append(costText.BlinkColor(Color.red, 0.6f, 3));

            // 强烈震动
            _shakeSequence.Join(wrapper.transform.ShakeUI(8f, 10f, 0.3f, 0.6f));
        }

        /// <summary>
        /// 重置UI状态
        /// </summary>
        public void ResetUIState()
        {
            // 安全停止所有动画
            _costUpdateSequence.SafeKill();
            _shakeSequence.SafeKill();

            // 重置Transform状态
            wrapper.transform.ResetTweenState();
            costText.transform.ResetTweenState();

            // 重置颜色
            costText.color = Color.white;
        }

        private void OnDestroy()
        {
            // 清理所有Tween
            _costUpdateSequence.SafeKill();
            _shakeSequence.SafeKill();
            wrapper.transform.DOKill();
            costText.transform.DOKill();
        }
    }
}
