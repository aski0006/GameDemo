using AsakiFramework;
using AsakiFramework.ObjectPool;
using Gameplay.GA;
using Gameplay.MVC.Interfaces;
using Gameplay.System;
using Gameplay.Utility;
using Gameplay.MVC.Model;
using Gameplay.UI;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Gameplay.UI
{
    public class CardViewer : AsakiMono, IPoolable, IView
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
        public GUID BoundModelInstanceID => Card?.ModelInstanceID ?? default;

        public void UnbindModel()
        {
            Card = null;
        }
        // 视图初始化
        public void BindModel(Card model)
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
            // 允许 hover 的条件现在由 InteractionSystem 控制：
            if (interactionSystem.PlayerCanHover() == false) return;

            // 当处于 Targeting 模式时（玩家在使用一张需要手动选目标的卡），
            // 我们仍然想让目标保持 Hover（以提供视觉提示），同时目标卡本身不可被拖拽。
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

            // 如果当前卡为手动目标卡，进入 Targeting 模式（不会把 PLayerIsDragging 设为 true）
            if (Card.manualTargetEffect != null)
            {
                LogInfo("手动选取目标 - 进入 Targeting 模式");
                // 标记当前正在进行手动目标选择
                interactionSystem.StartManualTargeting(Card);
                // 启动箭头指示（箭头的隐藏在 ManualTargetSystem.EndTargeting 中处理）
                manualTargetSystem.StartTargeting(transform.position);
            }
            else
            {
                // 普通拖拽：如果当前处于 Targeting 模式，不允许启动普通拖拽
                Lock(interactionSystem, () =>
                {
                    var started = interactionSystem.StartDragging(Card);
                    if (!started)
                    {
                        // 如果因为处于 target 模式而拒绝拖拽，直接返回
                        return;
                    }
                });

                // 隐藏 hover 视图（若存在）
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

            // 如果正在目标选择模式或该卡属于手动目标卡，则禁止拖动更新位置（卡牌不可拖拽）
            if (interactionSystem.IsTargetingMode) return;
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

            if (Card.manualTargetEffect != null)
            {
                // 结束手动目标选择，获取被选中的 EnemyCharacterView（可能为 null）
                var enemyView = manualTargetSystem.EndTargeting(
                    MouseUitility.GetMouseWorldPositionInWorldSpace(0)
                );

                // 退出 Targeting 模式（不管是否选到目标，都应退出）
                interactionSystem.StopManualTargeting();

                if (enemyView == null)
                {
                    // 没有选中目标，直接返回（不消耗卡）
                    return;
                }
                if (e.canPlay)
                {
                    var ctrl = enemySystem.GetEnemyControllerByView(enemyView);
                    if (ctrl == null)
                    {
                        return;
                    }
                    PlayCardGA playCardGa = new PlayCardGA(Card, ctrl);
                    ActionSystem.Instance.PerformGameAction(playCardGa);
                }
            }
            else
            {
                // 普通放置逻辑（射线检测）
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

                // 结束普通拖拽
                Lock(interactionSystem, () => { interactionSystem.StopDragging(); });
            }
        }

        #endregion

        private void DoMouseUpAction(RaycastHit hitInfo)
        {
            //TODO: 根据射线检测到的物体，执行不同的操作
            PlayCardGA playCardGa = new PlayCardGA(Card);
            ActionSystem.Instance.PerformGameAction(playCardGa);
        }
        public void OnGetFromPool()
        {
            wrapper.SetActive(true);
        }
        public void OnReturnToPool()
        {
            UnbindModel();
            wrapper.SetActive(false);
            cardRenderer.sprite = null;
            cardRenderer.color = Color.white;
            cardNameText.text = "";
            cardDescriptionText.text = "";
            cardCostText.text = "";


        }
        public void OnDestroyFromPool()
        { }
    }
}
