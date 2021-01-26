using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tresorit.Models;

namespace Tresorit.ViewModels
{
    public class ProductViewModel
    {
        public IEnumerable<Product> Products { get; set; }
        //public Product Product { get; set; }
        public string PartitonKey { get; set; }
        public string Review { get; set; }

        //public int Rating { get; set; }

        public ProductViewModel()
        {
            
        }

        //public ProductViewModel(Product product)
        //{
        //    Product = product;
        //}
    }
}
