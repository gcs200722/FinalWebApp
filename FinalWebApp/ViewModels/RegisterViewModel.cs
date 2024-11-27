using FinalWebApp.Enum;
using System.ComponentModel.DataAnnotations;

namespace FinalWebApp.ViewModels
{
    public class RegisterViewModel
    {
        public string? Id { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "Full Name must be at most 100 characters long.")]
        public string Fullname { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(256, ErrorMessage = "Email must be at most 256 characters long.")]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Confirm password does not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        public GenderEnum Gender { get; set; } // Assuming GenderEnum is an enum you defined

        [Required]
        [Phone]
        [StringLength(15, ErrorMessage = "Phone number must be at most 15 characters long.")]
        public string NumberPhone { get; set; }

        [Required]
        public DateTime? DateOfBirth { get; set; }

    }
}
