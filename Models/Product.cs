﻿using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Tresorit.Models
{
    public class Product : TableEntity
    {       

        [Required]
        [MaxLength(500)]
        [JsonProperty(PropertyName ="review")]
        public string Review { get; set; }

        [Required]
        [JsonProperty(PropertyName = "rating")]
        public int? Rating { get; set; }

        [JsonProperty(PropertyName = "averageRating")]
        [DisplayFormat(DataFormatString = "{0:N1}")]
        public decimal AverageRating { get; set; }

        [JsonProperty(PropertyName = "ratingQuantity")]
        public int? RatingQuantity { get; set; }

        [JsonProperty(PropertyName = "imageName")]
        public string ImageName { get; set; }

        [Display(Name ="Description")]
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [Display(Name = "Reviewer")]
        [JsonProperty(PropertyName = "reviewer")]
        public string Reviewer { get; set; }
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
