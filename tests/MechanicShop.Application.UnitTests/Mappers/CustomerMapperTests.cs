using MechanicShop.Application.Features.Customers.Mappers;
using MechanicShop.Domain.Customers;
using MechanicShop.Tests.Common.Customers;

namespace MechanicShop.Application.UnitTests.Mappers;

public class CustomerMapperTests
{

    [Fact]
    public void ToDto_ShouldMapCorrectly()
    {
        var customer = CustomerFactory.CreateCustomer().Value;

        var dto = customer.ToDto();

        Assert.NotNull(dto);
        Assert.Equal(customer.Id, dto.CustomerId);
        Assert.Equal(customer.Name, dto.Name);
        Assert.Equal(customer.Email, dto.Email);
        Assert.Equal(customer.PhoneNumber, dto.PhoneNumber);
        Assert.Equal(customer.Vehicles.Count(), dto.Vehicles.Count);
    }

    [Fact]
    public void ToDto_ShouldThrowException_WhenCustomerIsNull()
    {
        var customer = (Customer)null!;

        Assert.Throws<ArgumentNullException>(customer.ToDto);
    }

    [Fact]
    public void ToDto_ShouldThrowException_WhenVehicleIsNull()
    {
        var customer = CustomerFactory.CreateCustomer(vehicles: null,setListIfNull:false).Value;

        Assert.Throws<ArgumentNullException>(customer.ToDto);
    }

    [Fact]
    public void ToDtos_ShouldMapListCorrectly()
    {
        var customer = CustomerFactory.CreateCustomer().Value;
        List<Customer> customers = [customer];

        var dtos = customers.ToDtos();
        Assert.NotNull(dtos);
        Assert.Single(dtos);

        var dto = dtos[0];
        Assert.Equal(customer.Id, dto.CustomerId);
        Assert.Equal(customer.Name, dto.Name);
        Assert.Equal(customer.Email, dto.Email);
        Assert.Equal(customer.PhoneNumber, dto.PhoneNumber);
        Assert.Equal(customer.Vehicles.Count(), dto.Vehicles.Count);
    }

}
