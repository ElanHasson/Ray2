using System;

namespace Ray2.Grain.Account.Events
{
    public class DepositCommand : Command<long>
    {
        public decimal Amount { get;  set; }

        public DepositCommand(decimal amount)
        {
            Amount = amount;
        }
        public DepositCommand(Guid commandId):base(commandId)
        { }
    }
}
