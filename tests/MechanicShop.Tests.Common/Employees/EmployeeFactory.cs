using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Employees;
using MechanicShop.Domain.Identity.Enums;

namespace MechanicShop.Tests.Common.Employees;

public static class EmployeeFactory
{
    public static Result<Employee> CreateEmployee(
        Guid? id = null,
        string? firstName = "ahmed",
        string? lastName = "ali",
        Role? role = null)
    {
        return Employee.Create(
            id ?? Guid.NewGuid(),
            firstName!,
            lastName!,
            role ?? Role.Labor);
    }

    public static Result<Employee> CreateLabor(
       Guid? id = null,
       string? firstName = "ahmed",
        string? lastName = "labor"
      )
    {
        return CreateEmployee(id, firstName, lastName, Role.Labor);
    }

    public static Result<Employee> CreateManager(
       Guid? id = null,
       string? firstName = "ahmed",
        string? lastName = "manager")
    {
        return CreateEmployee(id, firstName, lastName, Role.Manager);
    }
}
