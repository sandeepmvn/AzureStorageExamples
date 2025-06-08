// See https://aka.ms/new-console-template for more information
using TableStorageConsoleApp;

Console.WriteLine("Hello, World!");
// Create a new instance of the TableService with the Azure Table Storage endpoint
TableService tableService = new TableService("https://azurestoragesa20251.table.core.windows.net");
// Initialize the table service
tableService.Intialize();
// Get a reference to a specific table, creating it if it does not exist
tableService.CreateTableClient("ProductsTest");
//Product entity = new()
//{
//    RowKey = "aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb",
//    PartitionKey = "gear-surf-surfboards",
//    Name = "Surfboard",
//    Quantity = 10,
//    Price = 300.00m,
//    Clearance = true
//};
var entity = new Product
{
    RowKey = Guid.NewGuid().ToString(),
    PartitionKey = "gear-surf-surfboards",
    Name = "Surfboard",
    Quantity = 10,
    Price = 300.00m,
    Clearance = true
};

// Create an entity in the table
await tableService.CreateEntityAsync(entity);

// Retrieve the entity by its RowKey and PartitionKey
var data =await tableService.GetEntityAsync(entity.RowKey, entity.PartitionKey);

Console.WriteLine($"Retrieved entity: {data.Name}, Quantity: {data.Quantity}, Price: {data.Price}, Clearance: {data.Clearance}");

// Query the table for all entities in the partition "gear-surf-surfboards"
var results =await tableService.QueryEntitiesAsync("gear-surf-surfboards");

foreach (var item in results)
{
    // Display each entity's details
    Console.WriteLine($"Entity: {item.Name}, Quantity: {item.Quantity}, Price: {item.Price}, Clearance: {item.Clearance}");
}
// Delete the entity from the table
await tableService.DeleteEntityAsync(entity.RowKey, entity.PartitionKey);
