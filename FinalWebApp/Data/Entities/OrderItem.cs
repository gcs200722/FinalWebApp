namespace FinalWebApp.Data.Entities
{
    public class OrderItem
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid ItemId { get; set; }
        public Guid CategoryId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string ItemName { get; set; }
        public string ItemImage { get; set; }
        public Item Item { get; set; }
        public Category Category { get; set; }
        public Order Order { get; set; }
    }
}
