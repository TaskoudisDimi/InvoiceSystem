using InvoiceSystem.Models;

namespace InvoicingSystem.Services;

public class InMemoryDataService : IDataService
{
    private readonly List<Company> _companies = new();
    private readonly List<Invoice> _invoices = new();

    public void AddCompany(Company company)
    {
        _companies.Add(company);
    }

    public Company? GetCompanyById(string id)
    {
        return _companies.FirstOrDefault(c => c.Id == id);
    }

    public void AddInvoice(Invoice invoice)
    {
        // Prevent duplicates based on InvoiceId
        if (_invoices.Any(i => i.InvoiceId == invoice.InvoiceId))
        {
            throw new InvalidOperationException($"Invoice with ID '{invoice.InvoiceId}' already exists.");
        }
        _invoices.Add(invoice);
    }

    public IEnumerable<Invoice> GetSentInvoices(string companyId, string? counterParty = null, string? dateIssued = null, string? invoiceId = null)
    {
        if (string.IsNullOrEmpty(companyId)) throw new ArgumentNullException(nameof(companyId), "Company ID cannot be null or empty.");
        var query = _invoices.Where(i => i.CompanyId == companyId);
        if (!string.IsNullOrEmpty(counterParty)) query = query.Where(i => i.CounterPartyCompanyId == counterParty);
        if (!string.IsNullOrEmpty(dateIssued)) query = query.Where(i => i.DateIssued == dateIssued);
        if (!string.IsNullOrEmpty(invoiceId)) query = query.Where(i => i.InvoiceId == invoiceId);
        return query;
    }

    public IEnumerable<Invoice> GetReceivedInvoices(string companyId, string? counterParty = null, string? dateIssued = null, string? invoiceId = null)
    {
        if (string.IsNullOrEmpty(companyId)) throw new ArgumentNullException(nameof(companyId), "Company ID cannot be null or empty.");
        var query = _invoices.Where(i => i.CounterPartyCompanyId == companyId);
        if (!string.IsNullOrEmpty(counterParty)) query = query.Where(i => i.CompanyId == counterParty);
        if (!string.IsNullOrEmpty(dateIssued)) query = query.Where(i => i.DateIssued == dateIssued);
        if (!string.IsNullOrEmpty(invoiceId)) query = query.Where(i => i.InvoiceId == invoiceId);
        return query;
    }

    public void SeedData()
    {
        // we use some data to simulate the process
        var companyA = new Company { Id = "compA", Name = "Company A", Users = new List<string> { "user1" } };
        var companyB = new Company { Id = "compB", Name = "Company B", Users = new List<string> { "user2" } };
        AddCompany(companyA);
        AddCompany(companyB);

        AddInvoice(new Invoice
        {
            InvoiceId = "inv1",
            DateIssued = "2025-08-21T00:00:00Z",
            NetAmount = 100,
            VatAmount = 20,
            TotalAmount = 120,
            Description = "Service",
            CompanyId = "compA",
            CounterPartyCompanyId = "compB"
        });
    }
}