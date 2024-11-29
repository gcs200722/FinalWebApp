namespace FinalWebApp.ViewModels
{
    public class InvoiceViewModel
    {
        public Guid OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string CustomerName { get; set; }
        public string TableName { get; set; }
        public List<InvoiceItemViewModel> Items { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
    }

    public class InvoiceItemViewModel
    {
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total => Quantity * Price;
    }
}
