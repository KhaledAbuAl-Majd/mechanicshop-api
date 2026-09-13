using MechanicShop.Application.Features.Customers.Commands.RemoveCustomer;

namespace MechanicShop.Application.SubcutaneousTests.Features.Customers.Commands.RemoveCustomer;

public class RemoveCustomerCommandValidatorTests
{
    private readonly RemoveCustomerCommandValidator _validator = new();

    [Fact]
    public async Task Validate_ShouldFail_WhenCustomerIdInvalid()
    {
        CancellationToken ct = CancellationToken.None;
        var command = new RemoveCustomerCommand(Guid.Empty);

        var result = await _validator.ValidateAsync(command, ct);

        Assert.False(result.IsValid);
        Assert.Equal(nameof(command.CustomerId), result.Errors[0].PropertyName);
    }


    [Fact]
    public async Task Validate_ShouldSuccess_WhenValidData()
    {
        CancellationToken ct = CancellationToken.None;
        var command = new RemoveCustomerCommand(Guid.NewGuid());

        var result = await _validator.ValidateAsync(command, ct);

        Assert.True(result.IsValid);
    }
}
