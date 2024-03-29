using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace project.Models
{
    public class Product
    {
        [Key] // Identificeert dit veld als de primaire sleutel.
        public int Id { get; set; }

        [Required] // Maakt het veld verplicht.
        [StringLength(100, ErrorMessage = "The name cannot exceed 100 characters.")]
        public string Name { get; set; }

        [Required] // Maakt het veld verplicht.
        [Range(0.01, double.MaxValue, ErrorMessage = "The price must be positive.")]
        [Column(TypeName = "decimal(18, 2)")] // Definieert de precisie en schaal voor de prijs.
        public decimal Price { get; set; }

        // Optioneel: Voorraad bijhouden
        [Range(0, int.MaxValue, ErrorMessage = "The stock must be positive or zero.")]
        public int Stock { get; set; }

        // Optioneel: Beschrijving toevoegen
        [StringLength(500, ErrorMessage = "The description cannot exceed 500 characters.")]
        public string Description { get; set; }

        // Optioneel: AfbeeldingsURL toevoegen voor het product
        [Url(ErrorMessage = "Please enter a valid URL.")]
        public string? ImageUrl { get; set; }

        // Optioneel: Categorie toevoegen als je producten wilt categoriseren
        public string Category { get; set; }
    }
}
