using Gameplay.MVC.Controller;
using Gameplay.GA;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Effects
{
    public class DealDamageEffect : Effect
    {
        [Header("伤害值"), SerializeField] private float demage = 10;

        // 覆写：明确该 Effect 返回的 GameAction 类型是 DealDamageGA
        public override Type ActionType => typeof(DealDamageGA);

        public override GameAction GetGameAction(List<CombatantBaseController> targets, CombatantBaseController caster = null)
        {
            return new DealDamageGA(demage, targets, caster);
        }
    }
}
