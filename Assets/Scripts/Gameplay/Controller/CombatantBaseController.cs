using Gameplay.View;
using Model;
using UnityEngine;

namespace Gameplay.Controller
{
    public class CombatantBaseController
    {
        private CombatantModel model;
        private CombatantViewBase view;

        public CombatantBaseController(CombatantModel model, CombatantViewBase view)
        {
            this.model = model;
            this.view = view;

            view.BindModel(model);
            // 绑定视图和模型后，刷新视图以显示初始状态
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
