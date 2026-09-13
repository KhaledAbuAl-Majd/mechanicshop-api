using System.Security.Cryptography;
using MechanicShop.Application.Features.Customers.Commands.CreateCustomer;

namespace MechanicShop.Application.SubcutaneousTests.Features.Customers.Commands.CreateCustomer;

public class CreateVehicleCommandValidatorTests
{
    private readonly CreateVehicleCommandValidator _validator = new();

    [Theory]
    [MemberData(nameof(GetInvalidMake))]
    public async Task Validate_ShouldFail_WhenMakeNotValid(string? make)
    {
        var ct = CancellationToken.None;
        var command = new CreateVehicleCommand(Make: make!, "m5", 2025, "tes|233");

        var result = await _validator.ValidateAsync(command, ct);

        Assert.False(result.IsValid);
        Assert.Equal(nameof(command.Make), result.Errors[0].PropertyName);
    }

    [Theory]
    [MemberData(nameof(GetInvalidModel))]
    public async Task Validate_ShouldFail_WhenModelNotValid(string? model)
    {
        var ct = CancellationToken.None;
        var command = new CreateVehicleCommand("bmw"!, Model: model!, 2025, "tes|233");

        var result = await _validator.ValidateAsync(command, ct);

        Assert.False(result.IsValid);
        Assert.Equal(nameof(command.Model), result.Errors[0].PropertyName);
    }

    [Theory]
    [MemberData(nameof(GetInvalidLicensePlate))]
    public async Task Validate_ShouldFail_WhenLicensePlateNotValid(string? licensePlate)
    {
        var ct = CancellationToken.None;
        var command = new CreateVehicleCommand("bmw"!, Model: "m5", 2025, LicensePlate: licensePlate!);

        var result = await _validator.ValidateAsync(command, ct);

        Assert.False(result.IsValid);
        Assert.Equal(nameof(command.LicensePlate), result.Errors[0].PropertyName);
    }

    [Fact]
    public async Task Validate_ShouldSuccess_WhenValidData()
    {
        var ct = CancellationToken.None;
        var command = new CreateVehicleCommand(Make: "bmw", "m5", 2025, "tes|233");

        var result = await _validator.ValidateAsync(command, ct);

        Assert.True(result.IsValid);
    }

    public static TheoryData<string?> GetInvalidMake() => new TheoryData<string?>()
    {
        null,
        string.Empty,
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
    };
    public static TheoryData<string?> GetInvalidModel() => new TheoryData<string?>()
    {
        null,
        string.Empty,
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
    };
    public static TheoryData<string?> GetInvalidLicensePlate() => new TheoryData<string?>()
    {
        null,
        string.Empty,
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(10))
    };

}
