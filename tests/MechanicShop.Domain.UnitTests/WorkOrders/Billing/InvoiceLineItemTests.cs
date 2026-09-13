using MechanicShop.Domain.WorkOrders.Billing.InvoiceLineItems;
using MechanicShop.Tests.Common.WorkOrders.Billing;

namespace MechanicShop.Domain.UnitTests.WorkOrders.Billing;

public class InvoiceLineItemTests
{
    [Fact]
    public void Create_ShouldReturnError_WhenInvoiceIdEmpty()
    {
        var result = InvoiceLineItemFactory.CreateInvoiceLineItem(invoiceId: Guid.Empty);

        Assert.False(result.IsSuccess);
        Assert.Equal(InvoiceLineItemErrors.InvoiceIdRequired.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_ShouldReturnError_WhenLineNumberInvalid(int lineNumber)
    {
        var result = InvoiceLineItemFactory.CreateInvoiceLineItem(lineNumber: lineNumber);

        Assert.False(result.IsSuccess);
        Assert.Equal(InvoiceLineItemErrors.LineNumberInvalid.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldReturnError_WhenDescriptionInvalid(string? description)
    {
        var result = InvoiceLineItemFactory.CreateInvoiceLineItem(description: description);

        Assert.False(result.IsSuccess);
        Assert.Equal(InvoiceLineItemErrors.DescriptionRequired.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_ShouldReturnError_WhenQuantityInvalid(int quantity)
    {
        var result = InvoiceLineItemFactory.CreateInvoiceLineItem(quantity: quantity);

        Assert.False(result.IsSuccess);
        Assert.Equal(InvoiceLineItemErrors.QuantityInvalid.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-0.5)]
    public void Create_ShouldReturnError_WhenUnitPriceInvalid(decimal unitPrice)
    {
        var result = InvoiceLineItemFactory.CreateInvoiceLineItem(unitPrice: unitPrice);

        Assert.False(result.IsSuccess);
        Assert.Equal(InvoiceLineItemErrors.UnitPriceInvalid.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WhenValidData()
    {
        var invoiceId = Guid.NewGuid();
        int lineNumber = 1;
        string description = "item description";
        int quantity = 3;
        decimal unitPrice = 80m;


        var result = InvoiceLineItemFactory.CreateInvoiceLineItem(
            invoiceId: invoiceId,
            lineNumber: lineNumber,
            description: description,
            quantity: quantity,
            unitPrice: unitPrice);

        Assert.True(result.IsSuccess);
        var item = result.Value;
        Assert.NotNull(item);
        Assert.Equal(invoiceId, item.InvoiceId);
        Assert.Equal(lineNumber, item.LineNumber);
        Assert.Equal(description, item.Description);
        Assert.Equal(quantity, item.Quantity);
        Assert.Equal(unitPrice, item.UnitPrice);
        Assert.Equal(quantity * unitPrice, item.LineTotal);
    }
}
