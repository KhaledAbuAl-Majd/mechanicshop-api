using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers;
using MechanicShop.Domain.Customers.Vehicles;
using MechanicShop.Tests.Common.Customers;

namespace MechanicShop.Domain.UnitTests.Customers;

public class CustomerTests
{
    [Fact]
    public void Create_ShouldReturnError_WhenIdEmpty()
    {
        var result = CustomerFactory.CreateCustomer(id: Guid.Empty);

        Assert.False(result.IsSuccess);
        Assert.Equal(CustomerErrors.IdRequired.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldReturnError_WhenNameInvalid(string? name)
    {
        var result = CustomerFactory.CreateCustomer(name: name);

        Assert.False(result.IsSuccess);
        Assert.Equal(CustomerErrors.NameRequired.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldReturnError_WhenPhoneEmptyOrNull(string? phone)
    {
        var result = CustomerFactory.CreateCustomer(phoneNumber: phone);

        Assert.False(result.IsSuccess);
        Assert.Equal(CustomerErrors.PhoneNumberRequired.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData("454")]
    [InlineData("1353453535353535353")]
    [InlineData("++35353535353")]
    public void Create_ShouldReturnError_WhenPhoneInvalid(string phone)
    {
        var result = CustomerFactory.CreateCustomer(phoneNumber: phone);

        Assert.False(result.IsSuccess);
        Assert.Equal(CustomerErrors.InvalidPhoneNumber.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldReturnError_WhenEmailEmptyOrNull(string? email)
    {
        var result = CustomerFactory.CreateCustomer(email: email);

        Assert.False(result.IsSuccess);
        Assert.Equal(CustomerErrors.EmailRequired.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData("plainaddress")]
    [InlineData("@missingusername.com")]
    [InlineData("username@")]
    [InlineData("user@.com")]
    [InlineData("user name@example.com")]
    [InlineData("user@example..com")]
    [InlineData("user@@example.com")]
    public void Create_ShouldReturnError_WhenEmailInvalid(string email)
    {
        var result = CustomerFactory.CreateCustomer(email: email);

        Assert.False(result.IsSuccess);
        Assert.Equal(CustomerErrors.EmailInvalid.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WhenValidData()
    {
        var id = Guid.NewGuid();
        const string name = "Customer #1";
        const string phoneNumber = "01012345678";
        const string email = "customer01@example.com";
        List<Vehicle> vehicles = [VehicleFactory.CreateVehicle().Value];

        var result = CustomerFactory.CreateCustomer(id: id, name: name, phoneNumber: phoneNumber, email: email, vehicles: vehicles);

        Assert.True(result.IsSuccess);

        var customer = result.Value;

        Assert.IsType<Customer>(customer);
        Assert.NotNull(customer);
        Assert.Equal(id, customer.Id);
        Assert.Equal(name, customer.Name);
        Assert.Equal(phoneNumber, customer.PhoneNumber);
        Assert.Equal(email, customer.Email);
        Assert.Single(customer.Vehicles);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_ShouldReturnError_WhenNameInvalid(string? name)
    {
        var customer = CustomerFactory.CreateCustomer().Value;

        var email = "user@example.com";
        var phone = "+201012345678";

        var result = customer.Update(name!, email, phone);

        Assert.False(result.IsSuccess);
        Assert.Equal(CustomerErrors.NameRequired.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_ShouldReturnError_WhenPhoneEmptyOrNull(string? phone)
    {
        var customer = CustomerFactory.CreateCustomer().Value;

        var name = "ahmed";
        var email = "user@example.com";

        var result = customer.Update(name, email, phone!);

        Assert.False(result.IsSuccess);
        Assert.Equal(CustomerErrors.PhoneNumberRequired.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData("454")]
    [InlineData("1353453535353535353")]
    [InlineData("++35353535353")]
    public void Update_ShouldReturnError_WhenPhoneInvalid(string phone)
    {
        var customer = CustomerFactory.CreateCustomer().Value;

        var name = "ahmed";
        var email = "user@example.com";

        var result = customer.Update(name, email, phone!);

        Assert.False(result.IsSuccess);
        Assert.Equal(CustomerErrors.InvalidPhoneNumber.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_ShouldReturnError_WhenEmailEmptyOrNull(string? email)
    {
        var customer = CustomerFactory.CreateCustomer().Value;

        var name = "ahmed";
        var phone = "+239384387";

        var result = customer.Update(name, email!, phone!);

        Assert.False(result.IsSuccess);
        Assert.Equal(CustomerErrors.EmailRequired.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData("plainaddress")]
    [InlineData("@missingusername.com")]
    [InlineData("username@")]
    [InlineData("user@.com")]
    [InlineData("user name@example.com")]
    [InlineData("user@example..com")]
    [InlineData("user@@example.com")]
    public void Update_ShouldReturnError_WhenEmailInvalid(string email)
    {
        var customer = CustomerFactory.CreateCustomer().Value;

        var name = "ahmed";
        var phone = "+239384387";

        var result = customer.Update(name, email!, phone!);

        Assert.False(result.IsSuccess);
        Assert.Equal(CustomerErrors.EmailInvalid.Code, result.TopError.Code);
    }

    [Fact]
    public void Update_ShouldReturnSuccess_AndAssignData()
    {
        var customer = CustomerFactory.CreateCustomer().Value;

        var name = "ahmed";
        var phone = "+239384387";
        var email = "user@example.com";

        var result = customer.Update(name, email, phone);

        Assert.True(result.IsSuccess);
        Assert.Equal(Result.Updated, result.Value);
        Assert.Equal(name, customer.Name);
        Assert.Equal(phone, customer.PhoneNumber);
        Assert.Equal(email, customer.Email);
    }

    [Fact]
    public void UpsertVehicles_ShouldReturnSuccess_WhenRemovingVehicleNotAtIncominList()
    {
        var v1 = VehicleFactory.CreateVehicle().Value;
        var v2 = VehicleFactory.CreateVehicle().Value;

        var customer = CustomerFactory.CreateCustomer(vehicles: [v1, v2]).Value;

        var incominVehicle = VehicleFactory.CreateVehicle(id: v1.Id).Value;

        var result = customer.UpsertVehicles([incominVehicle]);

        Assert.True(result.IsSuccess);
        Assert.Equal(Result.Updated, result.Value);
        Assert.Single(customer.Vehicles);
        Assert.Equal(v1.Id, customer.Vehicles.Single().Id);
    }

    [Fact]
    public void UpsertVehicles_ShouldReturnSuccess_WhenAddNewVehiclesAndUpdateExisting()
    {
        var original = VehicleFactory.CreateVehicle(make: "Ford").Value;

        var oldMake = original.Make;

        var customer = CustomerFactory.CreateCustomer(vehicles: [original]).Value;

        var updatedVehicle = VehicleFactory.CreateVehicle(id: original.Id, make: "BMW").Value;

        var newVehicle = VehicleFactory.CreateVehicle().Value;

        var result = customer.UpsertVehicles([updatedVehicle, newVehicle]);

        Assert.True(result.IsSuccess);
        Assert.Equal(Result.Updated, result.Value);
        Assert.Equal(2, customer.Vehicles.Count());
        Assert.Contains(customer.Vehicles, v => v.Id == updatedVehicle.Id && v.Make != oldMake);
        Assert.Contains(customer.Vehicles, v => v.Id == newVehicle.Id);
    }
}
