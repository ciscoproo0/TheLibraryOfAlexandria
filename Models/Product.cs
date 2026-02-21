using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheLibraryOfAlexandria.Models
{
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // Product basic information
        [Required(ErrorMessage = "A name for the product is required")]
        public string Name { get; set; } = "New Product";

        [Required(ErrorMessage = "A Description for the product is required")]
        public string Description { get; set; } = string.Empty;

        // Product media and pricing
        public string ImageUrl { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required")]
        public decimal Price { get; set; } = 0;

        [Required(ErrorMessage = "The quantity is required")]
        public int StockQuantity { get; set; } = 0;

        // Magic: The Gathering specific attributes
        public string Edition { get; set; } = string.Empty;
        public string Rarity { get; set; } = string.Empty;

        // Audit timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    }
}
