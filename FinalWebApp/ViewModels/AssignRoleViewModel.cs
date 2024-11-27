using Microsoft.AspNetCore.Identity;

namespace FinalWebApp.ViewModels
{
    public class AssignRoleViewModel
    {
        public string UserId { get; set; }
        public List<IdentityRole> Roles { get; set; }
        public List<string> SelectedRoles { get; set; } = new List<string>();
    }

}
