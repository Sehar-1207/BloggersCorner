using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BloggingCorner.Models
{
    public class Like
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Username { get; set; }

        [DataType(DataType.Date)]
        public DateTime LikedAt { get; set; } = DateTime.UtcNow;
        [ForeignKey("Post")]
        public int PostId { get; set; }
        public Post Post { get; set; }

        [ForeignKey("AspNetUsers")]
        public string UserId { get; set; }   // FK to AspNetUsers
        public ApplicationUser User { get; set; }
    }
}
