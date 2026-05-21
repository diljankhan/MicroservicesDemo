using System.Text.Json.Serialization;

namespace Demo.Services.Catalog.API.Models
{
    public class Product
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)] // Hides it if it is 0
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }

    }
}
