using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TheLibraryOfAlexandria.Models
{
    public class UserFavorite
    {
        [Key]
        public int Id { get; set; }

        // Foreign keys
        public int UserId { get; set; }
        public int ProductId { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        [JsonIgnore]
        public virtual User? User { get; set; }

        [ForeignKey("ProductId")]
        [JsonIgnore]
        public virtual Product? Product { get; set; }

        // Timestamp when the product was added to favorites
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}
