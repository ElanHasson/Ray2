using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Runtime;
using Ray2.Storage;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Ray2.PostgreSQL
{
    public class StateStorage : IStateStorage
    {
        private readonly ConcurrentDictionary<string, IPostgreSqlStateStorage> storageList = new ConcurrentDictionary<string, IPostgreSqlStateStorage>();
        private readonly IServiceProvider _serviceProvider;
        private readonly IPostgreSqlTableStorage _tableStorage;
        private readonly PostgreSqlOptions _options;
        private readonly string _providerName;

        public StateStorage(IServiceProvider serviceProvider, string name)
        {
            this._providerName = name;
            this._serviceProvider = serviceProvider;
            this._tableStorage = serviceProvider.GetRequiredServiceByName<IPostgreSqlTableStorage>(name);
            this._options = serviceProvider.GetRequiredService<IOptionsSnapshot<PostgreSqlOptions>>().Get(name);
        }
        public Task<bool> DeleteAsync(string tableName, object id)
        {
            var stotage = this.GetStorage(tableName, id);
            return stotage.DeleteAsync(id);
        }

        public Task<bool> InsertAsync<TState>(string tableName, object id, TState state) where TState : IState, new()
        {
            var stotage = this.GetStorage(tableName, id);
            return stotage.InsertAsync<TState>(id, state);
        }

        public Task<TState> ReadAsync<TState>(string tableName, object id) where TState : IState, new()
        {
            var stotage = this.GetStorage(tableName, id);
            return stotage.ReadAsync<TState>(id);
        }

        public Task<bool> UpdateAsync<TState>(string tableName, object id, TState state) where TState : IState, new()
        {
            var stotage = this.GetStorage(tableName, id);
            return stotage.UpdateAsync<TState>(id, state);
        }
        private IPostgreSqlStateStorage GetStorage(string tableName, object id)
        {
            return storageList.GetOrAdd(tableName, (key) =>
            {
                this._tableStorage.CreateStateTable(tableName, id);
                return new PostgreSqlStateStorage(this._serviceProvider, _options, this._providerName, key);
            });
        }
    }
}
