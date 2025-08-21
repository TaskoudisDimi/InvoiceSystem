using InvoiceSystem;
using InvoiceSystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace InvoicingSystem.Controllers;

[ApiController]
[Route("[controller]")]
public class InvoiceController : ControllerBase
{
    private readonly InMemoryDataService _dataService;

    public InvoiceController(InMemoryDataService dataService)
    {
        _dataService = dataService;
    }

    [HttpPost]
    public IActionResult CreateInvoice([FromBody] Invoice invoice)
    {
        var companyId = HttpContext.Items["CompanyId"]?.ToString();
        if (string.IsNullOrEmpty(companyId)) return Unauthorized();

        if (string.IsNullOrEmpty(invoice.InvoiceId) || string.IsNullOrEmpty(invoice.CompanyId) || invoice.CompanyId != companyId)
            return BadRequest("Invalid invoice data");

        _dataService.AddInvoice(invoice);
        return Created($"/invoice/{invoice.InvoiceId}", invoice);
    }

    [HttpGet("sent")]
    public IActionResult GetSentInvoices([FromQuery] string? counter_party_company, [FromQuery] string? date_issued, [FromQuery] string? invoice_id)
    {
        var companyId = HttpContext.Items["CompanyId"]?.ToString();
        if (string.IsNullOrEmpty(companyId)) return Unauthorized();

        var invoices = _dataService.GetSentInvoices(companyId, counter_party_company, date_issued, invoice_id);
        return Ok(invoices);
    }

    [HttpGet("received")]
    public IActionResult GetReceivedInvoices([FromQuery] string? counter_party_company, [FromQuery] string? date_issued, [FromQuery] string? invoice_id)
    {
        var companyId = HttpContext.Items["CompanyId"]?.ToString();
        if (string.IsNullOrEmpty(companyId)) return Unauthorized();

        var invoices = _dataService.GetReceivedInvoices(companyId, counter_party_company, date_issued, invoice_id);
        return Ok(invoices);
    }
}