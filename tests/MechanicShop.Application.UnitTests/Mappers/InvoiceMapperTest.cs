using MechanicShop.Application.Features.Billing.Mappers;
using MechanicShop.Domain.WorkOrders.Billing;
using MechanicShop.Tests.Common.WorkOrders.Billing;

namespace MechanicShop.Application.UnitTests.Mappers;

public class InvoiceMapperTest
{
    [Fact]
    public void ToDto_ShouldMapCorrectly()
    {
        var invoice = InvoiceFactory.CreateInvoice().Value;

        var dto = invoice.ToDto();

        Assert.NotNull(dto);
        Assert.Equal(invoice.Id, dto.InvoiceId);
        Assert.Equal(invoice.WorkOrderId, dto.WorkOrderId);
        Assert.Equal(invoice.IssuedAtUtc, dto.IssuedAtUtc);
        Assert.Equal(invoice.SubTotal, dto.Subtotal);
        Assert.Equal(invoice.TaxAmount, dto.TaxAmount);
        Assert.Equal(invoice.DiscountAmount, dto.DiscountAmount);
        Assert.Equal(invoice.Total, dto.Total);
        Assert.Equal(invoice.Status.ToString(), dto.PaymentStatus);
        Assert.Single(dto.Items);

    }

    [Fact]
    public void ToDto_ShouldThrowException_WhenInvoiceIsNull()
    {
        var invoice = (Invoice)null!;

        Assert.Throws<ArgumentNullException>(invoice.ToDto);
    }

    [Fact]
    public void ToDto_ShouldThrowException_WhenLineItemsIsNull()
    {
        var invoice = InvoiceFactory.CreateInvoice(items: null, setListIfNull: false).Value;

        Assert.Throws<ArgumentNullException>(invoice.ToDto);
    }

    [Fact]
    public void ToDtos_ShouldMapListCorrectly()
    {
        var invoice = InvoiceFactory.CreateInvoice().Value;
        List<Invoice> invoices = [invoice];

        var dtos = invoices.ToDtos();
        Assert.NotNull(dtos);
        Assert.Single(dtos);

        var dto = dtos[0];

        Assert.Equal(invoice.Id, dto.InvoiceId);
        Assert.Equal(invoice.WorkOrderId, dto.WorkOrderId);
        Assert.Equal(invoice.IssuedAtUtc, dto.IssuedAtUtc);
        Assert.Equal(invoice.SubTotal, dto.Subtotal);
        Assert.Equal(invoice.TaxAmount, dto.TaxAmount);
        Assert.Equal(invoice.DiscountAmount, dto.DiscountAmount);
        Assert.Equal(invoice.Total, dto.Total);
        Assert.Equal(invoice.Status.ToString(), dto.PaymentStatus);
        Assert.Single(dto.Items);
    }
}
