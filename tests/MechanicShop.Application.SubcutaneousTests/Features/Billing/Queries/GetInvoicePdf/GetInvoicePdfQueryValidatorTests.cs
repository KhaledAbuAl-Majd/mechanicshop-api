using MechanicShop.Application.Features.Billing.Queries.GetInvoicePdf;
using MechanicShop.Domain.WorkOrders.Billing;

namespace MechanicShop.Application.SubcutaneousTests.Features.Billing.Queries.GetInvoicePdf;

public class GetInvoicePdfQueryValidatorTests
{
    private readonly GetInvoicePdfQueryValidator _validator = new();

    [Fact]
    public async Task Validate_ShouldFail_WhenIdEmpty()
    {
        var ct = CancellationToken.None;

        var query = new GetInvoicePdfQuery(Guid.Empty);

        var result = await _validator.ValidateAsync(query, ct);

        Assert.False(result.IsValid);
        Assert.Equal(InvoiceErrors.IdRequired.Code, result.Errors[0].ErrorCode);
    }


    [Fact]
    public async Task Validate_ShouldSuccess_WhenValidData()
    {
        var ct = CancellationToken.None;

        var query = new GetInvoicePdfQuery(Guid.NewGuid());

        var result = await _validator.ValidateAsync(query, ct);

        Assert.True(result.IsValid);
    }
}
