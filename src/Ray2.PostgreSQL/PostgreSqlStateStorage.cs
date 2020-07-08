using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Runtime;
using Ray2.Serialization;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Ray2.PostgreSQL
{
    public class PostgreSqlStateStorage : IPostgreSqlStateStorage
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;
        private readonly PostgreSqlOptions _options;
        private readonly string providerName;
        private readonly string tableName;
        private string insertSql;
        private string updateSql;
        private string deleteSql;
        private string selectSql;

        public PostgreSqlStateStorage(IServiceProvider serviceProvider, PostgreSqlOptions options, string name, string tableName)
        {
            this.providerName = name;
            this.tableName = tableName;
            this._serviceProvider = serviceProvider;
            this._options = serviceProvider.GetRequiredService<IOptionsMonitor<PostgreSqlOptions>>().Get(name);
            this._logger = serviceProvider.GetRequiredService<ILogger<PostgreSqlStateStorage>>();
            this.BuildSql(tableName);
        }

        public async Task<bool> DeleteAsync( object id)
        {
            using (var db = PostgreSqlDbContext.Create(this._options))
            {
                return await db.ExecuteAsync(this.deleteSql, new { Id = id.ToString() }) > 0;
            }
        }
        public async Task<bool> InsertAsync<TState>( object id, TState state) where TState : IState, new()
        {
            using (var db = PostgreSqlDbContext.Create(this._options))
            {
                var data = this.GetSerializer().Serialize(state);
                return await db.ExecuteAsync(this.insertSql, new { Id = id.ToString(), Data = data, DataType = this._options.SerializationType }) > 0;
            }
        }
        public async Task<TState> ReadAsync<TState>( object id) where TState : IState, new()
        {
            using (var db = PostgreSqlDbContext.Create(this._options))
            {
                var list = await db.QueryAsync<dynamic>(this.selectSql, new { Id = id.ToString() });
                if (list.Count() == 0)
                {
                    return default(TState);
                }
                var data = list.FirstOrDefault();
                return this.GetSerializer(data.datatype).Deserialize<TState>(data.data);
            }
        }
        public async Task<bool> UpdateAsync<TState>( object id, TState state) where TState : IState, new()
        {
            using (var db = PostgreSqlDbContext.Create(this._options))
            {
                var data = this.GetSerializer().Serialize(state);
                return await db.ExecuteAsync(this.updateSql, new { Id = id.ToString(), Data = data, DataType = this._options.SerializationType }) > 0;
            }
        }
        private ISerializer GetSerializer(string name)
        {
            return this._serviceProvider.GetRequiredServiceByName<ISerializer>(name);
        }
        private ISerializer GetSerializer()
        {
            return this.GetSerializer(this._options.SerializationType);
        }
        private void BuildSql(string tableName)
        {
            this.updateSql = $"Update {tableName} set data=@Data,datatype =@DataType  WHERE id=@Id";
            this.selectSql = $"SELECT data,datatype FROM {tableName} WHERE id=@Id";
            this.insertSql = $"INSERT INTO {tableName}(id,data,datatype) VALUES (@Id,@Data,@DataType)";
            this.deleteSql = $"DELETE FROM {tableName} WHERE id=@Id";
        }
    }
}
