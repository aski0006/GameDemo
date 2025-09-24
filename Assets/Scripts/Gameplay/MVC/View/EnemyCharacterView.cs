using TMPro;
using UnityEngine;
using Gameplay.MVC.Model;

namespace Gameplay.MVC.View
{
    public class EnemyCharacterView : CombatantViewBase
    {
        [Header("敌人属性")]
        [SerializeField] private TMP_Text attackText;
        [SerializeField] private TMP_Text defenseText;
        
        /// <summary>
        /// 刷新攻击力 / 防御力文本
        /// </summary>
        private void RefreshAtkDef(int atk, int def)
        {
            if (attackText) attackText.text = atk.ToString();
            if (defenseText) defenseText.text = def.ToString();
        }

        /* 如果希望每次模型数值变化时自动刷新，可重写 RefreshView */
        public override void RefreshView()
        {
            base.RefreshView();
            if (boundModel is EnemyCharacterModel enemy)
                RefreshAtkDef(enemy.CurrentAtk, enemy.CurrentDef);
        }
    }
}
