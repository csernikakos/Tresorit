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

            return View("ProductForm",productViewModel);
        }

        public IActionResult Save(Product product)
        {
            List<Product> products = dataTable.GetPartitionKeyItems(product.PartitionKey);
            ProductViewModel productViewModel = new ProductViewModel
            {
                Products = products
            };

            Guid guid = Guid.NewGuid();
            Product insertedProduct = new Product() {
                PartitionKey = product.PartitionKey,
                RowKey = guid.ToString(),
                Review = product.Review,
                Rating = product.Rating
            };

            dataTable.InsertOrMergeProduct(insertedProduct).Wait();

            return RedirectToAction("Index", "Home");
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
