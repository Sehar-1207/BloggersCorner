using System.ComponentModel.DataAnnotations;

namespace BloggingCorner.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string? Description { get; set; }
        public ICollection<Post> posts { get; set; }
    }
}
