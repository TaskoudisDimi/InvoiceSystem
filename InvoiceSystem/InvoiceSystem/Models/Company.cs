namespace InvoiceSystem.Models
{
    public class Company
    {
        public required string Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<string> Users { get; set; } = new List<string>();

    }
}
