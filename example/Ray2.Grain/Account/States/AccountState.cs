using Ray2.Grain.Account.Commands;
using Ray2.Grain.Account.Events;

namespace Ray2.Grain.Account.States
{
    public class AccountState : State<long>
    {
        public void Handle(DepositedEvent evt)
        {
            this.Balance += evt.Amount;
        }

        public void Handle(AccountOpenedEvent evt)
        {
            this.Status = Status.Open;
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
