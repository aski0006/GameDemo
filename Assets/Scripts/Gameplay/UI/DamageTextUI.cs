using AsakiFramework;
using AsakiFramework.ObjectPool;
using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Gameplay.UI
{
    /// <summary>
    /// 适配卡牌游戏视觉风格的伤害数字 UI
    /// 修复说明：
    /// - 修复了原来通过修改 TMP_Text.color.alpha 动画导致某些情况下透明度始终为 0 的问题。
    /// - 解决方法：使用 CanvasGroup 来控制整条伤害文本的透明度（更稳定、可靠），同时保留对字体颜色和大小的控制。
    /// </summary>
    public class DamageTextUI : AsakiMono, IPoolable
    {
        [Header("伤害文本组件"), SerializeField] private TMP_Text damageText;
        [Header("伤害数字颜色"), SerializeField] private Color damageColor = Color.white;
        [Header("暴击颜色"), SerializeField] private Color critColor = Color.yellow;

        public void OnGetFromPool()
        { }
        public void OnReturnToPool()
        {
            damageText.text = string.Empty;
            damageText.transform.position = Vector3.zero;
            damageText.gameObject.SetActive(false);
        }
        public void OnDestroyFromPool()
        { }
        public void SetDamageText(int damage, bool isCritical)
        {
            damageText.text = damage.ToString();
            damageText.color = isCritical ? critColor : damageColor;
        }
        public void PlayShowAnimation(Vector3 worldPosition, Action onFinish = null)
        {
            StartCoroutine(PlayShowAnimationCoroutine(worldPosition, onFinish));
        }

        private IEnumerator PlayShowAnimationCoroutine(Vector3 worldPosition, Action onFinish = null)
        {
            damageText.transform.position = worldPosition;
            damageText.gameObject.SetActive(true);
            Vector3 originalScale = damageText.transform.localScale;
            Sequence seq = DOTween.Sequence();
            seq.Append(damageText.transform.DOScale(Vector3.one, 0.25f));
            seq.Append(damageText.transform.DOMove(worldPosition + Vector3.up * 2f, 0.25f));
            seq.Play();
            yield return seq.WaitForCompletion();
            damageText.gameObject.SetActive(false);
            damageText.transform.localScale = originalScale;
            onFinish?.Invoke();
        }
    }
}
