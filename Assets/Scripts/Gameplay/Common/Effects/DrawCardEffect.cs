using Gameplay.MVC.Controller;
using Gameplay.GA;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Effects
{
    public class DrawCardEffect : Effect
    {
        [Header("抽卡数量"), SerializeField] private int drawCount = 1;

        public override GameAction GetGameAction(List<CombatantBaseController> targets)
        {
            return new DrawCardGA(drawCount);
        }
    }
}
