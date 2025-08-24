using InvoiceSystem.Models;

namespace InvoicingSystem.Services;

public interface IDataService
{
    void AddCompany(Company company);
    Company? GetCompanyById(string id);
    void AddInvoice(Invoice invoice);
    IEnumerable<Invoice> GetSentInvoices(string companyId, string? counterParty = null, string? dateIssued = null, string? invoiceId = null);
    IEnumerable<Invoice> GetReceivedInvoices(string companyId, string? counterParty = null, string? dateIssued = null, string? invoiceId = null);
    void SeedData();
}