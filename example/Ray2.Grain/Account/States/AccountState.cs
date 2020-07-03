using Ray2.Grain.Account.Commands;
using Ray2.Grain.Account.Events;

namespace Ray2.Grain.Account.States
{
    public class AccountState : State<long>
    {
        public void Handle(DepositedEvent @event)
        {
            this.Balance += @event.Amount;
        }

        public void Handle(AccountOpenedEvent @event)
        {
            this.Status = Status.Open;
        }

        public void Handle(TransferredEvent @event)
        {
            this.Balance -= @event.Amount;
        }

        public decimal Balance { get; set; }

        public Status Status { get; set; }
    }

    public enum Status
    {
        Open = 1,
        Closed = 2
    }
}
