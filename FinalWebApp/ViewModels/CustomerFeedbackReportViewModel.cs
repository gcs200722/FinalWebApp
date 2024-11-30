using FinalWebApp.Data.Entities;

namespace FinalWebApp.ViewModels
{
    public class CustomerFeedbackReportViewModel
    {
        public int TotalReviews { get; set; }
        public double AverageFoodQualityRating { get; set; }
        public double AverageServiceRating { get; set; }
        public double AverageAmbienceRating { get; set; }
        public List<CustomerReview> CustomerReviews { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }
    }
}
