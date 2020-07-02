﻿using Ray2.Configuration;
using Ray2.Storage;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ray2.EventSource
{
    /// <summary>
    /// This is the event source interface
    /// </summary>
    public interface IEventSourcing
    {
        EventSourceOptions Options { get; set; }
        Task<IList<IEvent>> GetListAsync(EventQueryModel queryModel);
        Task ClearSnapshotAsync(string id);
    }

    /// <summary>
    /// This is the event source interface
    /// </summary>
    public interface IEventSourcing<TState, TStateKey> : IEventSourcing
        where TState : IState<TStateKey>, new()
    {

        Task<IEventSourcing<TState, TStateKey>> Init(TStateKey stateKey);
        Task<bool> SaveAsync(IEvent<TStateKey> @event);
        Task<bool> SaveAsync(IList<IEvent<TStateKey>> events);
        new Task<IList<IEvent<TStateKey>>> GetListAsync(EventQueryModel queryModel);

        Task<TState> ReadSnapshotAsync();
        Task SaveSnapshotAsync(TState state);
        Task ClearSnapshotAsync();

    }



}
