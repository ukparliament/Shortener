namespace Shortener
{
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    public class AzureTableStorageService : IStorageService
    {
        public CloudTable table { get; }

        public AzureTableStorageService(IConfiguration config)
        {
            var serviceConfig = config.GetSection("StorageService");

            var storageAccount = CloudStorageAccount.Parse(serviceConfig["ConnectionString"]);
            var tableClient = storageAccount.CreateCloudTableClient();

            table = tableClient.GetTableReference(serviceConfig["TableName"]);
        }

        public async Task<string> GetValue(string id)
        {
            var retrieveOperation = TableOperation.Retrieve<ShortUrlEntity>(string.Empty, id);
            var retrievedResult = await table.ExecuteAsync(retrieveOperation);

            return (retrievedResult.Result as ShortUrlEntity)?.Url;
        }

        public async Task<bool> ContainsKey(string id)
        {
            return !(await GetValue(id) is null);
        }

        public async Task Add(string id, string url)
        {
            var entity = new ShortUrlEntity
            {
                PartitionKey = string.Empty,
                RowKey = id,
                Url = url
            };
            var retrieveOperation = TableOperation.Insert(entity);

            await table.ExecuteAsync(retrieveOperation);
        }
    }
}
