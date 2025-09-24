using AsakiFramework;
using Gameplay.GA;
using Gameplay.UI;
using System.Collections;
using UnityEngine;

namespace Gameplay.System
{
    public class CostSystem : AsakiMono
    {
        [Header("配置")][Space]
        [Header("当前最大费用")][SerializeField] private int maxCost = 20;

        private int currentCost;
        private void Awake()
        {
            currentCost = maxCost;
        }

        private void Start()
        {
            EventBus.Instance.Trigger(new CardCostUI.CostChangedEvent { oldCost = 0, newCost = currentCost });
        }

        private void OnEnable()
        {
            ActionSystem.Instance.AttachPerformer<UsageCostGA>(UsageCostPerformer);
            ActionSystem.Instance.AttachPerformer<RefillCostGA>(RefillCostPerformer);
            ActionSystem.Instance.AttachPerformer<RefillAllCostGA>(RefillAllCostPerformer);

            ActionSystem.Instance.SubscribePost<EnemyTurnGA>(OnEnemyTurnPostReaction);

            EventBus.Instance.SubscribeRef<TryPlayCardEvent>(TryPlayCardEventHandler);
        }

        private void OnDisable()
        {
            ActionSystem.Instance?.DetachPerformer<UsageCostGA>();
            ActionSystem.Instance?.DetachPerformer<RefillCostGA>();
            ActionSystem.Instance?.DetachPerformer<RefillAllCostGA>();

            ActionSystem.Instance?.UnsubscribePost<EnemyTurnGA>(OnEnemyTurnPostReaction);

            if (EventBus.Instance == null) return;
            EventBus.Instance?.UnsubscribeRef<TryPlayCardEvent>(TryPlayCardEventHandler);
        }

        #region 事件处理

        private void TryPlayCardEventHandler(ref TryPlayCardEvent eventData)
        {
            if (currentCost < eventData.cardCost)
                eventData.canPlay = false;
        }

        #endregion

        #region 响应器

        private void OnEnemyTurnPostReaction(EnemyTurnGA enemyTurnGa)
        {
            enemyTurnGa.AddPostReaction(new RefillAllCostGA());
            // 在敌人回合结束时，添加一个补充所有费用的反应
        }

        #endregion

        #region 事件

        public struct TryPlayCardEvent
        {
            public int cardCost;
            public bool canPlay;
        }

        #endregion

        #region 执行器

        private IEnumerator UsageCostPerformer(UsageCostGA usageCostGA)
        {
            if (currentCost < usageCostGA.UsageCostAmount)
            {
                LogError($"当前费用不足，无法执行使用费用操作。当前费用: {currentCost}, 需要费用: {usageCostGA.UsageCostAmount}");
                yield break;
            }
            int oldCost = currentCost;
            currentCost -= usageCostGA.UsageCostAmount;
            EventBus.Instance.Trigger(new CardCostUI.CostChangedEvent { oldCost = oldCost, newCost = currentCost });

        }

        private IEnumerator RefillCostPerformer(RefillCostGA refillCostGA)
        {
            int oldCost = currentCost;
            currentCost += refillCostGA.RefillCostAmount;
            currentCost = Mathf.Min(currentCost, maxCost); // 确保不超过最大费用
            EventBus.Instance.Trigger(new CardCostUI.CostChangedEvent { oldCost = oldCost, newCost = currentCost });
            yield return null;
        }

        private IEnumerator RefillAllCostPerformer(RefillAllCostGA refillAllCostGA)
        {
            int oldCost = currentCost;
            currentCost = maxCost; // 重置费用到最大值
            EventBus.Instance.Trigger(new CardCostUI.CostChangedEvent { oldCost = oldCost, newCost = currentCost });
            yield return null;
        }

        #endregion
    }
}
