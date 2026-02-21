using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TheLibraryOfAlexandria.Models
{
    public class ShoppingCart
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // Foreign key to the User who owns this cart
        public int UserId { get; set; }

        // Timestamp when the cart was created
        public DateTime CreatedAt { get; set; }

        // Navigation property to the User
        [ForeignKey("UserId")]
        [JsonIgnore]
        public virtual User? User { get; set; }

        // Items in the shopping cart
        public virtual List<ShoppingCartItem> Items { get; set; } = new List<ShoppingCartItem>();
    }
}
