using AsakiFramework;

namespace Gameplay.GA
{
    public class DrawCardGA : GameAction
    {
        public int DrawAmount { get; set; }
        public DrawCardGA(int drawAmount)
        {
            DrawAmount = drawAmount;
        }
    }
}
