using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace FieldWorkerBot.DataAccess.Infrastructure.AzureStorage
{
    /// <summary>
    ///     ITableStorage interface
    /// </summary>
    public interface ITableStorage
    {
        Task<object> AddOrUpdateAsync(ITableEntity entity);
        Task<T> GetAsync<T>(string partitionKey, string rowKey) where T : class, ITableEntity;
        Task<T> GetAsync<T>(string rowKey) where T : class, ITableEntity;
        Task<object> DeleteAsync(ITableEntity entity);
    }
}