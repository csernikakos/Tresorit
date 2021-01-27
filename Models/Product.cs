using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Tresorit.Models
{
    public class Product : TableEntity
    {
        public string Review { get; set; }

        public int? Rating { get; set; }

        [DisplayFormat(DataFormatString = "{0:N1}")]
        public decimal AverageRating { get; set; }

        public int? RatingQuantity { get; set; }
        public Product()
        {
        }

        public Product(string partitonKey, string rowKey)
        {
            PartitionKey = partitonKey;
            RowKey = rowKey;
        }
    }
}
