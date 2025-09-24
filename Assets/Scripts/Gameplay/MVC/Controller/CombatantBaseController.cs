using AsakiFramework;
using Gameplay.Events;
using Gameplay.MVC.Model;
using Gameplay.MVC.View;
using UnityEditor;
using UnityEngine;

namespace Gameplay.MVC.Controller
{
    public class CombatantBaseController
    {
        private CombatantModel model;
        private CombatantViewBase view;
        public GUID modelId { get; private set; }
        public CombatantBaseController(CombatantModel model, CombatantViewBase view)
        {
            this.model = model;
            this.view = view;
            modelId = model.ModelInstanceID;
            view.BindModel(model);
            view.RefreshView();
        }

        public void TakeDamage(float amount)
        {
            bool isCritical = model.IsCritical();
            if (isCritical)
            {
                amount *= 1.5f;
            }
            model.TakeDamage(amount);
            view.RefreshView();

            if (amount > 0f)
            {
                view.PlayHitAnimation();

                var vfxEvent = new DamageVfxEvent
                {
                    TargetView = view,
                    DamageAmount = Mathf.FloorToInt(amount),
                    IsCritical = isCritical,
                    HitPosition = view.transform.position
                };
                EventBus.Instance.Trigger(vfxEvent);

                // 2. 发布伤害数字事件
                var textEvent = new DamageTextEvent
                {
                    WorldPosition = view.transform.position,
                    DamageAmount = Mathf.FloorToInt(amount),
                    IsCritical = isCritical
                };
                EventBus.Instance.Trigger(textEvent);
            }
        }

        public void Heal(float amount)
        {
            model.Heal(amount);
            view.RefreshView();
        }

        public void ShowView() => view.Show();

        public void HideView() => view.Hide();

        public T GetModel<T>() where T : CombatantModel
        {
            if (model is T specificModel)
                return specificModel;

            return null;
        }

        public T GetView<T>() where T : CombatantViewBase
        {
            if (view is T specificView)
                return specificView;

            return null;
        }

    }
}
