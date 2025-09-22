using AsakiFramework;
using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI
{
    public class HPUI : AsakiMono
    {
        [Header("血条包装器"), SerializeField, NotNullComponent]
        private GameObject Hp;
        [Space]
        [Header("快速响应血条滑块"), SerializeField, NotNullComponent]
        private Slider fastHpBar;
        [Header("慢速响应血条滑块"), SerializeField, NotNullComponent]
        private Slider slowHpBar;
        [Header("血条慢速响应时间"), SerializeField, Range(0.1f, 5f)]
        private float slowHpBarDuration = 0.5f;
        [Space]
        [Header("血条文字"), SerializeField, NotNullComponent]
        private TMP_Text hpText;

        private Coroutine slowHpBarCoroutine;
        private Tween slowHpBarTween;

        private void Awake()
        {
            HasNotNullComponent(fastHpBar);
            HasNotNullComponent(slowHpBar);
            HasNotNullComponent(hpText);
        }

        public void Show() => Hp.SetActive(true);
        public void Hide() => Hp.SetActive(false);

        public void UpdateHpUI(float currentValue, float maxValue)
        {
            if (maxValue == 0f) return; // 避免除以零
            float scaleValue = currentValue / maxValue;
            scaleValue = Mathf.Clamp01(scaleValue); // 避免超过 1
            int hpTextValue = (int)Mathf.Floor(scaleValue * maxValue);
            hpText.text = hpTextValue.ToString();
            fastHpBar.value = scaleValue;
            if (slowHpBarCoroutine != null) CoroutineUtility.StopCoroutine(slowHpBarCoroutine);
            slowHpBarCoroutine = CoroutineUtility.StartCoroutine(
                SlowHpBarChangeCoroutine(scaleValue)
            );
        }

        private IEnumerator SlowHpBarChangeCoroutine(float targetValue)
        {
            slowHpBarTween?.Kill();
            bool completed = false;
            slowHpBarTween = slowHpBar.DOValue(targetValue, slowHpBarDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => completed = true);
            yield return new WaitUntil(() => completed);
        }
    }
}
