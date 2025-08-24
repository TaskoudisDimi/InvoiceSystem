using InvoiceSystem.Models;
using InvoicingSystem.Controllers;
using InvoicingSystem.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace InvoicingSystem.Tests;

public class InvoiceControllerTests
{
    private readonly InMemoryDataService _service;
    private readonly InvoiceController _controller;

    public InvoiceControllerTests()
    {
        _service = new InMemoryDataService();
        _controller = new InvoiceController(_service);
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        // Seed data for each test case
        _service.SeedData();
    }

    [Fact]
    public void CreateInvoice_ValidData_ReturnsCreated()
    {
        _controller.HttpContext.Items["CompanyId"] = "compA";
        var invoice = new Invoice
        {
            InvoiceId = "test",
            CompanyId = "compA",
            CounterPartyCompanyId = "compB",
            DateIssued = "2025-08-21T00:00:00Z",
            NetAmount = 100,
            VatAmount = 20,
            TotalAmount = 120,
            Description = "Test"
        };

        var result = _controller.CreateInvoice(invoice) as CreatedResult;

        Assert.NotNull(result);
        Assert.Equal(201, result?.StatusCode);
        Assert.Equal($"/invoice/{invoice.InvoiceId}", result?.Location);

        var invoices = _service.GetSentInvoices("compA").ToList();
        Assert.Contains(invoices, i => i.InvoiceId == "test");
    }

    [Fact]
    public void CreateInvoice_DuplicateInvoiceId_ReturnsBadRequest()
    {
        _controller.HttpContext.Items["CompanyId"] = "compA";
        var invoice = new Invoice
        {
            InvoiceId = "inv1",
            CompanyId = "compA",
            CounterPartyCompanyId = "compB",
            DateIssued = "2025-08-21T00:00:00Z",
            NetAmount = 100,
            VatAmount = 20,
            TotalAmount = 120,
            Description = "Duplicate Test"
        };

        var result = _controller.CreateInvoice(invoice) as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result?.StatusCode);
        Assert.Contains("already exists", result?.Value.ToString());
    }

    [Fact]
    public void CreateInvoice_Unauthorized_ReturnsUnauthorized()
    {
        _controller.HttpContext.Items["CompanyId"] = null;
        var invoice = new Invoice
        {
            InvoiceId = "test",
            CompanyId = "compA",
            CounterPartyCompanyId = "compB",
            DateIssued = "2025-08-21T00:00:00Z",
            NetAmount = 100,
            VatAmount = 20,
            TotalAmount = 120,
            Description = "Test"
        };

        var result = _controller.CreateInvoice(invoice) as UnauthorizedResult;

        Assert.NotNull(result);
        Assert.Equal(401, result?.StatusCode);
    }

    [Fact]
    public void CreateInvoice_InvalidInvoiceData_ReturnsBadRequest()
    {
        _controller.HttpContext.Items["CompanyId"] = "compA";
        var invoice = new Invoice
        {
            InvoiceId = "",
            CompanyId = "compA",
            CounterPartyCompanyId = "compB",
            DateIssued = "2025-08-21T00:00:00Z",
            NetAmount = 100,
            VatAmount = 20,
            TotalAmount = 120,
            Description = "Test"
        };

        var result = _controller.CreateInvoice(invoice) as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result?.StatusCode);
        Assert.Equal("Invalid invoice data", result?.Value);
    }

    [Fact]
    public void GetSentInvoices_AuthenticatedWithFilters_ReturnsFilteredInvoices()
    {
        _controller.HttpContext.Items["CompanyId"] = "compA";
        var counterParty = "compB";
        var dateIssued = "2025-08-21T00:00:00Z";
        var invoiceId = "inv1";

        var result = _controller.GetSentInvoices(counterParty, date_issued: dateIssued, invoice_id: invoiceId) as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result?.StatusCode);
        var invoices = Assert.IsAssignableFrom<IEnumerable<Invoice>>(result?.Value);
        Assert.Single(invoices);
        var invoice = invoices.First();
        Assert.Equal("inv1", invoice.InvoiceId);
    }

    [Fact]
    public void GetSentInvoices_Unauthenticated_ReturnsDefaultCompanyInvoices()
    {
        _controller.HttpContext.Items["CompanyId"] = null;

        var result = _controller.GetSentInvoices(null, null, null) as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result?.StatusCode);
        var invoices = Assert.IsAssignableFrom<IEnumerable<Invoice>>(result?.Value).ToList();
        Assert.Single(invoices);
        Assert.Equal("inv1", invoices[0].InvoiceId); 
    }

    [Fact]
    public void GetSentInvoices_PartialFilterByCounterParty_ReturnsFilteredInvoices()
    {
        _controller.HttpContext.Items["CompanyId"] = "compA";
        var counterParty = "compB";

        _service.AddInvoice(new Invoice
        {
            InvoiceId = "inv5", 
            DateIssued = "2025-08-23T00:00:00Z",
            NetAmount = 500,
            VatAmount = 100,
            TotalAmount = 600,
            Description = "Test5",
            CompanyId = "compA",
            CounterPartyCompanyId = "compC" 
        });

        var result = _controller.GetSentInvoices(counterParty, null, null) as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result?.StatusCode);
        var invoices = Assert.IsAssignableFrom<IEnumerable<Invoice>>(result?.Value).ToList();
        Assert.Single(invoices);
        Assert.Equal("inv1", invoices[0].InvoiceId); 
    }

    [Fact]
    public void GetSentInvoices_PartialFilterByDateIssued_ReturnsFilteredInvoices()
    {
        _controller.HttpContext.Items["CompanyId"] = "compA";
        var dateIssued = "2025-08-21T00:00:00Z";

        _service.AddInvoice(new Invoice
        {
            InvoiceId = "inv4", 
            DateIssued = "2025-08-23T00:00:00Z", 
            NetAmount = 400,
            VatAmount = 80,
            TotalAmount = 480,
            Description = "Test4",
            CompanyId = "compA",
            CounterPartyCompanyId = "compB"
        });

        var result = _controller.GetSentInvoices(null, date_issued: dateIssued, null) as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result?.StatusCode);
        var invoices = Assert.IsAssignableFrom<IEnumerable<Invoice>>(result?.Value).ToList();
        Assert.Single(invoices);
        Assert.Equal("inv1", invoices[0].InvoiceId);
    }

    [Fact]
    public void GetSentInvoices_NoFilters_ReturnsAllSentInvoices()
    {
        _controller.HttpContext.Items["CompanyId"] = "compA";

        _service.AddInvoice(new Invoice
        {
            InvoiceId = "inv3", 
            DateIssued = "2025-08-23T00:00:00Z", 
            NetAmount = 300,
            VatAmount = 60,
            TotalAmount = 360,
            Description = "Test3",
            CompanyId = "compA",
            CounterPartyCompanyId = "compB"
        });

        var result = _controller.GetSentInvoices(null, null, null) as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result?.StatusCode);
        var invoices = Assert.IsAssignableFrom<IEnumerable<Invoice>>(result?.Value).ToList();
        Assert.Equal(2, invoices.Count); 
        Assert.Contains(invoices, i => i.InvoiceId == "inv1");
        Assert.Contains(invoices, i => i.InvoiceId == "inv3");
    }

    [Fact]
    public void GetSentInvoices_InvalidFilter_ReturnsEmptyList()
    {
        _controller.HttpContext.Items["CompanyId"] = "compA";
        var invoiceId = "nonexistent";

        var result = _controller.GetSentInvoices(null, null, invoice_id: invoiceId) as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result?.StatusCode);
        var invoices = Assert.IsAssignableFrom<IEnumerable<Invoice>>(result?.Value).ToList();
        Assert.Empty(invoices);
    }

    [Fact]
    public void GetReceivedInvoices_AuthenticatedWithFilters_ReturnsFilteredInvoices()
    {
        _controller.HttpContext.Items["CompanyId"] = "compB";
        var counterParty = "compA";
        var dateIssued = "2025-08-21T00:00:00Z";
        var invoiceId = "inv1";

        var result = _controller.GetReceivedInvoices(counterParty, date_issued: dateIssued, invoice_id: invoiceId) as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result?.StatusCode);
        var invoices = Assert.IsAssignableFrom<IEnumerable<Invoice>>(result?.Value);
        Assert.Single(invoices);
        var invoice = invoices.First();
        Assert.Equal("inv1", invoice.InvoiceId);
    }

    [Fact]
    public void GetReceivedInvoices_Unauthenticated_ReturnsDefaultCompanyInvoices()
    {
        _controller.HttpContext.Items["CompanyId"] = null;

        var result = _controller.GetReceivedInvoices(null, null, null) as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result?.StatusCode);
        var invoices = Assert.IsAssignableFrom<IEnumerable<Invoice>>(result?.Value).ToList();
        Assert.Single(invoices);
        Assert.Equal("inv2", invoices[0].InvoiceId); 
    }
}