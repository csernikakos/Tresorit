using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Tresorit.Models;

namespace Tresorit.ViewModels
{
    public class ProductViewModel
    {
        public IEnumerable<Product> Products { get; set; }
        public string PartitonKey { get; set; }

        [Required]
        [MaxLength(500)]
        public string Review { get; set; }

        public ProductViewModel()
        {            
        }

    }
}
