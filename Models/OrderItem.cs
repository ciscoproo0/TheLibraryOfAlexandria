using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheLibraryOfAlexandria.Models
{
    public class OrderItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // Foreign keys
        public int OrderId { get; set; }
        public int ProductId { get; set; }

        // Order item details
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        // Navigation properties
        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }
    }
}
