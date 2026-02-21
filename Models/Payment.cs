using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TheLibraryOfAlexandria.Models
{
    public enum PaymentMethod {
        PayPal,
        CreditCard,
        DebitCard,
        Pix,
        Boleto,
        ApplePay,
        GooglePay,
        Venmo,
        Oxxo,
        Cash,
        Alternative
    }

    public enum PaymentStatus {
        Pending,
        Completed,
        Denied,
        Refunded,
        Reverted,
        Undefined,
    }
    public class Payment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // Foreign key to the Order being paid for
        public int OrderId { get; set; }

        // Payment amount in currency (typically USD)
        public decimal Amount { get; set; } = decimal.Zero;

        // Payment method used (credit card, PayPal, Pix, etc.)
        public PaymentMethod Method { get; set; } = PaymentMethod.CreditCard;

        // Current status of the payment (Pending, Completed, Denied, etc.)
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        // Unique transaction ID from payment provider
        [Required(ErrorMessage = "A transactionId is required")]
        public string TransactionId { get; set; } = string.Empty;

        // Timestamps for audit trail
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }

        // Navigation property to the related Order
        [ForeignKey("OrderId")]
        [JsonIgnore]
        public virtual Order? Order { get; set; }

    }
}