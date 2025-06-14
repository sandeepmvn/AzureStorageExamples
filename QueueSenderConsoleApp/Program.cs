// See https://aka.ms/new-console-template for more information
using Azure.Identity;
using Azure.Storage.Queues;
using QueueSenderConsoleApp;

Console.WriteLine("Hello, World!");
// Create a unique name for the queue
// TODO: Replace the <storage-account-name> placeholder 
string queueName = "quickstartqueues-0a069f7b-c7f6-4910-bcd2-8fa9aae4ad7c"; //"quickstartqueues-" + Guid.NewGuid().ToString();
string storageAccountName = "azurestoragesa20251";

// Instantiate a QueueClient to create and interact with the queue
QueueClient queueClient = new QueueClient(
    new Uri($"https://{storageAccountName}.queue.core.windows.net/{queueName}"),
    new DefaultAzureCredential());

//Create a queue
Console.WriteLine($"Creating queue: {queueName}");

// Create the queue
await queueClient.CreateIfNotExistsAsync();

//Snd a message to the queue
Console.WriteLine($"Sending message to queue: {queueName}");
await queueClient.SendMessageAsync("Hello, World!");

// Send a message with an Employee object serialized as JSON
//Console.WriteLine($"Sending message with Employee object to queue: {queueName}");
Employee employee = new Employee
{
    Id = 1,
    Name = "John Doe",
    Title = "Software Engineer",
    Department = "Engineering",
};
string employeeJson = System.Text.Json.JsonSerializer.Serialize(employee);
//256 KB is the maximum size for a message in Azure Storage Queues
await queueClient.SendMessageAsync(employeeJson);



Console.WriteLine();