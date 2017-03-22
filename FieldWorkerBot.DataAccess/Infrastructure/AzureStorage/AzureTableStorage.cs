using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace FieldWorkerBot.DataAccess.Infrastructure.AzureStorage
{
    /// <summary>
    ///     AzureTableStorage class
    /// </summary>
    public class AzureTableStorage : ITableStorage
    {
        private readonly CloudTableClient _client;
        private readonly string _tableName;

        private CloudTable _table;
        private bool _tableExits;
        private string _partitionKey;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AzureTableStorage" /> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="tableName">Name of the table.</param>
        public AzureTableStorage(string connectionString, string tableName, string partitionKey)
        {
            // TODO: add logger
            CloudStorageAccount account = CloudStorageAccount.Parse(connectionString);
            _client = account.CreateCloudTableClient();
            _tableName = tableName;
            _partitionKey = partitionKey;
        }

        /// <summary>
        ///     Gets the entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="rowKey">The row key.</param>
        /// <returns></returns>
        public async Task<T> GetAsync<T>(string partitionKey, string rowKey) where T : class, ITableEntity
        {
            EnsureTable();

            TableOperation retrieveOperation = TableOperation.Retrieve<T>(partitionKey, rowKey);

            TableResult result = await _table.ExecuteAsync(retrieveOperation);

            return result.Result as T;
        }

        /// <summary>
        ///     Gets the entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rowKey">The row key.</param>
        /// <returns></returns>
        public async Task<T> GetAsync<T>(string rowKey) where T : class, ITableEntity
        {
            EnsureTable();

            TableOperation retrieveOperation = TableOperation.Retrieve<T>(_partitionKey, rowKey);

            TableResult result = await _table.ExecuteAsync(retrieveOperation);

            return result.Result as T;
        }

        /// <summary>
        ///     Adds the or update entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public async Task<object> AddOrUpdateAsync(ITableEntity entity)
        {
            EnsureTable();

            TableOperation insertOrReplaceOperation = TableOperation.InsertOrReplace(entity);

            TableResult result = await _table.ExecuteAsync(insertOrReplaceOperation);

            return result.Result;
        }

        /// <summary>
        ///     Deletes the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public async Task<object> DeleteAsync(ITableEntity entity)
        {
            EnsureTable();

            TableOperation deleteOperation = TableOperation.Delete(entity);

            TableResult result = await _table.ExecuteAsync(deleteOperation);

            return result.Result;
        }

        /// <summary>
        ///     Ensures existance of the table.
        /// </summary>
        private void EnsureTable()
        {
            if (!_tableExits)
            {
                _table = _client.GetTableReference(_tableName);
                _table.CreateIfNotExists();
                _tableExits = true;
            }
        }
    }
}