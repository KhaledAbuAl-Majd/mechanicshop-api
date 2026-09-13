using MechanicShop.Application.Features.Customers.Commands.CreateCustomer;

namespace MechanicShop.Application.SubcutaneousTests.Features.Customers.Commands.CreateCustomer;

public class CreateCustomerCommandValidatorTests
{
    private readonly CreateCustomerCommandValidator _validator = new();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Validate_ShouldFail_WhenNameInvalid(string? name)
    {
        CancellationToken ct = CancellationToken.None;
        var vehicleComand = new CreateVehicleCommand(Make: "bmw", "m5", 2025, "tes|233");
        var command = new CreateCustomerCommand(Name: name!, "+2093857355", "khaled@gmail.com", [vehicleComand]);

        var result = await _validator.ValidateAsync(command, ct);

        Assert.False(result.IsValid);
        Assert.Equal(nameof(command.Name), result.Errors[0].PropertyName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("dfdadfd")]
    public async Task Validate_ShouldFail_WhenEmailInvalid(string? email)
    {
        CancellationToken ct = CancellationToken.None;
        var vehicleComand = new CreateVehicleCommand(Make: "bmw", "m5", 2025, "tes|233");
        var command = new CreateCustomerCommand(Name: "khaled", "+2093857355", Email: email!, [vehicleComand]);

        var result = await _validator.ValidateAsync(command, ct);

        Assert.False(result.IsValid);
        Assert.Equal(nameof(command.Email), result.Errors[0].PropertyName);
    }


    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("56565656565656545454")]
    [InlineData("565")]
    [InlineData("++434343434")]
    [InlineData("dfdakddkd")]
    public async Task Validate_ShouldFail_WhenPhoneInvalid(string? phone)
    {
        CancellationToken ct = CancellationToken.None;
        var vehicleComand = new CreateVehicleCommand(Make: "bmw", "m5", 2025, "tes|233");
        var command = new CreateCustomerCommand(Name: "khaled", PhoneNumber: phone!, "khaled@gmail.com", [vehicleComand]);

        var result = await _validator.ValidateAsync(command, ct);

        Assert.False(result.IsValid);
        Assert.Equal(nameof(command.PhoneNumber), result.Errors[0].PropertyName);
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenVehiclesEmpty()
    {
        CancellationToken ct = CancellationToken.None;

        var command = new CreateCustomerCommand("khaled", "+2093857355", "khaled@gmail.com", []);

        var result = await _validator.ValidateAsync(command, ct);

        Assert.False(result.IsValid);
        Assert.Equal(nameof(command.Vehicles), result.Errors[0].PropertyName);
    }

    [Fact]
    public async Task Validate_ShouldSuccess_WhenValidData()
    {
        CancellationToken ct = CancellationToken.None;

        var vehicleComand = new CreateVehicleCommand(Make: "bmw", "m5", 2025, "tes|233");
        var command = new CreateCustomerCommand("khaled", "+2093857355", "khaled@gmail.com", [vehicleComand]);

        var result = await _validator.ValidateAsync(command, ct);

        Assert.True(result.IsValid);
    }

}
