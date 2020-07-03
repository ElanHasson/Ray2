namespace Ray2.Grain.Account.Events
{
    public class TransferCommand : Command<long>
    {
        public TransferCommand() { }
        public TransferCommand(long toAccountId, decimal amount)
        {
            ToAccountId = toAccountId;
            Amount = amount;
        }
        public long ToAccountId { get; set; }
        public decimal Amount { get; set; }
    }
}
