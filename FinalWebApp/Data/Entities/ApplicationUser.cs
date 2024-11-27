using FinalWebApp.Enum;
using Microsoft.AspNetCore.Identity;
using System;

namespace FinalWebApp.Data.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string Fullname { get; set; }
        public GenderEnum Gender { get; set; }
        public string? Avatar { get; set; }
        public string NumberPhone { get; set; }  // Sửa lại kiểu dữ liệu cho số điện thoại (string thay vì int)
        public DateTime? DateOfBirth { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }
}
