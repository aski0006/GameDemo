using AsakiFramework;
using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Gameplay.UI
{
    public class CardCostUI : AsakiMono
    {
        [Header("卡牌费用 UI")]
        [SerializeField] private TMP_Text costText;

        private Tween _counterTween;
        private Coroutine runningAnim;
        // 事件
        public struct CostChangedEvent
        {
            public int oldCost;
            public int newCost;
        }
        private void OnEnable()
        {
            EventBus.Instance.Subscribe<CostChangedEvent>(UpdateCostTextHandler);
        }

        private void OnDisable()
        {
            EventBus.Instance?.Unsubscribe<CostChangedEvent>(UpdateCostTextHandler);
        }

        // 事件处理方法
        private void UpdateCostTextHandler(CostChangedEvent e)
        {
            if(runningAnim != null) CoroutineUtility.StopCoroutine(runningAnim);
            runningAnim = CoroutineUtility.StartCoroutine(AnimateCostCoroutine(e.oldCost, e.newCost));
        }
        
        public IEnumerator AnimateCostCoroutine(int startCost,  int targetCost, float duration = 0.5f)
        {
            _counterTween?.Kill();

            bool completed = false;
            _counterTween = DOTween.To(
                    () => startCost,
                    val => costText.text = val.ToString(),
                    targetCost,
                    duration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => completed = true);

            yield return new WaitUntil(() => completed);
        }
        protected void OnDestroy()
        {
            _counterTween?.Kill();
        }
    }
}
