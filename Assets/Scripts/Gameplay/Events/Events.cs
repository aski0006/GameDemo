// DamageEvents.cs

using Gameplay.MVC.Controller;
using Gameplay.MVC.View;
using UnityEngine;

namespace Gameplay.Events
{
    // 基础伤害事件
    public struct DamageEvent
    {
        public CombatantBaseController Attacker;
        public CombatantBaseController Target;
        public int DamageAmount;
        public bool IsCritical;
        public Vector3 HitPosition;
    }

    // 伤害特效事件
    public struct DamageVfxEvent
    {
        public CombatantViewBase TargetView;
        public int DamageAmount;
        public bool IsCritical;
        public Vector3 HitPosition;
    }

    // 伤害数字事件
    public struct DamageTextEvent
    {
        public Vector3 WorldPosition;
        public int DamageAmount;
        public bool IsCritical;
    }
}
