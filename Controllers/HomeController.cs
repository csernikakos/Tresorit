using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            dataTable = new DataTableQueries();
        }

        public IActionResult Index()
        {
            List<Product> products = dataTable.GetUniquePartitionKeys();
            foreach (var product in products)
            {
                product.AverageRating = dataTable.AverageRating(product.PartitionKey);
                product.RatingQuantity = dataTable.RatingQuantity(product.PartitionKey);
                //if (dataTable.AverageRating(product.PartitionKey)>0)
                //{
                //    product.AverageRating = dataTable.AverageRating(product.PartitionKey);
                //    product.RatingQuantity = dataTable.RatingQuantity(product.PartitionKey);
                //}
                //else
                //{
                //    product.AverageRating = 0;
                //    product.RatingQuantity = 0;
                //}

            }
            return View(products);
        }

        public IActionResult ProductForm(string partitionkey)
        {
            List<Product> products = dataTable.GetPartitionKeyItems(partitionkey);

            ProductViewModel productViewModel = new ProductViewModel
            {
                Products = products,
                PartitonKey = partitionkey
            };

            return View("ProductForm", productViewModel);
        }

        public IActionResult Save(Product product)
        {
            //List<Product> products = dataTable.GetPartitionKeyItems(product.PartitionKey);
            //ProductViewModel productViewModel = new ProductViewModel
            //{
            //    Products = products
            //};

            ////Guid guid = Guid.NewGuid();
            //Product insertedProduct = new Product() {
            //    PartitionKey = product.PartitionKey,
            //    RowKey = guid.ToString(),
            //    Review = product.Review,
            //    Rating = product.Rating
            //};

            Guid guid = Guid.NewGuid();
            product.RowKey = guid.ToString();
            dataTable.InsertOrMergeProduct(product).Wait();
            return RedirectToAction("Index", "Home");
        }

        //public IActionResult SaveNewProduct(Product product)
        //{
        //    dataTable.InsertOrMergeProduct(product).Wait();
        //    return RedirectToAction("Index", "Home");
        //}

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
