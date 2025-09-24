using Gameplay.Data;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Gameplay.MVC.Interfaces
{
    public enum CombatantType
    {
        Hero,
        Enemy,
    }

    public interface ICombatantReadOnly : IModel
    {
        public Sprite Sprite { get; }
        public string Name { get; }
        public float MaxHp { get; }
        public List<CardData> HoldCard { get; }
        public float CurrentHp { get; set; }
        public bool IsDead { get; }
        public CombatantType combatantType { get; set; }
    }

    public interface IHeroCombatant : ICombatantReadOnly
    {
        
    }

    public interface IEnemyCombatant : ICombatantReadOnly
    {
        public int CurrentAtk { get; set; }
        public int CurrentDef { get; set; }
    }
}
