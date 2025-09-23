using AsakiFramework;
using AsakiFramework.ObjectPool;
using DG.Tweening;
using Gameplay.UI;
using Gameplay.Model;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Gameplay.View
{
    public class CombatantViewBase : AsakiMono, IPoolable
    {
        [NotNullComponent, Header("战斗单位包装器"), SerializeField] public GameObject combatantWrapper;
        [NotNullComponent, Header("战斗单位精灵渲染器"), SerializeField] public SpriteRenderer combatantRenderer;
        [NotNullComponent, Header("战斗单位名称"), SerializeField] public TMP_Text combatantName;
        [NotNullComponent, Header("战斗单位血条"), SerializeField] public HPUI hpUI;

        public Action OnViewShow;
        public Action OnViewHide;
        private Vector3 originalScale;

        protected CombatantModel boundModel;
        private void Awake()
        {
            HasNotNullComponent(combatantWrapper);
            HasNotNullComponent(combatantRenderer);
            HasNotNullComponent(combatantName);
            HasNotNullComponent(hpUI);
            originalScale = combatantWrapper.transform.localScale;
        }

        public void Show()
        {
            combatantWrapper.SetActive(true);
            hpUI.Show();
            OnViewShow?.Invoke();
        }

        public void Hide()
        {
            combatantWrapper.SetActive(false);
            hpUI.Hide();
            OnViewHide?.Invoke();
        }

        public void BindModel(CombatantModel model)
        {
            if (model == null)
            {
                return;
            }
            boundModel = model;
            BaseCombatantSetup(
                boundModel.Sprite,
                boundModel.Name,
                boundModel.CurrentHp,
                boundModel.MaxHp
            );
        }

        public virtual void RefreshView()
        {
            hpUI.UpdateHpUI(boundModel.CurrentHp, boundModel.MaxHp);
        }

        private void BaseCombatantSetup(Sprite CombatantSprite, string combatantNameText, float currentHp, float maxHp)
        {
            combatantRenderer.sprite = CombatantSprite;
            combatantName.text = combatantNameText;
        }

        public void PlayDeathAnimation(float duration = 0.5f, Action onFinish = null)
        {
            StartCoroutine(OnCombatantDeathAnimationCoroutine(duration, onFinish));
        }

        private IEnumerator OnCombatantDeathAnimationCoroutine(float duration = 0.5f, Action onFinish = null)
        {
            originalScale = combatantWrapper.transform.localScale;
            Tween t = combatantWrapper.transform.DOScale(Vector3.zero, duration);
            yield return t.WaitForCompletion();
            onFinish?.Invoke();
        }
        public void OnGetFromPool()
        {
            combatantWrapper.transform.localScale = originalScale;
            combatantWrapper.SetActive(true);
        }
        public void OnReturnToPool()
        {
            combatantWrapper.SetActive(false);
        }
        public void OnDestroyFromPool()
        { }
    }
}
