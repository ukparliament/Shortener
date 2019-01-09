// MIT License
//
// Copyright (c) 2019 UK Parliament
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

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
