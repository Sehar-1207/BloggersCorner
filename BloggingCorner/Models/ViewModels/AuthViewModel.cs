using BloggingCorner.Models.Dto;

namespace BloggingCorner.Models.ViewModels
{
    public class AuthViewModel
    {
        public LoginDto Login { get; set; }
        public RegisterDto Register { get; set; }
        public bool IsRegisterActive { get; set; }
    }
}
