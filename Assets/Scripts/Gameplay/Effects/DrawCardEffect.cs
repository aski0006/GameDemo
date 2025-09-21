using Gameplay.GA;
using UnityEngine;

namespace Gameplay.Effects
{
    public class DrawCardEffect : Effect
    {
        [Header("抽卡数量"), SerializeField] private int drawCount = 1;
        public override GameAction GetGameAction()
        {
            return new DrawCardGA(drawCount);
        }
    }
}
