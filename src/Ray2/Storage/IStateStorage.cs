﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ray2.Storage
{
    public interface IStateStorage: IStorage
    {
        Task<TState> ReadAsync<TState>(string tableName, object id) where TState : IState, new();
        Task<bool> InsertAsync<TState>(string tableName, object id, TState state) where TState : IState, new();
        Task<bool> UpdateAsync<TState>(string tableName, object id, TState state) where TState : IState, new();
        Task<bool> DeleteAsync(string tableName, object id);
    }
}
