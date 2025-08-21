using System.ComponentModel.DataAnnotations;

namespace BloggingCorner.Models.Dto
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Email is required.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }

        public bool RememberMe { get; set; } // Default to false

    }
}
