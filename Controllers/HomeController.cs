using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Tresorit.Data;
using Tresorit.Models;
using Tresorit.ViewModels;

namespace Tresorit.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        DataTableQueries dataTable;
        string connString = "DefaultEndpointsProtocol=https;AccountName=tresoritstrorageacc;AccountKey=tq+qxPI6OO2QYMlCCSzveMTSHz+UJJVSUt/8/yvX9WVjE0umZJgaxbAVEZMeqlWReDOP20YlFXxteHd/GTD+FA==;EndpointSuffix=core.windows.net";


        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            dataTable = new DataTableQueries();
        }
        static IConfigurationRoot GetConfiguration()
        {
            return new ConfigurationBuilder().SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName).AddJsonFile("appsettings.json").Build();
        }

        static IEnumerable<FileInfo> GetFiles(string sourceFolder)
        { 
            return new DirectoryInfo(sourceFolder).GetFiles().Where(f => !f.Attributes.HasFlag(FileAttributes.Hidden));
        }        

        public IActionResult Index()
        {
            List<Product> products = dataTable.GetUniquePartitionKeys();
            foreach (var product in products)
            {
                product.AverageRating = dataTable.AverageRating(product.PartitionKey);
                product.RatingQuantity = dataTable.RatingQuantity(product.PartitionKey);
            }
            return View(products);
        }

        public IActionResult ProductForm(string partitionKey)
        {
            //List<Product> products = dataTable.GetPartitionKeyItems(partitionKey);
            //string description="";
            //foreach (var product in products)
            //{
            //    if (product.Rating==null&&product.Review==null)
            //    {
            //        description = product.Description;
            //    }
            //}
            //ProductViewModel productViewModel = new ProductViewModel
            //{
            //    Products = products,
            //    PartitonKey = partitionKey,
            //    Description = description             
            //};
            //return View("ProductForm", productViewModel);

            return View("ProductForm", ViewModelData(partitionKey));

        }

        public IActionResult Save(Product product, List<IFormFile> files)
        {
            Guid guid = Guid.NewGuid();
            product.RowKey = guid.ToString();
            dataTable.InsertOrMergeProduct(product).Wait();

            if (product.Rating == null && product.Review == null)
            {
                foreach (var file in files)
                {
                    dataTable.Post(file).Wait();
                }
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.Clear();
                return View("ProductForm", ViewModelData(product.PartitionKey));
            }            
        }

        public ProductViewModel ViewModelData(string partitionKey)
        {
            List<Product> products = dataTable.GetPartitionKeyItems(partitionKey);
            string description = "";
            foreach (var product in products)
            {
                if (product.Rating == null && product.Review == null)
                {
                    description = product.Description;

                }
            }
            ProductViewModel productViewModel = new ProductViewModel
            {
                Products = products,
                PartitonKey = partitionKey,
                Description = description
            };

            return productViewModel;
        }

        public IActionResult New()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        
        private CloudStorageAccount GetStorageAccount()
        {
            var storageAccount = CloudStorageAccount.Parse(connString);
            return storageAccount;
        }

        public async Task<FileEntity> InsertOrMergeEntityAsync(CloudTable table, FileEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }
            try
            {
                // Create the InsertOrReplace table operation
                TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(entity);

                // Execute the operation.
                TableResult result = await table.ExecuteAsync(insertOrMergeOperation);
                FileEntity insertedCustomer = result.Result as FileEntity;

                // Get the request units consumed by the current operation. RequestCharge of a TableResult is only applied to Azure Cosmos DB
                if (result.RequestCharge.HasValue)
                {
                    Console.WriteLine("Request Charge of InsertOrMerge Operation: " + result.RequestCharge);
                }

                return insertedCustomer;
            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                throw;
            }
        }

        private async Task UploadTempFilesToAzureTable(List<IFormFile> files)
        {
            //var imageTable = await CreateTableAsync("Image");
            var tableName = "publicblobcontainer";
            CloudStorageAccount storageAccount;
            storageAccount = CloudStorageAccount.Parse(connString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            CloudTable imageTable = tableClient.GetTableReference(tableName);

            var fileEntities = files.Select(f => new FileEntity(f.FileName, f.ContentType));
            foreach (var fileEntity in fileEntities)
            {
                await InsertOrMergeEntityAsync(imageTable, fileEntity);
            }
        }
        private async Task<CloudTable> CreateTableAsync(string tableName)
        {
            var storageAccount = GetStorageAccount();
            var tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());

            Console.WriteLine("Create a Table for the demo");

            var table = tableClient.GetTableReference(tableName);
            if (await table.CreateIfNotExistsAsync())
            {
                Console.WriteLine("Created Table named: {0}", tableName);
            }
            else
            {
                Console.WriteLine("Table {0} already exists", tableName);
            }

            Console.WriteLine();
            return table;
        }

        [HttpPost("")]
        public async Task<IActionResult> FileUpload(List<IFormFile> files)
        {
            long size = files.Sum(f => f.Length);

            var filePaths = new List<string>();
            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    // full path to file in temp location
                    var filePath = Path.GetTempFileName(); //we are using Temp file name just for the example. Add your own file path.
                    filePaths.Add(filePath);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await formFile.CopyToAsync(stream);
                    }
                }
            }

            await UploadTempFilesToAzureTable(files);
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// /
        /// </summary>
        /// <returns></returns>
    }
}
