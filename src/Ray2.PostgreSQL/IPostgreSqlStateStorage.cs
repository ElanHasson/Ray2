using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ray2.PostgreSQL
{
   public interface IPostgreSqlStateStorage
    {
        Task<bool> DeleteAsync(object id);
        Task<bool> InsertAsync<TState>(object id, TState state) where TState : IState, new();
        Task<TState> ReadAsync<TState>(object id) where TState : IState, new();
        Task<bool> UpdateAsync<TState>(object id, TState state) where TState : IState, new();
    }
}
