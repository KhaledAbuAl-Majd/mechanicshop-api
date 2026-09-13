using MechanicShop.Application.Features.Billing.Commands.SettleInvoice;
using MechanicShop.Domain.WorkOrders.Billing;

namespace MechanicShop.Application.SubcutaneousTests.Features.Billing.Commands.SettleInvoice;

public class SettleInvoiceCommandValidatorTests
{
    private readonly SettleInvoiceCommandValidator _validator = new();

    [Fact]
    public async Task Validate_ShouldFail_WhenIdEmpty()
    {
        var ct = CancellationToken.None;

        var command = new SettleInvoiceCommand(Guid.Empty);

        var result = await _validator.ValidateAsync(command, ct);

        Assert.False(result.IsValid);
        Assert.Equal(InvoiceErrors.IdRequired.Code, result.Errors[0].ErrorCode);
    }


    [Fact]
    public async Task Validate_ShouldSuccess_WhenValidData()
    {
        var ct = CancellationToken.None;

        var command = new SettleInvoiceCommand(Guid.NewGuid());

        var result = await _validator.ValidateAsync(command, ct);

        Assert.True(result.IsValid);
    }
}
