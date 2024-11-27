using FinalWebApp.Enum;
using System.ComponentModel.DataAnnotations;

namespace FinalWebApp.ViewModels
{
    public class TableViewModel
    {
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Must not be empty.")]
        [MaxLength(75)]
        public string Name { get; set; }
        [Required]
        public StatusTableEnum Status { get; set; }
        [Required]
        public int Capacity { get; set; }
    }
}
