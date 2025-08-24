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

        // we seed data for each test case
        _service.SeedData();
    }

    [Fact]
    public void CreateInvoice_ValidData_ReturnsCreated()
    {
        // Arrange
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

        // Act
        var result = _controller.CreateInvoice(invoice) as CreatedResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(201, result?.StatusCode);
        Assert.Equal($"/invoice/{invoice.InvoiceId}", result?.Location);

        // Verify invoice was saved
        var invoices = _service.GetSentInvoices("compA").ToList();
        Assert.Contains(invoices, i => i.InvoiceId == "test");
    }

    [Fact]
    public void CreateInvoice_DuplicateInvoiceId_ReturnsBadRequest()
    {
        // Arrange
        _controller.HttpContext.Items["CompanyId"] = "compA";
        var invoice = new Invoice
        {
            InvoiceId = "inv1", // Duplicate from seeded data
            CompanyId = "compA",
            CounterPartyCompanyId = "compB",
            DateIssued = "2025-08-21T00:00:00Z",
            NetAmount = 100,
            VatAmount = 20,
            TotalAmount = 120,
            Description = "Duplicate Test"
        };

        // Act
        var result = _controller.CreateInvoice(invoice) as BadRequestObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result?.StatusCode);
        Assert.Contains("already exists", result?.Value.ToString());
    }

    [Fact]
    public void CreateInvoice_Unauthorized_ReturnsUnauthorized()
    {
        // Arrange
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

        // Act
        var result = _controller.CreateInvoice(invoice) as UnauthorizedResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(401, result?.StatusCode);
    }

    [Fact]
    public void CreateInvoice_InvalidInvoiceData_ReturnsBadRequest()
    {
        // Arrange
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

        // Act
        var result = _controller.CreateInvoice(invoice) as BadRequestObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result?.StatusCode);
        Assert.Equal("Invalid invoice data", result?.Value);
    }

    [Fact]
    public void GetSentInvoices_AuthenticatedWithFilters_ReturnsFilteredInvoices()
    {
        // Arrange
        _controller.HttpContext.Items["CompanyId"] = "compA";
        var counterParty = "compB";
        var dateIssued = "2025-08-21T00:00:00Z";
        var invoiceId = "inv1";

        // Act
        var result = _controller.GetSentInvoices(counterParty, dateIssued, invoiceId) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result?.StatusCode);
        var invoices = Assert.IsAssignableFrom<IEnumerable<Invoice>>(result?.Value);
        Assert.Single(invoices);
        var invoice = invoices.First();
        Assert.Equal("inv1", invoice.InvoiceId);
    }

    [Fact]
    public void GetSentInvoices_PartialFilterByCounterParty_ReturnsFilteredInvoices()
    {
        // Arrange
        _controller.HttpContext.Items["CompanyId"] = "compA";
        var counterParty = "compB";

        // Add another invoice with different counter party for testing
        _service.AddInvoice(new Invoice
        {
            InvoiceId = "inv2",
            DateIssued = "2025-08-22T00:00:00Z",
            NetAmount = 200,
            VatAmount = 40,
            TotalAmount = 240,
            Description = "Test2",
            CompanyId = "compA",
            CounterPartyCompanyId = "compC"
        });

        // Act
        var result = _controller.GetSentInvoices(counterParty, null, null) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result?.StatusCode);
        var invoices = Assert.IsAssignableFrom<IEnumerable<Invoice>>(result?.Value).ToList();
        Assert.Single(invoices);
        Assert.Equal("inv1", invoices[0].InvoiceId); // Only matches compB
    }

    [Fact]
    public void GetSentInvoices_PartialFilterByDateIssued_ReturnsFilteredInvoices()
    {
        // Arrange
        _controller.HttpContext.Items["CompanyId"] = "compA";
        var dateIssued = "2025-08-21T00:00:00Z";

        // Add another invoice with different date
        _service.AddInvoice(new Invoice
        {
            InvoiceId = "inv2",
            DateIssued = "2025-08-22T00:00:00Z",
            NetAmount = 200,
            VatAmount = 40,
            TotalAmount = 240,
            Description = "Test2",
            CompanyId = "compA",
            CounterPartyCompanyId = "compB"
        });

        // Act
        var result = _controller.GetSentInvoices(null, dateIssued, null) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result?.StatusCode);
        var invoices = Assert.IsAssignableFrom<IEnumerable<Invoice>>(result?.Value).ToList();
        Assert.Single(invoices);
        Assert.Equal("inv1", invoices[0].InvoiceId); // Only matches the date
    }

    [Fact]
    public void GetSentInvoices_NoFilters_ReturnsAllSentInvoices()
    {
        // Arrange
        _controller.HttpContext.Items["CompanyId"] = "compA";

        // Add another invoice
        _service.AddInvoice(new Invoice
        {
            InvoiceId = "inv2",
            DateIssued = "2025-08-22T00:00:00Z",
            NetAmount = 200,
            VatAmount = 40,
            TotalAmount = 240,
            Description = "Test2",
            CompanyId = "compA",
            CounterPartyCompanyId = "compB"
        });

        // Act
        var result = _controller.GetSentInvoices(null, null, null) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result?.StatusCode);
        var invoices = Assert.IsAssignableFrom<IEnumerable<Invoice>>(result?.Value).ToList();
        Assert.Equal(2, invoices.Count); // Seeded + added
    }

    [Fact]
    public void GetSentInvoices_InvalidFilter_ReturnsEmptyList()
    {
        // Arrange
        _controller.HttpContext.Items["CompanyId"] = "compA";
        var invoiceId = "nonexistent";

        // Act
        var result = _controller.GetSentInvoices(null, null, invoiceId) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result?.StatusCode);
        var invoices = Assert.IsAssignableFrom<IEnumerable<Invoice>>(result?.Value).ToList();
        Assert.Empty(invoices);
    }


    [Fact]
    public void GetReceivedInvoices_AuthenticatedWithFilters_ReturnsFilteredInvoices()
    {
        // Arrange
        _controller.HttpContext.Items["CompanyId"] = "compB";
        var counterParty = "compA";
        var dateIssued = "2025-08-21T00:00:00Z";
        var invoiceId = "inv1";

        // Act
        var result = _controller.GetReceivedInvoices(counterParty, dateIssued, invoiceId) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result?.StatusCode);
        var invoices = Assert.IsAssignableFrom<IEnumerable<Invoice>>(result?.Value);
        Assert.Single(invoices);
        var invoice = invoices.First();
        Assert.Equal("inv1", invoice.InvoiceId);
    }

    [Fact]
    public void GetSentInvoices_NoMatchingInvoices_ReturnsEmptyList()
    {
        // Arrange
        _controller.HttpContext.Items["CompanyId"] = "compA";
        var counterParty = "nonexistent";
        var dateIssued = "2025-08-22T00:00:00Z";
        var invoiceId = "inv999";

        // Act
        var result = _controller.GetSentInvoices(counterParty, dateIssued, invoiceId) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result?.StatusCode);
        var invoices = Assert.IsAssignableFrom<IEnumerable<Invoice>>(result?.Value);
        Assert.Empty(invoices);
    }

    [Fact]
    public void GetSentInvoices_ValidCompanyIdWithFilters_ReturnsFilteredInvoices()
    {
        // Arrange
        _service.AddInvoice(new Invoice
        {
            InvoiceId = "inv2",
            DateIssued = "2025-08-22T00:00:00Z",
            NetAmount = 200,
            VatAmount = 40,
            TotalAmount = 240,
            Description = "Test2",
            CompanyId = "compA",
            CounterPartyCompanyId = "compC"
        });

        // Act
        var result = _service.GetSentInvoices("compA", "compC", null, "inv2");

        // Assert
        var invoices = result.ToList();
        Assert.Single(invoices);
        Assert.Equal("inv2", invoices[0].InvoiceId);
    }

    [Fact]
    public void GetReceivedInvoices_ValidCompanyIdWithFilters_ReturnsFilteredInvoices()
    {
        // Act
        var result = _service.GetReceivedInvoices("compB", "compA", "2025-08-21T00:00:00Z", "inv1");

        // Assert
        var invoices = result.ToList();
        Assert.Single(invoices);
        Assert.Equal("inv1", invoices[0].InvoiceId);
    }
}