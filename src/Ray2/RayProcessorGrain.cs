using Orleans;
using Ray2.EventProcess;
using Ray2.EventSource;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Ray2
{
    /// <summary>
    /// This is the event process grain
    /// </summary>
    public abstract class RayProcessorGrain<TState, TStateKey> : Grain, IEventProcessor
        where TState : IState<TStateKey>, new()
    {
        private IEventProcessCore<TState, TStateKey> _eventProcessCore;

        protected TState State
        {
            get { return _eventProcessCore.ReadStateAsync().GetAwaiter().GetResult(); }
        }

        protected abstract TStateKey Id { get; }

        public override async Task OnActivateAsync()
        {
            this.OrleansScheduler = TaskScheduler.Current;
            this._eventProcessCore = await this.ServiceProvider.GetEventProcessCore<TState, TStateKey>(this)
                .Init(this.Id, this.OnEventProcessing);
            await base.OnActivateAsync();
        }

        protected TaskScheduler OrleansScheduler { get; set; }

        public override async Task OnDeactivateAsync()
        {
            await this._eventProcessCore.SaveStateAsync();
            await base.OnDeactivateAsync();
        }

        public Task<bool> Tell(EventModel model)
        {
            return this._eventProcessCore.Tell(model);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual Task OnEventProcessing(IEvent @event)
        {
            return Task.Factory.StartNew<Task>(() =>
            {
                dynamic s = this;
                dynamic e = @event;
                return s.Apply(e);
            },
                CancellationToken.None, 
                TaskCreationOptions.None, this.OrleansScheduler).Unwrap();
        }
    }

    public abstract class RayProcessorGrain<TStateKey> : RayProcessorGrain<EventProcessState<TStateKey>, TStateKey>
    {
    }
}