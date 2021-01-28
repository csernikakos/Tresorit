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

        [Display(Name ="Description")]
        public string Description { get; set; }

        [Required]
        [MaxLength(500)]
        [Display(Name = "Review Text")]
        public string Review { get; set; }

        [MaxLength(500)]
        [Display(Name = "Name")]
        public string Reviewer { get; set; }

        public ProductViewModel()
        {            
        }

    }
}
