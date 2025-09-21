namespace Gameplay.GA
{
    public class RefillCostGA : GameAction
    {
        public int RefillCostAmount { get; private set; }
        public RefillCostGA(int refillCostAmount)
        {
            RefillCostAmount = refillCostAmount;
        }
    }
}
