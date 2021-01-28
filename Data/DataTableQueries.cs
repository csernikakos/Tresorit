using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tresorit.Models;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace Tresorit.Data
{
    public class DataTableQueries
    {
        string tableName = "TresoritTable";
        string connString = "DefaultEndpointsProtocol=https;AccountName=tresoritstrorageacc;AccountKey=tq+qxPI6OO2QYMlCCSzveMTSHz+UJJVSUt/8/yvX9WVjE0umZJgaxbAVEZMeqlWReDOP20YlFXxteHd/GTD+FA==;EndpointSuffix=core.windows.net";

        CloudTable table;

        public DataTableQueries()
        {
            CloudStorageAccount storageAccount;
            storageAccount = CloudStorageAccount.Parse(connString);

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            table = tableClient.GetTableReference(tableName);
        }

        [HttpGet]
        public List<Product> GetUniquePartitionKeys()
        {
            TableQuery<Product> queryResult = new TableQuery<Product>();
            var itemlist = table.ExecuteQuery(queryResult);

            List<Product> distinct = itemlist.GroupBy(x => x.PartitionKey).Select(g => g.First()).ToList();
            return distinct;
        }

        [HttpGet]
        public List<Product> GetPartitionKeyItems(string partitionkey)
        {
            TableQuery<Product> queryResult = new TableQuery<Product>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionkey)); 
            var itemlist = table.ExecuteQuery(queryResult);
            List<Product> distinct = itemlist.GroupBy(x => x.RowKey).Select(g => g.First()).OrderByDescending(t=>t.Timestamp).ToList();
            return distinct;
        }

        public decimal AverageRating(string partitionkey)
        {
            decimal result = 0;
            decimal sum = 0;
            TableQuery<Product> queryResult = new TableQuery<Product>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionkey));
            var itemlist = table.ExecuteQuery(queryResult);
            var itemCount = itemlist.Count();

            foreach (var item in itemlist)
            {
                if (item.Rating != null)
                {
                    sum += (int)item.Rating;
                }
            }
            if (itemlist.Count()>1)
            {
                itemCount = (itemCount - 1);
            }
            result = sum / itemCount;
            result = Math.Round(result, 1);

            return result;
        }

        public int RatingQuantity(string partitionkey)
        {
            int sum = 0;
            TableQuery<Product> queryResult = new TableQuery<Product>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionkey));
            var itemlist = table.ExecuteQuery(queryResult);
            foreach (var item in itemlist)
            {
                if (item.Review!=null)
                {
                    sum++;
                }
            }
            return sum;
        }

        [HttpPost]
        public async Task<Product> InsertOrMergeProduct(Product product)
        {
            if (product==null)
            {
                throw new ArgumentNullException();
            }
            try
            {
                TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(product);
                TableResult result = await table.ExecuteAsync(insertOrMergeOperation);
                Product insertedProduct = result.Result as Product;

                return insertedProduct;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                throw;
            }
        }

        [HttpPost]
        public async Task CreateBlob()
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(connString);
            string containername = "blobcontainer2" + Guid.NewGuid().ToString();
            BlobContainerClient containerClient = await blobServiceClient.CreateBlobContainerAsync(containername);
        }

        [HttpPost]
        public async Task UploadToBlobContainer()
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(connString);
            string containername = "blobcontainer" + Guid.NewGuid().ToString();
            BlobContainerClient containerClient = await blobServiceClient.CreateBlobContainerAsync(containername);

            string localPath = "E://";
            string fileName = "blobupload.txt";
            string localFilePath = Path.Combine(localPath, fileName);

            await File.WriteAllTextAsync(localFilePath, "Helo");

            BlobClient blobClient = containerClient.GetBlobClient(fileName);
            using FileStream uploadFileStream = File.OpenRead(localFilePath);
            await blobClient.UploadAsync(uploadFileStream, true);
            uploadFileStream.Close();
        }

        public async Task UploadImageToBlobContainer(IFormFile file)
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(connString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("publicblobcontainer");
            BlobClient blobClient = containerClient.GetBlobClient("publicblobcontainer");

            //string filePath = "E:\\VisualStudioProjects\\_img\\tresorit-logo.png";
            string currentFilePath = Path.GetFullPath(file.FileName);
            
            using FileStream uploadFileStream = File.OpenRead(currentFilePath);
            await blobClient.UploadAsync(uploadFileStream, true);
            uploadFileStream.Close();
        }              

        public void Upload(IEnumerable<FileInfo> files, string connectionString, string container)
        {
            var containerClient = new BlobContainerClient(connectionString, container);

            foreach (var file in files)
            {
                try
                {
                    var blobClient = containerClient.GetBlobClient(file.Name);
                    using(var fileStream = File.OpenRead(file.FullName))
                    {
                        blobClient.Upload(fileStream,overwrite:true);
                    }
                }
                catch (Exception)
                {

                    throw;
                }
            }
        }

        public async Task CreateBlobNew(IFormFile file)
        {

            BlobServiceClient blobServiceClient = new BlobServiceClient(connString);

            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("publicblobcontainer");

            BlobClient blobClient = containerClient.GetBlobClient("tresoritimg2.png");       
            
            //using FileStream uploadFileStream = File.OpenRead(filepath);

            //await blobClient.UploadAsync(uploadFileStream, true);
            //uploadFileStream.Close();

            var imageInBytes = ConvertImageToByteArray(file);
        }
        private byte[] ConvertImageToByteArray(IFormFile image)
        {
            byte[] result = null;
            using(var fileStream = image.OpenReadStream())
                using(var memoryStream = new MemoryStream())
            {
                fileStream.CopyTo(memoryStream);
                result = memoryStream.ToArray();
            }
            return result;
        }

        private async Task<string> UploadImageByteArry(byte[] imageBytes, string imageName, string contentType)
        {
            if (imageBytes==null || imageBytes.Length==0)
            {
                return null;
            }
            //BlobServiceClient blobServiceClient = new BlobServiceClient(connString);
            //BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("publicblobcontainer");
            //BlobClient blobClient = containerClient.GetBlobClient(imageName);

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connString);
            

            CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference("quickstartblobs" + Guid.NewGuid().ToString());
            await cloudBlobContainer.CreateAsync();

            var cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(imageName);
            cloudBlockBlob.Properties.ContentType = contentType;

            const int byteArrayStartIndex = 0;
            await cloudBlockBlob.UploadFromStreamAsync(imageBytes, byteArrayStartIndex, imageBytes.Length);

        }

    }
}
