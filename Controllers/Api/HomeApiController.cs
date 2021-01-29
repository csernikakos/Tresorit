using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tresorit.Data;
using Tresorit.Models;

namespace Tresorit.Controllers.Api
{
    [ApiController]
    public class HomeApiController : ControllerBase
    {
        DataTableQueries dataTable;
        private readonly IConfiguration _configuration;
        public HomeApiController(IConfiguration configuration)
        {            
            _configuration = configuration;
            var connectionString = _configuration.GetConnectionString("StorageAccount");
            dataTable = new DataTableQueries(connectionString);
        }

        [HttpGet("products/types")]
        public IEnumerable<Product> GetProductTypes()
        {
            List<Product> products = dataTable.GetUniquePartitionKeys();
            foreach (var product in products)
            {
                product.AverageRating = dataTable.AverageRating(product.PartitionKey);
                product.RatingQuantity = dataTable.RatingQuantity(product.PartitionKey);
            }
            return products;
        }
        [HttpGet("products/{partitionKey}")]
        public IEnumerable<Product> GetReviewsByProduct(string partitionkey)
        {
            List<Product> products = dataTable.GetPartitionKeyItems(partitionkey);
            return products;
        }


        [HttpGet("products/imageName/{partitionKey}")]
        public string GetImageName(string partitionKey)
        {
            var imageName = "";
            var productList = GetReviewsByProduct(partitionKey);
            foreach (var product in productList)
            {
                if (product.ImageName != null)
                {
                    imageName = product.ImageName;
                }
            }
            return imageName;
        }       

    }
}
