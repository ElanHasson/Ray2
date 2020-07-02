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

        public AccountFlow(ILogger<AccountFlow> logger)
        {
            this.logger = logger;
        }

        protected override long Id => this.GetPrimaryKeyLong();


        public async Task Apply(TransferCommand evt)
        {

            var toActor = GrainFactory.GetGrain<IAccount>(evt.ToAccountId);
            await toActor.Transfer(evt.ToAccountId, evt.Amount);
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