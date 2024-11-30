namespace FinalWebApp.Data.Entities
{
    public class Category
    {
        public Category() 
        {
            Id = Guid.NewGuid();
            Items = new List<Item>();
            OrderItems = new List<OrderItem>();
        }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public ICollection<Item> Items { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }
    }
}
