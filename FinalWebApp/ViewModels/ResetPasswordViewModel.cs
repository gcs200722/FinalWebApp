using System.ComponentModel.DataAnnotations;

namespace FinalWebApp.ViewModels
{
    public class ResetPasswordViewModel
    {
        public Guid UserId { get; set; }

        public string Token { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
    }

}
