using Microsoft.AspNetCore.Identity;
namespace BloggingCorner.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public string Role { get; set; }
    }
}
