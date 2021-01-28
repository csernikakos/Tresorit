using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Tresorit.Data;
using Tresorit.Models;
using Tresorit.ViewModels;

namespace Tresorit.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        DataTableQueries dataTable;

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

        public IActionResult Save(Product product)
        {
            Guid guid = Guid.NewGuid();
            product.RowKey = guid.ToString();
            dataTable.InsertOrMergeProduct(product).Wait();
            //var config = GetConfiguration();
            //var files = GetFiles(config["AzureStorage:SourceFolder"]);
            //foreach (var item in files)
            //{
            //    Console.WriteLine(item.Name);
            //}
            //dataTable.Upload(files, config["AzureStorage:ConnectionString"], config["AzureStorage:Container"]);

            //return RedirectToAction("Index", "Home");
            if (product.Rating == null && product.Review == null)
            {
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
    }
}
