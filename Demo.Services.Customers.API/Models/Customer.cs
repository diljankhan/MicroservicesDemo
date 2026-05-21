using System.Text.Json.Serialization;

namespace Demo.Services.Customers.API.Models
{
    public class Customer
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)] // Hides it if it is 0
        public int Id { get; set; }


        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
