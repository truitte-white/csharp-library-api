using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LibraryAPI.Helpers
{
    public interface IDbHelper
    {
        Task<T?> FindOne<T>(string tableName, Dictionary<string, object> filter, Dictionary<string, object>? options = null) where T : class;
        Task<List<T>> FindAll<T>(string tableName, Dictionary<string, object>? filter = null, Dictionary<string, object>? options = null) where T : class;
        Task<int> Create<T>(string tableName, T entity) where T : class;
        Task<int> Update<T>(string tableName, object updatedBody, Dictionary<string, object> filter) where T : class;
    }
}
