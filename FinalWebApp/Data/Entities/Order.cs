namespace FinalWebApp.Data.Entities
{
    public class Order
    {
        public Guid Id { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string CustomerId { get; set; } // Nếu bạn sử dụng ASP.NET Identity
        public string Status { get; set; }
        public Guid TableId { get; set; } // ID bàn đặt

        public DateTime DiningTime { get; set; }
        public Table Table { get; set; } // Liên kết tới bảng Table
        public List<OrderItem> OrderItems { get; set; }
        public ApplicationUser Customer { get; set; }
    }
}

