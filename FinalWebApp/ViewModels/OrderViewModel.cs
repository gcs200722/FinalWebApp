namespace FinalWebApp.ViewModels
{
    public class OrderViewModel
    {
        public List<CartItemViewModel> Items { get; set; }
        public decimal TotalAmount { get; set; }
        public string CustomerName { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime OrderTime { get; set; }
        public string TableNumber { get; set; } // Số bàn
        public DateTime DiningTime { get; set; }
    }

}
