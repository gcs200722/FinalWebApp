using FinalWebApp.Enum;
using System.ComponentModel.DataAnnotations;

namespace FinalWebApp.ViewModels
{
    public class RegisterViewModel : IValidatableObject
    {
        public string? Id { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "Full Name must be at most 100 characters long.")]
        public string Fullname { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(256, ErrorMessage = "Email must be at most 256 characters long.")]
        public string Email { get; set; }
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string Password { get; set; }

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


        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Nếu đang tạo mới (không có Id), thì yêu cầu password
            if (string.IsNullOrWhiteSpace(Id))
            {
                if (string.IsNullOrWhiteSpace(Password))
                {
                    yield return new ValidationResult("Password is required.", new[] { "Password" });
                }

                if (string.IsNullOrWhiteSpace(ConfirmPassword))
                {
                    yield return new ValidationResult("Confirm Password is required.", new[] { "ConfirmPassword" });
                }
            }
        }
    }
}
