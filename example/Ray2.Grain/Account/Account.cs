using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using Ray2.Grain.Account.Commands;
using Ray2.Grain.Account.Events;
using Ray2.Grain.Account.Exceptions;
using Ray2.Grain.Account.States;

namespace Ray2.Grain.Account
{
    [EventSourcing("postgresql", "rabbitmq")]
    public class Account : RayGrain<AccountState, long>, IAccount
    {
        public Account(ILogger<Account> logger) : base(logger)
        {

        }

        protected override long Id => this.GetPrimaryKeyLong();


        /// <summary>
        /// The Command Handler.
        /// This method restricts execution to itself only as it should ever allow a single concurrent caller at a time.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public Task Open(OpenAccountCommand command)
        {
            /*
             * 1. Validate command upon it's own merits (required fields present, version is still allowed, etc)
             * 2. Validate command against the current state
             * 3. Apply command
             * 4. Write Events
             */
            
            if (this.IsOpen())
            {
                throw new AccountAlreadyOpenException();
            }

            if (this.IsClosed())
            {
                throw new CannotOpenAClosedAccount();
            }
            
            return this.WriteAsync(new AccountOpenedEvent(), MQ.MQPublishType.Asynchronous);
        }

        private bool IsOpen()
        {
            return this.State.Status == Status.Open;
        }
        private bool IsClosed()
        {
            return this.State.Status == Status.Closed;
        }
        /// <summary>
        /// The Command Handler.
        /// This Allows interleaving of processing because multiple deposits can happen concurrently
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public  Task Deposit(DepositCommand command)
        {
            if (this.IsClosed())
            {
                throw new UnableToDepositToClosedAccountException();
            }

            return  this.ConcurrentWriteAsync(new DepositedEvent(command.Amount), MQ.MQPublishType.Asynchronous);
        }

        public Task<decimal> GetBalance()
        {
            return Task.FromResult(State.Balance);
        }

        public Task Transfer(long toAccountId, decimal amount)
        {
            var evt = new TransferCommand(toAccountId, amount, State.Balance - amount);
            evt.RelationEvent = evt.GetRelationKey();
            return base.WriteAsync(evt);
        }
    }
}
