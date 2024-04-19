using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheLibraryOfAlexandria.Models
{
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public string Status { get; set; } = "pending";
        public decimal TotalPrice { get; set; } = decimal.Zero;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public virtual List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        
        public virtual ShippingInfo ShippingInfo{ get; set; } = new ShippingInfo();
    }
}
