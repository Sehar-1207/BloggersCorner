using System.ComponentModel.DataAnnotations;

namespace BloggingCorner.Models.Dto
{
    public class RegisterDto
    {
        [Required, StringLength(50, ErrorMessage = "Username must be between 3 and 50 characters.", MinimumLength = 3)]
        public string UserName { get; set; }

        [Required, StringLength(100, ErrorMessage = "Full name must be between 3 and 100 characters.", MinimumLength = 3)]
        public string FullName { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        [Required, DataType(DataType.Password), Compare("Password")]
        public string ConfirmPassword { get; set; }

        public string? AdminCode { get; set; }

    }
}
