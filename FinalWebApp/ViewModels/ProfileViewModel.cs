using System;
using System.ComponentModel.DataAnnotations;
using FinalWebApp.Enum;
using Microsoft.AspNetCore.Http;

namespace FinalWebApp.ViewModels
{
    public class ProfileViewModel
    {
        public string UserId { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Full name must be at most 100 characters.")]
        public string Fullname { get; set; }

        public GenderEnum Gender { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [StringLength(15, ErrorMessage = "Phone number must be at most 15 characters.")]
        public string NumberPhone { get; set; }

        public IFormFile? Avatar { get; set; }  // Đường dẫn ảnh đại diện
        public string? AvatarUrl { get; set; }
    }
}
