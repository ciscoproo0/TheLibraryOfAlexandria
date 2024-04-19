using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TheLibraryOfAlexandria.Models
{
    public class ShoppingCartItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("ShoppingCart")]
        public int CartId { get; set; }

        [JsonIgnore]
        public virtual ShoppingCart? ShoppingCart { get; set; }

        [ForeignKey("Product")]
        public int ProductId { get; set; }

        [JsonIgnore]
        public virtual Product? Product { get; set; }

        public int Quantity { get; set; }
    }
}
