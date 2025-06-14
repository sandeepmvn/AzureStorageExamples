// See https://aka.ms/new-console-template for more information
using Azure.Identity;
using Azure.Storage.Queues;

Console.WriteLine("Hello, World!");
// Create a unique name for the queue
// TODO: Replace the <storage-account-name> placeholder 
string queueName = "quickstartqueues-0a069f7b-c7f6-4910-bcd2-8fa9aae4ad7c"; //"quickstartqueues-" + Guid.NewGuid().ToString();
string storageAccountName = "azurestoragesa20251";



// Instantiate a QueueClient to create and interact with the queue
QueueClient queueClient = new QueueClient(
    new Uri($"https://{storageAccountName}.queue.core.windows.net/{queueName}"),
    new DefaultAzureCredential());


//Recieve messages from the queue
Console.WriteLine($"Receiving messages from queue: {queueName}");

// Get the messages from the queue
var messages = await queueClient.ReceiveMessagesAsync(maxMessages: 10, visibilityTimeout: TimeSpan.FromSeconds(30));
foreach (var message in messages.Value)
{
    Console.WriteLine($"Message ID: {message.MessageId}, Content: {message.MessageText}");

    // Process the message (e.g., deserialize if it's an Employee object)
    // For demonstration, we will just print the message text
    // If you had serialized an Employee object, you would deserialize it here
    var employee = System.Text.Json.JsonSerializer.Deserialize<Employee>(message.MessageText);
    // Delete the message after processing
    await queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt);
}



public class Employee
{
    public int Id { get; set; }

    public string Name { get; set; }
    public string Title { get; set; }
    public string Department { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }

}
