﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ray2.Configuration;
using Ray2.Internal;
using Ray2.Storage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Ray2.EventSource
{
    public class EventSourcing<TState, TStateKey> : EventSourcing, IEventSourcing<TState, TStateKey> where TState : IState<TStateKey>, new()
    {
        private IStorageFactory _storageFactory;
        private TStateKey Id;
        private IDataflowBufferBlock<EventStorageModel> _eventBufferBlock;
        private IEventStorage _eventStorage;
        private IStateStorage _snapshotStorage;
        private string SnapshotTable;

        public EventSourcing(IServiceProvider serviceProvider, ILogger<EventSourcing<TState, TStateKey>> logger)
            : base(serviceProvider, logger)
        {
        }
        public async Task<IEventSourcing<TState, TStateKey>> Init(TStateKey stateKey)
        {
            this.Id = stateKey;
            this._storageFactory = new StorageFactory(this._serviceProvider, this.Options.StorageOptions);
            this._eventStorage = await this._storageFactory.GetEventStorage(this.Options.EventSourceName, this.Id.ToString());

            string bufferKey = "es" + this.Options.EventSourceName + this.Options.StorageOptions.StorageProvider;
            this._eventBufferBlock = this._serviceProvider.GetRequiredService<IDataflowBufferBlockFactory>().Create<EventStorageModel>(bufferKey, this.LazySaveAsync);

            //Get snapshot storage information
            if (this.Options.SnapshotOptions.SnapshotType != SnapshotType.NoSnapshot)
            {
                IStorageFactory snapshotStorageFactory = new StorageFactory(this._serviceProvider, this.Options.SnapshotOptions);
                this._snapshotStorage = await snapshotStorageFactory.GetStateStorage(this.Options.EventSourceName, StorageType.EventSourceSnapshot, this.Id.ToString());
                this.SnapshotTable = await snapshotStorageFactory.GetTable(this.Options.EventSourceName, StorageType.EventSourceSnapshot, this.Id.ToString());
            }
            return this;
        }
        public async Task<bool> SaveAsync(IEvent<TStateKey> @event)
        {
            //Sharding processing
            string storageTableName = await this.GetEventTableName();
            EventStorageModel storageModel = new EventStorageModel(@event.Id, @event, this.Options.EventSourceName, storageTableName);
            return await this._eventBufferBlock.SendAsync(storageModel);
        }
        public async Task<bool> SaveAsync(IList<IEvent<TStateKey>> events)
        {
            if (events.Count == 0)
                return true;
            string storageTableName = await this.GetEventTableName();
            EventCollectionStorageModel storageModel = new EventCollectionStorageModel(this.Options.EventSourceName, storageTableName);
            foreach (var e in events)
            {
                EventModel eventModel = new EventModel(e);
                storageModel.Events.Add(eventModel);
            }
            return await this._eventStorage.SaveAsync(storageModel);
        }
        public async Task LazySaveAsync(BufferBlock<IDataflowBufferWrap<EventStorageModel>> eventBuferr)
        {
            var bufferWrapList = new List<IDataflowBufferWrap<EventStorageModel>>();
            while (eventBuferr.TryReceive(out var wrap))
            {
                bufferWrapList.Add(wrap);
                if (bufferWrapList.Count >= 500) break;//Process up to 500 items at a time
            }
            try
            {
                await _eventStorage.SaveAsync(bufferWrapList);
            }
            catch (Exception ex)
            {
                bufferWrapList.ForEach(f => f.ExceptionHandler(ex));
            }
        }
        private async Task<string> GetEventTableName()
        {
            return await this._storageFactory.GetTable(this.Options.EventSourceName, StorageType.EventSource, this.Id.ToString());
        }
        public async new Task<IList<IEvent<TStateKey>>> GetListAsync(EventQueryModel queryModel)
        {
            queryModel.Id = this.Id.ToString();
            List<string> tables = await this._storageFactory.GetTableList(this.Options.EventSourceName, StorageType.EventSource, this.Id.ToString(), queryModel.StartTime);
            List<IEvent<TStateKey>> events = new List<IEvent<TStateKey>>();
            foreach (var t in tables)
            {
                var eventModels = await _eventStorage.GetListAsync(t, queryModel);
                var _events = this.ConvertEvent<TStateKey>(eventModels);
                events.AddRange(_events);
                if (queryModel.Limit > 0)
                {
                    if (events.Count >= queryModel.Limit)
                        break;
                }
            }
            return events;
        }

        public async Task<TState> ReadSnapshotAsync()
        {
            var state = new TState { Id = this.Id };
            if (this.Options.SnapshotOptions.SnapshotType != SnapshotType.NoSnapshot)
            {
                var st = await this._snapshotStorage.ReadAsync<TState>(this.SnapshotTable, this.Id);
                if (st != null)
                {
                    state = st;
                }
                else
                {
                    if (state.Version == 0)
                    {
                        //Initialize data first
                        await this.SaveSnapshotAsync(state);
                    }
                }
            }
            //Get current event
            IList<IEvent<TStateKey>> events = await this.GetListAsync(new EventQueryModel(state.Version));
            if (events == null || events.Count == 0)
                return state;

            state.Player(events);
            await this.SaveSnapshotAsync(state); //save snapshot
            return state;
        }
        public async Task ClearSnapshotAsync()
        {
            if (this.Options.SnapshotOptions.SnapshotType == SnapshotType.NoSnapshot)
                return;

            await this._snapshotStorage.DeleteAsync(this.SnapshotTable, this.Id);
        }
        public async Task SaveSnapshotAsync(TState state)
        {
            if (this.Options.SnapshotOptions.SnapshotType == SnapshotType.NoSnapshot)
                return;
            try
            {
                if (state.Version == 0)
                    await this._snapshotStorage.InsertAsync(this.SnapshotTable, state.Id, state);
                else
                    await this._snapshotStorage.UpdateAsync(this.SnapshotTable, state.Id, state);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, $"SaveSnapshotAsync {nameof(TState)}:Id= {state.Id} failure ;");
                return;
            }
        }

    }

    public class EventSourcing : IEventSourcing
    {
        protected readonly IServiceProvider _serviceProvider;
        protected readonly ILogger _logger;

        public EventSourcing(IServiceProvider serviceProvider, ILogger<EventSourcing> logger)
        {
            this._serviceProvider = serviceProvider;
            this._logger = logger;
        }
        public EventSourceOptions Options { get; set; }

        public virtual async Task ClearSnapshotAsync(string id)
        {
            IStorageFactory storageFactory = new StorageFactory(this._serviceProvider, Options.SnapshotOptions);
            var storageTable = await storageFactory.GetTable(this.Options.EventSourceName, StorageType.EventSourceSnapshot, id);
            var storage = await storageFactory.GetStateStorage(this.Options.EventSourceName, StorageType.EventSourceSnapshot, id);
            await storage.DeleteAsync(storageTable, id);
        }

        public virtual async Task<IList<IEvent>> GetListAsync(EventQueryModel queryModel)
        {
            IStorageFactory storageFactory = new StorageFactory(this._serviceProvider, Options.StorageOptions);
            var storages = await storageFactory.GetEventStorageList(this.Options.EventSourceName, queryModel.StartTime);
            List<IEvent> events = new List<IEvent>();
            foreach (var storage in storages)
            {
                foreach (var table in storage.Tables)
                {
                    var eventModels = await storage.Storage.GetListAsync(table, queryModel);
                    var _events = this.ConvertEvent(eventModels);
                    events.AddRange(_events);
                    if (queryModel.Limit > 0)
                    {
                        if (events.Count >= queryModel.Limit)
                            break;
                    }
                }
            }
            return events;
        }

        public List<IEvent> ConvertEvent(IList<EventModel> eventModels)
        {
            List<IEvent> events = new List<IEvent>();
            if (eventModels == null || eventModels.Count == 0)
                return new List<IEvent>();

            foreach (var model in eventModels)
            {
                events.Add(model.Event);
            }
            return events;
        }
        public List<IEvent<TStateKey>> ConvertEvent<TStateKey>(IList<EventModel> eventModels)
        {
            List<IEvent<TStateKey>> events = new List<IEvent<TStateKey>>();
            if (eventModels == null || eventModels.Count == 0)
                return events;

            foreach (var model in eventModels)
            {
                if (model.Event is IEvent<TStateKey> @event)
                {
                    events.Add(@event);
                }
                else
                {
                    this._logger.LogWarning($"{model.TypeCode}.{model.Version}  not equal to {typeof(IEvent<TStateKey>).Name}");
                    continue;
                }
            }
            return events;
        }
    }
}
