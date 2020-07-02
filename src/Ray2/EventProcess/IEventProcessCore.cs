﻿using Ray2.Configuration;
using Ray2.EventSource;
using System;
using System.Threading.Tasks;

namespace Ray2.EventProcess
{
    using EventProcessor = Func<IEvent, Task>;
    public interface IEventProcessCore
    {
        EventProcessOptions Options { get; set; }
        Task<IEventProcessCore> Init(EventProcessor eventProcessor);
        Task<bool> Tell(EventModel model);
    }


    public interface IEventProcessCore<TState, TStateKey> : IEventProcessCore
    where TState : IState<TStateKey>, new()
    {
        Task<IEventProcessCore<TState, TStateKey>> Init(TStateKey id, EventProcessor eventProcessor);
        Task SaveStateAsync();
        Task<TState> ReadStateAsync();
        Task ClearStateAsync();
    }
}
