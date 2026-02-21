using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheLibraryOfAlexandria.Models
{
    public enum ShippingStatus
    {
        Preparing,
        Shipped,
        Delivered,
        Returned,
        Cancelled
    }

    public class ShippingInfo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // Foreign key to the Order being shipped
        public int OrderId { get; set; }

        // Shipping address details
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;

        // Shipping cost for this order
        public decimal ShippingCost { get; set; } = decimal.Zero;

        // Current shipping status (Preparing, Shipped, Delivered, etc.)
        public ShippingStatus Status { get; set; } = ShippingStatus.Preparing;

        // Tracking number provided by carrier
        public string TrackingNumber { get; set; } = string.Empty;

        // Estimated and actual delivery dates (populated when shipment progresses)
        public DateTime? EstimatedDelivery { get; set; }
        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }

        // Navigation property to the related Order
        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }
    }
}