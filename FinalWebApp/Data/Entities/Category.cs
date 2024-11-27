namespace FinalWebApp.Data.Entities
{
    public class Category
    {
        public Category() 
        {
            Id = Guid.NewGuid();
            Items = new List<Item>();
        }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public ICollection<Item> Items { get; set; }
    }
}
