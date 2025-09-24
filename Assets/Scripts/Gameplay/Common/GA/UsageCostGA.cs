namespace Gameplay.GA
{
    public class UsageCostGA : GameAction
    {
        public int UsageCostAmount { get; private set; }
        public UsageCostGA(int usageCost)
        {
            UsageCostAmount = usageCost;
        }
    }
}
