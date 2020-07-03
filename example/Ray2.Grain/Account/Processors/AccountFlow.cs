using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using Ray2.Grain.Account.Events;
using Ray2.Grain.Account.States;

namespace Ray2.Grain.Account.Processors
{
    [EventProcessor(typeof(Account), "rabbitmq", "postgresql", OnceProcessCount = 2000)]
    public class AccountFlow : RayProcessorGrain<AccountState, long>
    {
        private readonly ILogger logger;
        private readonly IGrainFactory _grainFactory;

        public AccountFlow(ILogger<AccountFlow> logger, IGrainFactory grainFactory)
        {
            this.logger = logger;
            _grainFactory = grainFactory;
        }

        protected override long Id => this.GetPrimaryKeyLong();


        public async Task Apply(TransferredEvent evt)
        {
            var targetAccount = this.GrainFactory.GetGrain<IAccount>(evt.DestinationAccountId);
            await targetAccount.Deposit(new DepositCommand(evt.Amount)
            {
                RelationEvent = evt.GetRelationKey()
            });
        }

        public Task Apply(AccountOpenedEvent evt)
        {
            //this.logger.LogError($"加款：{evt.Amount}; 余额：{evt.Balance}");
            return Task.CompletedTask;
        }

        public Task Apply(DepositedEvent evt)
        {
            //this.logger.LogError($"加款：{evt.Amount}; 余额：{evt.Balance}");
            return Task.CompletedTask;
        }
    }
}