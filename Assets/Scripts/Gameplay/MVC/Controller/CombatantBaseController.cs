using Gameplay.MVC.View;
using Gameplay.MVC.Model;
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

            model.TakeDamage(amount);
            view.RefreshView();
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
