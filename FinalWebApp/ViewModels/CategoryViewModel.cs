using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace FinalWebApp.ViewModels
{
    [Bind("Id,Name")]
    public class CategoryViewModel
    {
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Must not be empty.")]
        [MaxLength(75)]
        public string Name { get; set; }

    }
}
