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
        public int OrderId { get; set; }
        [ForeignKey("OrderId"), Required]
        public decimal Amount { get; set; } = decimal.Zero;
        public PaymentMethod Method { get; set; } = PaymentMethod.CreditCard;
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        [Required(ErrorMessage = "A transactionId is required")]
        public string TransactionId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }

        [JsonIgnore]
        public virtual Order? Order { get; set; }

    }
}