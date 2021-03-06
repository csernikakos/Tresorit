﻿using Azure.Storage.Blobs;
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
using Tresorit.Controllers.Api;
using Tresorit.Data;
using Tresorit.Models;
using Tresorit.ViewModels;

namespace Tresorit.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private DataQueries dataQueries;
        private readonly IConfiguration _configuration;
        private HomeApiController apiController;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            var connectionString = _configuration.GetConnectionString("StorageAccount");
            dataQueries = new DataQueries(connectionString);
            apiController = new HomeApiController(_configuration);
        }

        public IActionResult Index()
        {           
            var products = apiController.GetProductTypes();
            return View(products);
        }

        public IActionResult Details(string partitionKey)
        {            
            return View("Details", ViewModelData(partitionKey));
        }

        // Save new Table Storage entry

        public IActionResult Save(Product product, List<IFormFile> files)
        {
            Guid guid = Guid.NewGuid();
            product.RowKey = guid.ToString();

            // Insert new Product into the Table Storage

            if (product.Rating == null && product.Review == null)
            {
                // Check it if the new product has an image attached, if yes it will upload to the Blob Storage
                foreach (var file in files)
                {
                    if (file.ContentType.Contains("image"))
                    {
                        product.ImageName = file.FileName;
                        try
                        {
                            dataQueries.UploadAsync(file).Wait();
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }                    
                }
                dataQueries.InsertOrMergeProductAsync(product).Wait();
                return RedirectToAction("Index", "Home");
            }

            // Insert a review to the product
            else
            {
                ModelState.Clear();
                if (files.Count() == 0)
                {
                    product.ImageName = null;
                }

                // Check the product has an attached image or not

                if(product.Rating != 0 && product.Review!=null)
                {
                    if (apiController.GetImageName(product.PartitionKey) == "")
                    {
                        product.ImageName = null;
                    }
                    else
                    {
                        product.ImageName = apiController.GetImageName(product.PartitionKey);
                    }                                 
                }                
                dataQueries.InsertOrMergeProductAsync(product).Wait();
                return View("Details", ViewModelData(product.PartitionKey));
            }            
        }

        // Collecting data for the ViewModel

        public ProductViewModel ViewModelData(string partitionKey)
        {
            var products = apiController.GetReviewsByProduct(partitionKey);
            string description = "";
            string imageName = "";
            foreach (var product in products)
            {
                if (product.Rating == null && product.Review == null)
                {
                    description = product.Description;
                    imageName = product.ImageName;
                }
            }
            ProductViewModel productViewModel = new ProductViewModel
            {
                Products = products,
                PartitonKey = partitionKey,
                Description = description,
                ImageName = imageName
            };

            return productViewModel;
        }

        public IActionResult New()
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
