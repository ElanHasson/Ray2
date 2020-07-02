namespace Ray2.Grain.Account.Events
{
    public class TransferCommand : Event<long>
    {
        public TransferCommand() { }
        public TransferCommand(long toAccountId, decimal amount, decimal balance)
        {
            ToAccountId = toAccountId;
            Amount = amount;
            Balance = balance;
        }
        public long ToAccountId { get; set; }
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
     
    }
}
