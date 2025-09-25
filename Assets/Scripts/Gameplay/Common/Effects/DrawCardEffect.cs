using Gameplay.MVC.Controller;
using Gameplay.GA;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Effects
{
    public class DrawCardEffect : Effect
    {
        [Header("抽卡数量"), SerializeField] private int drawCount = 1;

        public override GameAction GetGameAction(List<CombatantBaseController> targets, CombatantBaseController caster = null)
        {
            // 抽卡不需要 caster，直接返回
            return new DrawCardGA(drawCount);
        }
    }
}
