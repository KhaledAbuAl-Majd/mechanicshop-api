using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Billing;
using MechanicShop.Domain.WorkOrders.Billing.Enums;
using MechanicShop.Domain.WorkOrders.Billing.InvoiceLineItems;
using MechanicShop.Tests.Common;
using MechanicShop.Tests.Common.WorkOrders.Billing;

namespace MechanicShop.Domain.UnitTests.WorkOrders.Billing;

public class InvoiceTests
{
    [Fact]
    public void Create_ShouldReturnError_WhenIdEmpty()
    {
        var result = InvoiceFactory.CreateInvoice(id: Guid.Empty);

        Assert.False(result.IsSuccess);
        Assert.Equal(InvoiceErrors.IdRequired.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnError_WhenWorkOrderIdInvalid()
    {
        var result = InvoiceFactory.CreateInvoice(workOrderId: Guid.Empty);

        Assert.False(result.IsSuccess);
        Assert.Equal(InvoiceErrors.WorkOrderIdInvalid.Code, result.TopError.Code);
    }

    [Theory]
    [MemberData(nameof(InvalidInvoiceLineItemsData))]
    public void Create_ShouldReturnError_WhenInvoiceLineItemsInvalid(List<InvoiceLineItem>? items)
    {
        var result = Invoice.Create(
            id: Guid.NewGuid(),
            workOrderId: Guid.NewGuid(),
            items: items!,
            discountAmount: 0,
            taxAmount: 0,
            datetime: TimeProvider.System);


        InvoiceFactory.CreateInvoice(items: items);

        Assert.False(result.IsSuccess);
        Assert.Equal(InvoiceErrors.LineItemsEmpty.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnError_WhenDiscountAmountInvalid()
    {
        var result = InvoiceFactory.CreateInvoice(discountAmount: -1);

        Assert.False(result.IsSuccess);
        Assert.Equal(InvoiceErrors.DiscountNegative.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnError_WhenDiscountExceedsSubtotal()
    {
        List<InvoiceLineItem> lineItems = [InvoiceLineItemFactory.CreateInvoiceLineItem().Value, InvoiceLineItemFactory.CreateInvoiceLineItem().Value];

        var subTotal = lineItems.Sum(x => x.LineTotal);

        var discount = subTotal + 1;

        var result = InvoiceFactory.CreateInvoice(items: lineItems, discountAmount: discount);

        Assert.False(result.IsSuccess);
        Assert.Equal(InvoiceErrors.DiscountExceedsSubtotal.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnError_WhenTaxAmountInvalid()
    {
        var result = InvoiceFactory.CreateInvoice(taxAmount: -1);

        Assert.False(result.IsSuccess);
        Assert.Equal(InvoiceErrors.TaxNegative.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WhenValidData()
    {
        var time = new FakeTimeProvider();
        time.SetUtcNow(DateTimeOffset.UtcNow.AddYears(-3));//static time provider

        var id = Guid.NewGuid();
        Guid workOrderId = Guid.NewGuid();
        List<InvoiceLineItem> lineItems = [InvoiceLineItemFactory.CreateInvoiceLineItem().Value, InvoiceLineItemFactory.CreateInvoiceLineItem().Value];
        decimal discount = 0;
        decimal taxAmount = 0;
        var subTotal = lineItems.Sum(x => x.LineTotal);
        var total = subTotal - discount + taxAmount;

        var result = InvoiceFactory.CreateInvoice(id: id, workOrderId: workOrderId, items: lineItems, discountAmount: discount, taxAmount: taxAmount, datetime: time);

        Assert.True(result.IsSuccess);
        var invoice = result.Value;
        Assert.NotNull(invoice);
        Assert.Equal(id, invoice.Id);
        Assert.Equal(workOrderId, invoice.WorkOrderId);
        Assert.Equal(2, invoice.LineItems.Count);
        Assert.Equal(discount, invoice.DiscountAmount);
        Assert.Equal(taxAmount, invoice.TaxAmount);
        Assert.Equal(subTotal, invoice.SubTotal);
        Assert.Equal(total, invoice.Total);
        Assert.Equal(time.GetUtcNow(), invoice.IssuedAtUtc);
        Assert.Equal(InvoiceStatus.Unpaid, invoice.Status);
    }

    [Fact]
    public void ApplyDiscount_ShouldReturnError_WhenInvoiceLocked()
    {
        var invoice = InvoiceFactory.CreateInvoice().Value;

        invoice.MarkAsPaid(TimeProvider.System);

        var result = invoice.ApplyDiscount(10);

        Assert.False(result.IsSuccess);
        Assert.Equal(InvoiceErrors.InvoiceLocked.Code, result.TopError.Code);
    }

    [Fact]
    public void ApplyDiscount_ShouldReturnError_WhenDiscountAmountInvalid()
    {
        var invoice = InvoiceFactory.CreateInvoice().Value;

        var result = invoice.ApplyDiscount(-1);

        Assert.False(result.IsSuccess);
        Assert.Equal(InvoiceErrors.DiscountNegative.Code, result.TopError.Code);
    }

    [Fact]
    public void ApplyDiscount_ShouldReturnError_WhenDiscountExceedsSubtotal()
    {
        List<InvoiceLineItem> lineItems = [InvoiceLineItemFactory.CreateInvoiceLineItem().Value, InvoiceLineItemFactory.CreateInvoiceLineItem().Value];

        var subTotal = lineItems.Sum(x => x.LineTotal);

        var discount = subTotal + 1;

        var invoice = InvoiceFactory.CreateInvoice(items: lineItems).Value;

        var result = invoice.ApplyDiscount(discount);

        Assert.False(result.IsSuccess);
        Assert.Equal(InvoiceErrors.DiscountExceedsSubtotal.Code, result.TopError.Code);
    }

    [Fact]
    public void ApplyDiscount_ShouldReturnSuccess_AndAssignDiscount()
    {
        var invoice = InvoiceFactory.CreateInvoice().Value;

        decimal discount = 10;
        var originalTotal = invoice.Total;

        var result = invoice.ApplyDiscount(discount);

        Assert.True(result.IsSuccess);
        Assert.Equal(Result.Updated, result.Value);
        Assert.Equal(discount, invoice.DiscountAmount);
        Assert.Equal(originalTotal - discount, invoice.Total);
    }


    [Fact]
    public void MarkAsPaid_ShouldReturnError_WhenInvoiceLocked()
    {
        var time = TimeProvider.System;

        var invoice = InvoiceFactory.CreateInvoice().Value;

        invoice.MarkAsPaid(time);

        var result = invoice.MarkAsPaid(time);

        Assert.False(result.IsSuccess);
        Assert.Equal(InvoiceErrors.InvoiceLocked.Code, result.TopError.Code);
    }

    [Fact]
    public void MarkAsPaid_ShouldReturnSuccess_WhenValidData()
    {
        var time = new FakeTimeProvider();
        time.SetUtcNow(DateTimeOffset.UtcNow.AddYears(-3));

        var invoice = InvoiceFactory.CreateInvoice().Value;

        var result = invoice.MarkAsPaid(time);

        Assert.True(result.IsSuccess);
        Assert.Equal(Result.Updated, result.Value);
        Assert.Equal(InvoiceStatus.Paid, invoice.Status);
        Assert.Equal(time.GetUtcNow(), invoice.PaidAt);
    }


    public static TheoryData<List<InvoiceLineItem>> InvalidInvoiceLineItemsData() => new TheoryData<List<InvoiceLineItem>>
    {
        null!,
        new List<InvoiceLineItem>()
    };
}
