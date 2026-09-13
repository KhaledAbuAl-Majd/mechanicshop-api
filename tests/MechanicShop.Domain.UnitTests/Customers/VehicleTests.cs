using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers.Vehicles;
using MechanicShop.Tests.Common.Customers;

namespace MechanicShop.Domain.UnitTests.Customers;

public class VehicleTests
{
    [Fact]
    public void Create_ShouldReturnError_WhenIdEmpty()
    {
        var result = VehicleFactory.CreateVehicle(id: Guid.Empty);

        Assert.False(result.IsSuccess);
        Assert.Equal(VehicleErrors.IdRequired.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldReturnError_WhenMakeInvalid(string? make)
    {
        var result = VehicleFactory.CreateVehicle(make: make);

        Assert.False(result.IsSuccess);
        Assert.Equal(VehicleErrors.MakeRequired.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldReturnError_WhenModelInvalid(string? model)
    {
        var result = VehicleFactory.CreateVehicle(model: model);

        Assert.False(result.IsSuccess);
        Assert.Equal(VehicleErrors.ModelRequired.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldReturnError_WhenLicensePlateInvalid(string? licensePlate)
    {
        var result = VehicleFactory.CreateVehicle(licensePlate: licensePlate);

        Assert.False(result.IsSuccess);
        Assert.Equal(VehicleErrors.LicensePlateRequired.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData(1885)]
    [InlineData(2300)]
    public void Create_ShouldReturnError_WhenYearInvalid(int year)
    {
        var result = VehicleFactory.CreateVehicle(year: year);

        Assert.False(result.IsSuccess);
        Assert.Equal(VehicleErrors.YearInvalid.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WhenValidData()
    {
        var id = Guid.NewGuid();
        string make = "tesla";
        string model = "db3";
        int year = DateTimeOffset.UtcNow.Year - 1;
        string licensePlate = "dba 253";

        var result = VehicleFactory.CreateVehicle(id: id, make: make, model: model, year: year, licensePlate: licensePlate);

        Assert.True(result.IsSuccess);
        var vehicle = result.Value;
        Assert.NotNull(vehicle);
        Assert.Equal(id, vehicle.Id);
        Assert.Equal(make, vehicle.Make);
        Assert.Equal(model, vehicle.Model);
        Assert.Equal(year, vehicle.Year);
        Assert.Equal(licensePlate, vehicle.LicensePlate);
    }


    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_ShouldReturnError_WhenMakeInvalid(string? make)
    {
        var vehicle = VehicleFactory.CreateVehicle().Value;

        string model = "db3";
        int year = DateTimeOffset.UtcNow.Year - 1;
        string licensePlate = "dba 253";

        var result = vehicle.Update(make!, model, year, licensePlate);

        Assert.False(result.IsSuccess);
        Assert.Equal(VehicleErrors.MakeRequired.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_ShouldReturnError_WhenModelInvalid(string? model)
    {
        var vehicle = VehicleFactory.CreateVehicle().Value;

        string make = "tesla";
        int year = DateTimeOffset.UtcNow.Year - 1;
        string licensePlate = "dba 253";

        var result = vehicle.Update(make, model!, year, licensePlate);

        Assert.False(result.IsSuccess);
        Assert.Equal(VehicleErrors.ModelRequired.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_ShouldReturnError_WhenLicensePlateInvalid(string? licensePlate)
    {
        var vehicle = VehicleFactory.CreateVehicle().Value;

        string make = "tesla";
        string model = "db3";
        int year = DateTimeOffset.UtcNow.Year - 1;

        var result = vehicle.Update(make, model, year, licensePlate!);

        Assert.False(result.IsSuccess);
        Assert.Equal(VehicleErrors.LicensePlateRequired.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData(1885)]
    [InlineData(2300)]
    public void Update_ShouldReturnError_WhenYearInvalid(int year)
    {
        var vehicle = VehicleFactory.CreateVehicle().Value;

        string make = "tesla";
        string model = "db3";
        string licensePlate = "dba 253";

        var result = vehicle.Update(make, model, year, licensePlate);

        Assert.False(result.IsSuccess);
        Assert.Equal(VehicleErrors.YearInvalid.Code, result.TopError.Code);
    }

    [Fact]
    public void Update_ShouldReturnSuccess_WhenValidData()
    {
        string make = "tesla";
        string model = "db3";
        int year = DateTimeOffset.UtcNow.Year - 1;
        string licensePlate = "dba 253";

        var vehicle = VehicleFactory.CreateVehicle().Value;

        var result = vehicle.Update(make, model, year, licensePlate);

        Assert.True(result.IsSuccess);
        Assert.Equal(Result.Updated, result.Value);
        Assert.Equal(make, vehicle.Make);
        Assert.Equal(model, vehicle.Model);
        Assert.Equal(year, vehicle.Year);
        Assert.Equal(licensePlate, vehicle.LicensePlate);
    }

    [Fact]
    public void VehicleInfo_ShouldReturnFormattedString()
    {
        var vehicle = VehicleFactory.CreateVehicle(make: "Ford", model: "Mustang", year: 2021).Value;

        Assert.Equal("Ford | Mustang | 2021", vehicle.VehicleInfo);
    }
}
