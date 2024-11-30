using System.ComponentModel.DataAnnotations;

namespace FinalWebApp.Data.Entities
{
    public class CustomerReview
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string CustomerName { get; set; }

        [Required]
        [Range(1, 5)]
        public int FoodQualityRating { get; set; }  // Đánh giá chất lượng món ăn (1-5)

        [Required]
        [Range(1, 5)]
        public int ServiceRating { get; set; }  // Đánh giá dịch vụ (1-5)

        [Required]
        [Range(1, 5)]
        public int AmbienceRating { get; set; }  // Đánh giá không gian (1-5)

        [StringLength(500)]
        public string? Comments { get; set; }  // Ý kiến đóng góp

        [Required]
        public DateTime ReviewDate { get; set; }
    }
}
