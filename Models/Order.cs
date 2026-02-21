using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheLibraryOfAlexandria.Models
{
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // Foreign key to User who placed the order
        public int UserId { get; set; }

        // Order status (pending, processing, completed, cancelled)
        public string Status { get; set; } = "pending";

        // Total price of the order (sum of items + shipping)
        public decimal TotalPrice { get; set; } = decimal.Zero;

        // Timestamps for audit trail
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property to the user who placed the order
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        // Line items included in this order
        public virtual List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        // Shipping information (optional until order is finalized)
        public virtual ShippingInfo? ShippingInfo { get; set; }
    }
}
