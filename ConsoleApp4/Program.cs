// See https://aka.ms/new-console-template for more information
using ConsoleApp4;

Console.WriteLine("Hello, World!");


BlobService blobService = new BlobService("azurestoragesa20251");

await blobService.CreateContainerAsync("test123container");
var blobName = await blobService.UploadBlobAsync();

//var value=blobService.GenerateSASBlobURL(blobName,TimeSpan.FromHours(1));
await blobService.RequestUserDelegationKey();
var value = await blobService.GenerateUserDelegationSASBlobUrl(blobName, TimeSpan.FromHours(1));
Console.WriteLine(value);




