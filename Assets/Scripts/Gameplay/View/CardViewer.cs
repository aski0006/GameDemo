using AsakiFramework;
using AsakiFramework.ObjectPool;
using Gameplay.GA;
using Gameplay.System;
using Gameplay.Utility;
using Gameplay.Model;
using Gameplay.UI;
using System;
using TMPro;
using UnityEngine;

namespace Gameplay.View
{
    public class CardViewer : AsakiMono
    {
        [Header("卡牌包装器"), SerializeField] private GameObject wrapper;
        [Space]
        [Header("卡牌精灵渲染器"), SerializeField] private SpriteRenderer cardRenderer;
        [Space]
        [Header("卡牌文本组件列表")]
        [Header("卡牌名称文本组件"), SerializeField] TMP_Text cardNameText;
        [Header("卡牌描述文本组件"), SerializeField] TMP_Text cardDescriptionText;
        [Header("卡牌费用文本组件"), SerializeField] TMP_Text cardCostText;
        [Space]
        [Header("卡牌交互层"), SerializeField] private LayerMask interactionLayerMask;
        private CardViewHoverSystem cardViewHoverSystem;
        private InteractionSystem interactionSystem;
        private ManualTargetSystem manualTargetSystem;
        private EnemySystem enemySystem;

        //---------------------------------------------------------------
        private Vector3 dragStartPos;
        private Quaternion dragStartRot;
        private void Awake()
        {
            HasNotNullComponent(cardRenderer);
            HasNotNullComponent(cardNameText);
            HasNotNullComponent(cardDescriptionText);
            HasNotNullComponent(cardCostText);
            cardViewHoverSystem = FromScene<CardViewHoverSystem>();
            interactionSystem = FromScene<InteractionSystem>();
            manualTargetSystem = FromScene<ManualTargetSystem>();
            enemySystem = FromScene<EnemySystem>();
        }
        public Card Card { get; private set; }
        // 视图初始化
        public void Setup(Card model)
        {
            Card = model;
            cardRenderer.sprite = model.cardSprite;
            cardNameText.text = model.cardName;
            cardDescriptionText.text = model.cardDescription;
            cardCostText.text = model.cardCost.ToString();
        }

        #region 卡牌鼠标交互

        private void OnMouseEnter()
        {
            if (interactionSystem.PlayerCanHover() == false) return;
            wrapper.SetActive(false);
            cardViewHoverSystem.ShowHoverCardView(Card, transform.position);
        }

        private void OnMouseExit()
        {
            if (interactionSystem.PlayerCanHover() == false) return;
            wrapper.SetActive(true);
            cardViewHoverSystem.HideHoverCardView();
        }

        private void OnMouseDown()
        {
            if (interactionSystem.PlayerCanInteract() == false) return;
            if (Card.manualTargetEffect != null)
            {

                LogInfo("手动选取目标");
                manualTargetSystem.StartTargeting(transform.position);
            }
            else
            {
                Lock(interactionSystem, () => { interactionSystem.PLayerIsDragging = true; });
                cardViewHoverSystem.HideHoverCardView();
                wrapper.SetActive(true);
                dragStartPos = transform.position;
                dragStartRot = transform.rotation;
                transform.rotation = Quaternion.Euler(0, 0, 0);
                transform.position = MouseUitility.GetMouseWorldPositionInWorldSpace(-1);
            }

        }

        private void OnMouseDrag()
        {
            if (interactionSystem.PlayerCanInteract() == false) return;

            if (Card.manualTargetEffect != null) return;
            transform.position = MouseUitility.GetMouseWorldPositionInWorldSpace(-1);
        }

        private void OnMouseUp()
        {
            if (interactionSystem.PlayerCanInteract() == false) return;
            var e = new CostSystem.TryPlayCardEvent
            {
                cardCost = Card.cardCost,
                canPlay = true // 默认允许
            };
            EventBus.Instance.TriggerRef(ref e);
            LogInfo("尝试使用卡牌 : " + e.canPlay);
            if (Card.manualTargetEffect != null)
            {
                LogInfo("使用手动选取目标效果");
                var enemyView = manualTargetSystem.EndTargeting(
                    MouseUitility.GetMouseWorldPositionInWorldSpace(0)
                );
                if (enemyView == null)
                {
                    return;
                }
                if (e.canPlay)
                {
                    var ctrl = enemySystem.GetEnemyControllerByView(enemyView);
                    if (ctrl == null)
                    {
                        LogError($"未能通过视图找到敌人控制器：{enemyView.name}，可能尚未注册或已被移除。取消出牌。");
                        return;
                    }
                    PlayCardGA playCardGa = new PlayCardGA(Card, ctrl);
                    ActionSystem.Instance.PerformGameAction(playCardGa);
                }
            }
            else
            {
                if (Physics.Raycast(
                        transform.position, Vector3.forward,
                        out RaycastHit hit, 10f, interactionLayerMask) && e.canPlay
                )
                {
                    DoMouseUpAction(hit);
                }
                else
                {
                    EventBus.Instance.Trigger(new CardCostUI.CostInsufficientEvent());
                    transform.position = dragStartPos;
                    transform.rotation = dragStartRot;
                }
                Lock(interactionSystem, () => { interactionSystem.PLayerIsDragging = false; });
            }

        }

        #endregion

        private void DoMouseUpAction(RaycastHit hitInfo)
        {
            //TODO: 根据射线检测到的物体，执行不同的操作
            PlayCardGA playCardGa = new PlayCardGA(Card);
            ActionSystem.Instance.PerformGameAction(playCardGa);
        }
    }
}