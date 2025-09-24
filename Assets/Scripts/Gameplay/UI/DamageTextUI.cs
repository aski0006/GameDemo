using AsakiFramework;
using AsakiFramework.ObjectPool;
using DG.Tweening;
using System;
using UnityEngine;

namespace Gameplay.UI
{
    public class DamageTextUI : AsakiMono, IPoolable
    {
        [Header("伤害文本组件"), SerializeField] private TMPro.TMP_Text damageText;
        [Header("伤害数字颜色"), SerializeField] private Color damageColor = Color.white;
        [Header("暴击颜色"), SerializeField] private Color critColor = Color.yellow;

        [Header("显示动画设置")]
        [SerializeField] private float showMoveDistance = 80f;
        [SerializeField] private float showDuration = 1.2f;
        [SerializeField] private Ease showEase = Ease.OutCubic;
        [SerializeField] private float showScaleUp = 1.5f;
        [SerializeField] private float floatAmplitude = 10f; // 浮动幅度
        [SerializeField] private float floatFrequency = 2f; // 浮动频率

        [Header("隐藏动画设置")]
        [SerializeField] private float hideDuration = 0.3f;
        [SerializeField] private Ease hideEase = Ease.InCubic;

        private Sequence currentAnimationSequence;
        private Vector3 originalPosition;

        public void SetDamageText(int damage, bool isCritical = false)
        {
            damageText.text = damage.ToString();
            damageText.color = isCritical ? critColor : damageColor;
            damageText.fontSize = isCritical ? 36 : 28; // 暴击时字体更大
        }

        public void PlayShowAnimation(Transform targetTransform)
        {
            // 停止之前的动画
            currentAnimationSequence?.SafeKill();

            // 记录原始位置 - 使用世界坐标
            originalPosition = targetTransform.position;
            
            // 重要修改：将伤害文本移动到目标位置，但不修改目标transform
            transform.position = originalPosition;
            
            // 播放显示动画 - 现在作用于伤害文本自身的transform
            currentAnimationSequence = ShowDamageNumber();
        }

        public void PlayHideAnimation(Action onComplete = null)
        {
            // 停止之前的动画
            currentAnimationSequence?.SafeKill();

            // 播放隐藏动画
            var hideTween = HideDamageNumber(damageText);
            hideTween.onComplete += () => onComplete?.Invoke();
            currentAnimationSequence = DOTween.Sequence().Append(hideTween);
        }

        #region 伤害数字动画方法

        /// <summary>
        /// 显示伤害数字动画 - 现在作用于伤害文本自身的transform
        /// </summary>
        /// <returns>动画序列</returns>
        private Sequence ShowDamageNumber()
        {
            var sequence = DOTween.Sequence();

            // 重置初始状态 - 作用于伤害文本自身
            transform.localScale = Vector3.one * 0.5f; // 从小开始
            transform.localRotation = Quaternion.identity;

            // 初始弹跳效果 - 作用于伤害文本自身
            sequence.Append(transform.DOScale(showScaleUp, showDuration * 0.3f).SetEase(Ease.OutBack));
            sequence.Append(transform.DOScale(1f, showDuration * 0.2f).SetEase(Ease.InBack));

            // 向上移动 - 作用于伤害文本自身
            sequence.Join(transform.DOMoveY(originalPosition.y + showMoveDistance, showDuration).SetEase(showEase));

            // 添加轻微的左右浮动 - 作用于伤害文本自身
            sequence.Join(transform.DOMoveX(originalPosition.x + floatAmplitude, showDuration * 0.5f)
                .SetEase(Ease.InOutSine)
                .SetLoops(2, LoopType.Yoyo));

            // 旋转效果 - 作用于伤害文本自身
            sequence.Join(transform.DORotate(new Vector3(0, 0, UnityEngine.Random.Range(-15f, 15f)), 
                showDuration * 0.5f).SetEase(Ease.InOutSine));

            return sequence;
        }

        /// <summary>
        /// 隐藏伤害数字动画
        /// </summary>
        /// <param name="graphic">要隐藏的UI元素</param>
        /// <returns>动画补间</returns>
        private Tween HideDamageNumber(TMPro.TMP_Text graphic)
        {
            return graphic.DOFade(0, hideDuration).SetEase(hideEase);
        }

        #endregion

        public void OnGetFromPool()
        {
            // 重置状态
            damageText.color = damageColor;
            damageText.alpha = 1f;
            damageText.fontSize = 28;
            transform.localScale = Vector3.one;
            transform.localRotation = Quaternion.identity;
        }

        public void OnReturnToPool()
        {
            // 停止所有动画
            currentAnimationSequence?.SafeKill();
            currentAnimationSequence = null;
        }

        public void OnDestroyFromPool()
        {
            // 销毁时停止所有动画
            currentAnimationSequence?.SafeKill();
        }
    }
}