using InvoiceSystem.Models;
using InvoicingSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace InvoicingSystem.Controllers;

[ApiController]
[Route("[controller]")]
public class InvoiceController : ControllerBase
{
    private readonly IDataService _dataService;

    public InvoiceController(IDataService dataService)
    {
        _dataService = dataService;
    }

    // create a new invoice
    [HttpPost]
    public IActionResult CreateInvoice([FromBody] Invoice invoice)
    {
        var companyId = HttpContext.Items["CompanyId"]?.ToString();
        if (string.IsNullOrEmpty(companyId)) return Unauthorized();

        if (string.IsNullOrEmpty(invoice.InvoiceId) || string.IsNullOrEmpty(invoice.CompanyId) || invoice.CompanyId != companyId)
            return BadRequest("Invalid invoice data");

        try
        {
            _dataService.AddInvoice(invoice);
            return Created($"/invoice/{invoice.InvoiceId}", invoice);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // get sent invoices with optional filters
    [HttpGet("sent")]
    public IActionResult GetSentInvoices([FromQuery] string? counter_party_company, [FromQuery] string? date_issued, [FromQuery] string? invoice_id)
    {
        var companyId = HttpContext.Items["CompanyId"]?.ToString() ?? "compA"; 
        var invoices = _dataService.GetSentInvoices(companyId, counter_party_company, date_issued, invoice_id);
        return Ok(invoices);
    }

    // get received invoices with optional filters
    [HttpGet("received")]
    public IActionResult GetReceivedInvoices([FromQuery] string? counter_party_company, [FromQuery] string? date_issued, [FromQuery] string? invoice_id)
    {
        var companyId = HttpContext.Items["CompanyId"]?.ToString() ?? "compA"; 
        var invoices = _dataService.GetReceivedInvoices(companyId, counter_party_company, date_issued, invoice_id);
        return Ok(invoices);
    }
}