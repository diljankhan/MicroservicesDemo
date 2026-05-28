namespace Demo.Services.Catalog.API.Events
{
    public class ProductUpdatedEvent
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
}
