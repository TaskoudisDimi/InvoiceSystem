namespace InvoiceSystem.Models
{
    public class Invoice
    {
        public required string InvoiceId { get; set; }
        public string DateIssued { get; set; } = string.Empty; 
        public float NetAmount { get; set; }
        public float VatAmount { get; set; }
        public float TotalAmount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string CompanyId { get; set; } = string.Empty;  
        public string CounterPartyCompanyId { get; set; } = string.Empty;  
    }
}
