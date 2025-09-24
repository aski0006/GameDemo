using AsakiFramework;
using DG.Tweening;
using Gameplay.MVC.Model;
using Gameplay.UI;
using System;
using UnityEngine;

namespace Gameplay.System
{
    public class CardViewHoverSystem : AsakiMono
    {
        [Header("悬停卡牌视图")][SerializeField] private CardViewer hoverCardView;
        [Header("悬浮卡牌位置偏移")][SerializeField] private Vector3 hoverCardOffset = new Vector3(0, 2, 0);
        [Header("悬停卡牌缩放比例")][SerializeField][Min(0.1f)] private float hoverCardScale = 1.5f;

        [Header("DOTween 设置")]
        [SerializeField][Min(0f)] private float hoverMoveDuration = 0.15f;
        [SerializeField][Min(0f)] private float hoverScaleDuration = 0.15f;
        [SerializeField] private Ease hoverEase = Ease.OutBack;
        [SerializeField][Min(0f)] private float hideDuration = 0.12f;

        // 用于管理当前播放的 Sequence，方便在新的悬停/隐藏时清理
        private Sequence currentSequence;

        /// <summary>
        ///     显示悬停视图并播放平滑动画。
        ///     position: 通常传入原始卡牌位置（world space），动画将从该位置移动到 position + hoverCardOffset。
        /// </summary>
        public void ShowHoverCardView(Card card, Vector3 position)
        {
            if (hoverCardView == null) return;

            // 先停止可能存在的动画
            KillCurrentSequence();

            hoverCardView.gameObject.SetActive(true);
            hoverCardView.BindModel(card);

            // 初始状态：从卡牌位置弹出，并从比目标略小的 scale 演出（更有“弹出”感）
            Vector3 targetPos = position + hoverCardOffset;
            Vector3 startPos = position;
            float startScale = Mathf.Max(0.8f, hoverCardScale * 0.6f);

            // 立即设置起始值（避免瞬移）
            hoverCardView.transform.position = startPos;
            hoverCardView.transform.localScale = Vector3.one * startScale;

            // 创建并播放动画序列（移动 + 缩放）
            currentSequence = DOTween.Sequence();
            currentSequence.Append(hoverCardView.transform.DOMove(targetPos, hoverMoveDuration).SetEase(hoverEase));
            currentSequence.Join(hoverCardView.transform.DOScale(Vector3.one * hoverCardScale, hoverScaleDuration).SetEase(hoverEase));
            currentSequence.Play();
        }

        /// <summary>
        ///     隐藏悬停视图，并播放收起动画，动画完成后隐藏视图并解绑模型。
        /// </summary>
        public void HideHoverCardView()
        {
            if (hoverCardView == null) return;

            KillCurrentSequence();

            // 如果当前视图未激活，直接返回（或保证 Unbind）
            if (!hoverCardView.gameObject.activeSelf)
            {
                hoverCardView.UnbindModel();
                return;
            }

            // 播放缩小并在结束后隐藏与解绑
            currentSequence = DOTween.Sequence();
            currentSequence.Append(hoverCardView.transform.DOScale(Vector3.zero, hideDuration).SetEase(Ease.InBack));
            currentSequence.OnComplete(() =>
            {
                try
                {
                    hoverCardView.UnbindModel();
                    hoverCardView.gameObject.SetActive(false);
                    // 恢复 scale 以防复用时视觉异常（池获取时会设置正确值）
                    hoverCardView.transform.localScale = Vector3.one;
                }
                catch (Exception ex)
                {
                    LogError($"HideHoverCardView 完成回调出错: {ex}");
                }
            });
            currentSequence.Play();
        }

        private void KillCurrentSequence()
        {
            if (currentSequence != null && currentSequence.IsActive())
            {
                currentSequence.Kill(true);
                currentSequence = null;
            }
            // 额外确保 transform 上的 tweens 被清理，避免残留
            if (hoverCardView != null)
            {
                hoverCardView.transform.DOKill(true);
            }
        }
    }
}
