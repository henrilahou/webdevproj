using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace project.Models
{
    public class OrderViewModel
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please enter a quantity.")]
        public int Quantity { get; set; }

        // Deze lijst wordt gebruikt om de dropdown te vullen in de view.
        public IEnumerable<SelectListItem> Products { get; set; }
    }
}
