namespace Ray2.Grain.Account.Events
{
    public class DepositedEvent : Event<long>
    {
        public DepositedEvent() { }
        public DepositedEvent(decimal amount)
        {
            Amount = amount;
        }
        public decimal Amount { get; set; }

    }
}