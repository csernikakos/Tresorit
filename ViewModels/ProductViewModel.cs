using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Tresorit.Models;

namespace Tresorit.ViewModels
{
    public class ProductViewModel
    {
        public IEnumerable<Product> Products { get; set; }
        public string PartitonKey { get; set; }

        public string Description { get; set; }

        [Required]
        [MaxLength(500)]
        public string Review { get; set; }

        public ProductViewModel()
        {            
        }

    }
}
