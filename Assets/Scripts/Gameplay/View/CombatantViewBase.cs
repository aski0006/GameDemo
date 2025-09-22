using AsakiFramework;
using Gameplay.UI;
using Gameplay.Model;
using System;
using TMPro;
using UnityEngine;

namespace Gameplay.View
{
    public class CombatantViewBase : AsakiMono
    {
        [NotNullComponent, Header("战斗单位包装器"), SerializeField] public GameObject combatantWrapper;
        [NotNullComponent, Header("战斗单位精灵渲染器"), SerializeField] public SpriteRenderer combatantRenderer;
        [NotNullComponent, Header("战斗单位名称"), SerializeField] public TMP_Text combatantName;
        [NotNullComponent, Header("战斗单位血条"), SerializeField] public HPUI hpUI;

        public Action OnViewShow;
        public Action OnViewHide;

        protected CombatantModel boundModel;
        private void Awake()
        {
            HasNotNullComponent(combatantWrapper);
            HasNotNullComponent(combatantRenderer);
            HasNotNullComponent(combatantName);
            HasNotNullComponent(hpUI);
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

        private void BaseCombatantSetup(Sprite CombatantSprite, string combatantNameText, float currentHp, float maxHp
        )
        {
            combatantRenderer.sprite = CombatantSprite;
            combatantName.text = combatantNameText;
        }
    }
}
