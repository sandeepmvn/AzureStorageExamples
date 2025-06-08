using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp4
{
    public class BlobService
    {
        private BlobServiceClient _blobServiceClient;
        public BlobServiceClient BlobServiceClient
        {
            get { return _blobServiceClient; }
            set { _blobServiceClient = value; }
        }

        private UserDelegationKey _userDelegationKey;

        private string _name;
        public BlobService(string name)
        {
            _name = name;
            Intialize();
        }

        void Intialize()
        {
            _blobServiceClient = new BlobServiceClient(
        new Uri($"https://{_name}.blob.core.windows.net"),
        new DefaultAzureCredential());
        }

        public BlobContainerClient BlobContainerClientProperty { get; set; }

        //Create a container
        public async Task CreateContainerAsync(string containerName)
        {
            // Create the container and return a container client object
            //BlobContainerClientProperty = await _blobServiceClient.CreateBlobContainerAsync(containerName);

            BlobContainerClientProperty = _blobServiceClient.GetBlobContainerClient(containerName);
            await BlobContainerClientProperty.CreateIfNotExistsAsync();
        }

        //Upload a blob to a container
        public async Task<string> UploadBlobAsync()
        {
            // Create a local file in the ./data/ directory for uploading and downloading
            string localPath = "data";
            Directory.CreateDirectory(localPath);
            string fileName = "quickstart" + Guid.NewGuid().ToString() + ".txt";
            string localFilePath = Path.Combine(localPath, fileName);

            // Write text to the file
            await File.WriteAllTextAsync(localFilePath, "Hello, World!");

            // Get a reference to a blob
            BlobClient blobClient = BlobContainerClientProperty.GetBlobClient(fileName);

            Console.WriteLine("Uploading to Blob storage as blob:\n\t {0}\n", blobClient.Uri);

            // Upload data from the local file, overwrite the blob if it already exists
            await blobClient.UploadAsync(localFilePath, true);

            return fileName;
        }


        //List blobs in a container
        public async Task ListBlobsAsync()
        {
            Console.WriteLine("Listing blobs in container: " + BlobContainerClientProperty.Name);
            // Get the blobs in the container and print their names
            await foreach (var blobItem in BlobContainerClientProperty.GetBlobsAsync())
            {
                Console.WriteLine("\t" + blobItem.Name);
            }
        }

        //Download a blob
        public async Task DownloadBlobAsync(string blobName)
        {
            // Get a reference to the blob
            BlobClient blobClient = BlobContainerClientProperty.GetBlobClient(blobName);
            // Download the blob's contents and save it to a file
            string localFilePath = Path.Combine("data", blobName);
            Console.WriteLine("Downloading blob to\n\t{0}\n", localFilePath);
            await blobClient.DownloadToAsync(localFilePath);
        }

        //Delete a container
        public async Task DeleteContainerAsync()
        {
            // Delete the container and all its blobs
            await BlobContainerClientProperty.DeleteIfExistsAsync();
            Console.WriteLine($"Container {BlobContainerClientProperty.Name} deleted.");
        }


        public async Task RequestUserDelegationKey(
    )
        {
            // Get a user delegation key for the Blob service that's valid for 1 day
            UserDelegationKey userDelegationKey =
                await _blobServiceClient.GetUserDelegationKeyAsync(
                    DateTimeOffset.UtcNow,
                    DateTimeOffset.UtcNow.AddDays(1));

            _userDelegationKey = userDelegationKey;
        }


        //Create a user delegation SAS
        public async Task<Uri> GenerateUserDelegationSASBlobUrl(string blobName,
    TimeSpan timeSpan)
        {
            // Get a reference to the blob
            BlobClient blobClient = BlobContainerClientProperty.GetBlobClient(blobName);
            // Create a SAS token for the blob resource that's also valid for 1 day
            BlobSasBuilder sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = blobClient.BlobContainerName,
                BlobName = blobClient.Name,
                Resource = "b",
                StartsOn = DateTimeOffset.UtcNow,
                ExpiresOn = DateTimeOffset.UtcNow.Add(timeSpan)
            };

            // Specify the necessary permissions
            sasBuilder.SetPermissions(BlobSasPermissions.Read | BlobSasPermissions.Write);

            // Add the SAS token to the blob URI
            BlobUriBuilder uriBuilder = new BlobUriBuilder(blobClient.Uri)
            {
                // Specify the user delegation key
                Sas = sasBuilder.ToSasQueryParameters(
                    _userDelegationKey,
                    blobClient
                    .GetParentBlobContainerClient()
                    .GetParentBlobServiceClient().AccountName)
            };

            return uriBuilder.ToUri();
        }

        //Generate a SAS token for a blob
        public async Task<Uri> GenerateSASBlobURL(string blobName,
    TimeSpan timeSpan = default,
    string storedPolicyName = null)
        {
            // Get a reference to the blob
            BlobClient blobClient = BlobContainerClientProperty.GetBlobClient(blobName);
            // Check if BlobContainerClient object has been authorized with Shared Key
            if (blobClient.CanGenerateSasUri)
            {
                // Create a SAS token that's valid for one day
                BlobSasBuilder sasBuilder = new BlobSasBuilder()
                {
                    BlobContainerName = blobClient.GetParentBlobContainerClient().Name,
                    BlobName = blobClient.Name,
                    Resource = "b"
                };

                if (storedPolicyName == null)
                {
                    sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.Add(timeSpan);
                    sasBuilder.SetPermissions(BlobContainerSasPermissions.Read);
                }
                else
                {
                    sasBuilder.Identifier = storedPolicyName;
                }

                Uri sasURI = blobClient.GenerateSasUri(sasBuilder);

                return sasURI;
            }
            else
            {
                // Client object is not authorized via Shared Key
                return null;
            }
        }
    }
}
