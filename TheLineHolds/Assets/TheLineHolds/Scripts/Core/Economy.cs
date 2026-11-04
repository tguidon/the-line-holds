using System;
namespace TheLineHolds
{
    // The only owner of match funds; callers validate an order before asking to spend.
    public sealed class Economy
    {
        public int Funds { get; private set; }
        public Economy(int startingFunds) { Funds = startingFunds; }
        public bool TrySpend(int cost)
        {
            if (cost < 0 || cost > Funds) return false;
            Funds -= cost;
            return true;
        }
        public void Credit(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            Funds += amount;
        }
    }
}
