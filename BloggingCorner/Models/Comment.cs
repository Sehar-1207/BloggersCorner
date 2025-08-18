using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BloggingCorner.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string username { get; set; }
        public string Commentcontent { get; set; }

        [DataType(DataType.Date)]
        public DateTime CommentAt { get; set; } = DateTime.UtcNow;
        [ForeignKey("Post")]
        public int PostId { get; set; }
        [ValidateNever]
        public Post Post { get; set; }
    }
}
