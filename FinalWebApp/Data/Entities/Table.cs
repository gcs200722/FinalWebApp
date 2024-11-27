using FinalWebApp.Enum;
using System.ComponentModel.DataAnnotations;

namespace FinalWebApp.Data.Entities
{
    public class Table
    {
        public Table()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; set; }
        [MaxLength(100)]
        public string Name { get; set; }
        public int Capacity { get; set; }
        public StatusTableEnum Status { get; set; }
    }
}
