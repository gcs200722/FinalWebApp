using System.ComponentModel.DataAnnotations;

namespace FinalWebApp.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "Username must be at most 100 characters long.")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string Password { get; set; }

        // Optional: Giữ trạng thái đăng nhập (ví dụ: "remember me")
        public bool RememberMe { get; set; }
    }
}

