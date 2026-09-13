using MechanicShop.Domain.Employees;
using MechanicShop.Domain.Identity.Enums;
using MechanicShop.Tests.Common.Employees;

namespace MechanicShop.Domain.UnitTests.Employees;

public class EmployeeTests
{
    [Fact]
    public void Create_ShouldReturnError_WhenIdEmpty()
    {
        var result = EmployeeFactory.CreateEmployee(id: Guid.Empty);

        Assert.False(result.IsSuccess);
        Assert.Equal(EmployeeErrors.IdRequired.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("    ")]
    public void Create_ShouldReturnError_WhenFirstNameEmpty(string? firstName)
    {
        var result = EmployeeFactory.CreateEmployee(firstName: firstName);

        Assert.False(result.IsSuccess);
        Assert.Equal(EmployeeErrors.FirstNameRequired.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("    ")]
    public void Create_ShouldReturnError_WhenLastNameEmpty(string? lastName)
    {
        var result = EmployeeFactory.CreateEmployee(lastName: lastName);

        Assert.False(result.IsSuccess);
        Assert.Equal(EmployeeErrors.LastNameRequired.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnError_WhenRoleInvalid()
    {
        var result = EmployeeFactory.CreateEmployee(role: (Role)9999);

        Assert.False(result.IsSuccess);
        Assert.Equal(EmployeeErrors.RoleInvalid.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WhenValidData()
    {
        var id = Guid.NewGuid();
        string firstName = "khaled".Trim();
        string lastName = "Abu Al-Majd".Trim();
        Role role = Role.Manager;

        string fullName = $"{firstName} {lastName}";

        var result = EmployeeFactory.CreateEmployee(
            id: id,
            firstName: firstName,
            lastName: lastName,
            role: role);



        Assert.True(result.IsSuccess);
        var employee = result.Value;
        Assert.NotNull(employee);
        Assert.Equal(id, employee.Id);
        Assert.Equal(firstName, employee.FirstName);
        Assert.Equal(lastName, employee.LastName);
        Assert.Equal(role, employee.Role);
        Assert.Equal(fullName, employee.FullName);
    }
}
