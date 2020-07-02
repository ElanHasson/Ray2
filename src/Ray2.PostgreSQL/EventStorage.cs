﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Runtime;
using Ray2.EventSource;
using Ray2.Internal;
using Ray2.Storage;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ray2.PostgreSQL
{
    public class EventStorage : IEventStorage
    {
        private readonly ConcurrentDictionary<string, IPostgreSqlEventStorage> storageList = new ConcurrentDictionary<string, IPostgreSqlEventStorage>();
        private readonly IServiceProvider _serviceProvider;
        private readonly IPostgreSqlTableStorage _tableStorage;
        private readonly PostgreSqlOptions _options;
        private readonly string _providerName;
        public EventStorage(IServiceProvider serviceProvider, string name)
        {
            this._providerName = name;
            this._serviceProvider = serviceProvider;
            this._tableStorage = serviceProvider.GetRequiredServiceByName<IPostgreSqlTableStorage>(name);
            this._options = serviceProvider.GetRequiredService<IOptionsSnapshot<PostgreSqlOptions>>().Get(name);
        }

        public Task<EventModel> GetAsync(string tableName, object id, long version)
        {
            var stotage = this.GetStorage(tableName, id);
            return stotage.GetAsync(id, version);
        }

        public Task<List<EventModel>> GetListAsync(string tableName, EventQueryModel queryModel)
        {
            var stotage = this.GetStorage(tableName, queryModel.Id);
            return stotage.GetListAsync(queryModel);
        }

        public Task SaveAsync(List<IDataflowBufferWrap<EventStorageModel>> wrapList)
        {
            Dictionary<string, List<IDataflowBufferWrap<EventStorageModel>>> eventsList = wrapList.GroupBy(f => f.Data.StorageTableName).ToDictionary(x => x.Key, v => v.ToList());
            foreach (var key in eventsList.Keys)
            {
                var events = eventsList[key];
                var stotage = this.GetStorage(key, events.First().Data.Id);
                stotage.SaveAsync(events);
            }
            return Task.CompletedTask;
        }

        public Task<bool> SaveAsync(EventCollectionStorageModel events)
        {
            var stotage = this.GetStorage(events.StorageTableName, events.GetId());
            return stotage.SaveAsync(events);
        }

        private IPostgreSqlEventStorage GetStorage(string tableName, object id)
        {
            return storageList.GetOrAdd(tableName, (key) =>
            {
                this._tableStorage.CreateEventTable(tableName, id);
                return new PostgreSqlEventStorage(this._serviceProvider, _options, this._providerName, key);
            });
        }
    }
}
