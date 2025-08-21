using InvoiceSystem.Models;

namespace InvoiceSystem
{
    public class InMemoryDataService
    {
        
        private readonly List<Company> _companies = new();
        private readonly List<Invoice> _invoices = new();

        public void AddCompany(Company company) => _companies.Add(company);
        public Company? GetCompanyById(string id) => _companies.FirstOrDefault(c => c.Id == id);

        public void AddInvoice(Invoice invoice) => _invoices.Add(invoice);

        public IEnumerable<Invoice> GetSentInvoices(string companyId, string? counterParty = null, string? dateIssued = null, string? invoiceId = null)
        {
            var query = _invoices.Where(i => i.CompanyId == companyId);
            if (!string.IsNullOrEmpty(counterParty)) query = query.Where(i => i.CounterPartyCompanyId == counterParty);
            if (!string.IsNullOrEmpty(dateIssued)) query = query.Where(i => i.DateIssued == dateIssued);
            if (!string.IsNullOrEmpty(invoiceId)) query = query.Where(i => i.InvoiceId == invoiceId);
            return query;
        }

        public IEnumerable<Invoice> GetReceivedInvoices(string companyId, string? counterParty = null, string? dateIssued = null, string? invoiceId = null)
        {
            var query = _invoices.Where(i => i.CounterPartyCompanyId == companyId);
            if (!string.IsNullOrEmpty(counterParty)) query = query.Where(i => i.CompanyId == counterParty);
            if (!string.IsNullOrEmpty(dateIssued)) query = query.Where(i => i.DateIssued == dateIssued);
            if (!string.IsNullOrEmpty(invoiceId)) query = query.Where(i => i.InvoiceId == invoiceId);
            return query;
        }

        public void SeedData()
        {
            var companyA = new Company { Id = "companyA", Name = "Company A", Users = new List<string> { "user1" } };
            var companyB = new Company { Id = "companyB", Name = "Company B", Users = new List<string> { "user2" } };
            AddCompany(companyA);
            AddCompany(companyB);

            AddInvoice(new Invoice { InvoiceId = "inv1", DateIssued = "2025-08-21T00:00:00Z", NetAmount = 100, VatAmount = 20, TotalAmount = 120, Description = "Service", CompanyId = "compA", CounterPartyCompanyId = "compB" });
        }
    }
}
