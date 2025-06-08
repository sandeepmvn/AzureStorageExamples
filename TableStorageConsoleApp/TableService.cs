using Azure;
using Azure.Data.Tables;
using Azure.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableStorageConsoleApp
{
    //Reference: https://learn.microsoft.com/bs-latn-ba/azure/cosmos-db/table/quickstart-dotnet?toc=https%3A%2F%2Flearn.microsoft.com%2Fbs-latn-ba%2Fazure%2Fstorage%2Ftables%2Ftoc.json&bc=https%3A%2F%2Flearn.microsoft.com%2Fbs-latn-ba%2Fazure%2Fbread%2Ftoc.json
    public record Product : ITableEntity
    {
        public required string RowKey { get; set; }

        public required string PartitionKey { get; set; }

        public required string Name { get; set; }

        public required int Quantity { get; set; }

        public required decimal Price { get; set; }

        public required bool Clearance { get; set; }

        public ETag ETag { get; set; } = ETag.All;

        public DateTimeOffset? Timestamp { get; set; }
    };

    public class TableService
    {
        string _endpoint;
        public TableService(string endpoint)
        {
            _endpoint = endpoint;
        }

        //property to hold the service client
        public TableServiceClient ServiceClient { get; set; }

        // The service client for Azure Table Storage
        public void Intialize()
        {
            // Initialize the table service, e.g., connect to Azure Table Storage
            Console.WriteLine("Table service initialized.");
            DefaultAzureCredential credential = new();

            TableServiceClient serviceClient = new(
                endpoint: new Uri(_endpoint),
                credential
            );

            ServiceClient = serviceClient;
        }

        //property to hold the table client
        public TableClient TableClient { get; set; }
        // Get a list of tables in the service
        public void CreateTableClient(string tableName)
        {
            // Get a reference to a specific table
            TableClient client = ServiceClient.GetTableClient(
     tableName: tableName
 );
            // Create the table if it does not exist
            client.CreateIfNotExists();
            TableClient = client;
            Console.WriteLine($"Table {tableName} is ready for use.");
        }

        //Create an entity
        public async Task CreateEntityAsync(Product entity)
        {
            //await client.AddEntityAsync(product);
           
            Response response = await TableClient.UpsertEntityAsync<Product>(
                entity: entity,
                mode: TableUpdateMode.Replace
            );
            Console.WriteLine($"Entity {entity.RowKey} created in table {TableClient.Name}.");
        }

        // Get an entity by its RowKey and PartitionKey
        public async Task<Product> GetEntityAsync(string rowKey, string partitionKey)
        {
            try
            {
                //                Response<Product> response = await TableClient.GetEntityAsync<Product>(
                //    rowKey: "aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb",
                //    partitionKey: "gear-surf-surfboards"
                //);
                Response<Product> response = await TableClient.GetEntityAsync<Product>(
                    partitionKey: partitionKey,
                    rowKey: rowKey
                );
                Console.WriteLine($"Entity {rowKey} retrieved from table {TableClient.Name}.");
                return response.Value;
            }
            catch (RequestFailedException ex)
            {
                Console.WriteLine($"Error retrieving entity: {ex.Message}");
                return null;
            }
        }
        //Query entities
        public async Task<List<Product>> QueryEntitiesAsync(string partitionKey)
        {
            List<Product> products = new List<Product>();
            try
            {
                // Create a query to filter entities by PartitionKey
                string filter = TableClient.CreateQueryFilter($"PartitionKey eq {partitionKey}");
                AsyncPageable<Product> queryResults = TableClient.QueryAsync<Product>(filter: filter);
                await foreach (Product product in queryResults)
                {
                    products.Add(product);
                }
                Console.WriteLine($"Query completed. Found {products.Count} entities in partition {partitionKey}.");
            }
            catch (RequestFailedException ex)
            {
                Console.WriteLine($"Error querying entities: {ex.Message}");
            }
            return products;
        }

        //Delete an entity
        public async Task DeleteEntityAsync(string rowKey, string partitionKey)
        {
            try
            {
                // Delete the entity by its RowKey and PartitionKey
                await TableClient.DeleteEntityAsync(
                    partitionKey: partitionKey,
                    rowKey: rowKey
                );
                Console.WriteLine($"Entity {rowKey} deleted from table {TableClient.Name}.");
            }
            catch (RequestFailedException ex)
            {
                Console.WriteLine($"Error deleting entity: {ex.Message}");
            }
        }
    }
}
