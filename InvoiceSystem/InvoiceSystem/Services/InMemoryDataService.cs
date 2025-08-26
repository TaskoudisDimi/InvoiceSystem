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

    // Adds a new invoice, ensuring no duplicate InvoiceId exists
    public void AddInvoice(Invoice invoice)
    {
        if (_invoices.Any(i => i.InvoiceId == invoice.InvoiceId))
        {
            throw new InvalidOperationException($"Invoice with ID '{invoice.InvoiceId}' already exists.");
        }
        _invoices.Add(invoice);
    }

    // Retrieves sent invoices with optional filtering
    public IEnumerable<Invoice> GetSentInvoices(string companyId, string? counterParty = null, string? dateIssued = null, string? invoiceId = null)
    {
        var query = _invoices.Where(i => i.CompanyId == companyId);
        if (!string.IsNullOrEmpty(counterParty)) query = query.Where(i => i.CounterPartyCompanyId == counterParty);
        if (!string.IsNullOrEmpty(dateIssued)) query = query.Where(i => i.DateIssued == dateIssued);
        if (!string.IsNullOrEmpty(invoiceId)) query = query.Where(i => i.InvoiceId == invoiceId);
        return query;
    }

    // Retrieves received invoices with optional filtering
    public IEnumerable<Invoice> GetReceivedInvoices(string companyId, string? counterParty = null, string? dateIssued = null, string? invoiceId = null)
    {
        var query = _invoices.Where(i => i.CounterPartyCompanyId == companyId);
        if (!string.IsNullOrEmpty(counterParty)) query = query.Where(i => i.CompanyId == counterParty);
        if (!string.IsNullOrEmpty(dateIssued)) query = query.Where(i => i.DateIssued == dateIssued);
        if (!string.IsNullOrEmpty(invoiceId)) query = query.Where(i => i.InvoiceId == invoiceId);
        return query;
    }

    // Seeds initial data for testing
    public void SeedData()
    {
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
        AddInvoice(new Invoice
        {
            InvoiceId = "inv2",
            DateIssued = "2025-08-22T00:00:00Z",
            NetAmount = 200,
            VatAmount = 40,
            TotalAmount = 240,
            Description = "Service from compB",
            CompanyId = "compB",
            CounterPartyCompanyId = "compA"
        });
    }
}