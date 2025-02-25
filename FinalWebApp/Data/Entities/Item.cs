﻿namespace FinalWebApp.Data.Entities
{
    public class Item
    {
        public Item()
        {
            Id = Guid.NewGuid();
            OrderItems = new List<OrderItem>();
        }
        public Guid Id { get; set; }

        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? Image { get; set; }
        public Guid CategoryId { get; set; }
        public virtual Category? Category { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }

    }
}
