namespace Demo.Services.Orders.API.Models
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }
        public int CustomerId { get; set; } // Logical link only!
        public int ProductId { get; set; }  // Logical link only!
    }
}
