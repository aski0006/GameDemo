using Gameplay.MVC.Controller;
using Gameplay.GA;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Effects
{
    public class DrawCardEffect : Effect
    {
        [Header("抽卡数量"), SerializeField] private int drawCount = 1;
        
        public override Type ActionType => typeof(DrawCardGA);
        public override GameAction GetGameAction(List<CombatantBaseController> targets, CombatantBaseController caster = null)
        {
            return new DrawCardGA(drawCount);
        }
    }
}
