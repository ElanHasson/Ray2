namespace Ray2.Grain.Account
{
    public class TransferredEvent : Event<long>
    {
        public long DestinationAccountId { get; set; }
        public decimal Amount { get; set; }

        public TransferredEvent()
        {
            
        }


        public TransferredEvent(long destinationAccountId, decimal amount)
        {
            DestinationAccountId = destinationAccountId;
            Amount = amount;
        }
    }
}