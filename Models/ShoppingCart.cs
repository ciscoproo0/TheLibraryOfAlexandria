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

        [ForeignKey("User")]
        public int UserId { get; set; }

        [JsonIgnore]
        public virtual User? User { get; set; }

        public DateTime CreatedAt { get; set; }

        public virtual List<ShoppingCartItem> Items { get; set; } = new List<ShoppingCartItem>();
    }
}
