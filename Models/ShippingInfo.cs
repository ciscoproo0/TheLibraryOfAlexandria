using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheLibraryOfAlexandria.Models
{   public class ShippingInfo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int OrderId { get; set; }
        [ForeignKey("OrderId"), Required]
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public decimal ShippingCost { get; set; } = decimal.Zero;
        public string Status { get; set; } = string.Empty; 
        public string TrackingNumber { get; set; } = string.Empty;
        public DateTime? EstimatedDelivery { get; set; } = DateTime.UtcNow;
        public DateTime? ShippedDate { get; set; } = DateTime.UtcNow;
        public DateTime? DeliveredDate { get; set; } = DateTime.UtcNow;
    }
}