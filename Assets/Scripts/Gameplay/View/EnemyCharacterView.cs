using TMPro;
using UnityEngine;
using Model;

namespace Gameplay.View
{
    public class EnemyCharacterView : CombatantVIewBase
    {
        [Header("敌人属性")]
        [SerializeField] private TMP_Text attackText;
        [SerializeField] private TMP_Text defenseText;

        /// <summary>
        /// 外部调用：把 EnemyCharacter 数据绑定到视图
        /// </summary>
        public void BindModel(EnemyCharacter model)
        {
            base.BindModel(model);          // 刷新血条、头像、名字
            RefreshAtkDef(model.CurrentAtk, model.CurrentDef);
        }

        /// <summary>
        /// 刷新攻击力 / 防御力文本
        /// </summary>
        private void RefreshAtkDef(int atk, int def)
        {
            if (attackText)  attackText.text  = $"ATK: {atk}";
            if (defenseText) defenseText.text = $"DEF: {def}";
        }

        /* 如果希望每次模型数值变化时自动刷新，可重写 RefreshView */
        public override void RefreshView()
        {
            base.RefreshView();
            if (boundModel is EnemyCharacter enemy)
                RefreshAtkDef(enemy.CurrentAtk, enemy.CurrentDef);
        }
    }
}
