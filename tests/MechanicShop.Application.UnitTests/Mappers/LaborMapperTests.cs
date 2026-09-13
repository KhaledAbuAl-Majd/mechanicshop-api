using MechanicShop.Application.Features.Labors.Mappers;
using MechanicShop.Domain.Employees;
using MechanicShop.Tests.Common.Employees;

namespace MechanicShop.Application.UnitTests.Mappers;

public class LaborMapperTests
{
    [Fact]
    public void ToDto_ShouldMapCorrectly()
    {
        var labor = EmployeeFactory.CreateLabor().Value;

        var dto = labor.ToDto();

        Assert.NotNull(dto);
        Assert.Equal(labor.Id, dto.LaborId);
        Assert.Equal(labor.FullName, dto.Name);
    }

    [Fact]
    public void ToDto_ShouldThrowException_WhenLaborIsNull()
    {
        var labor = (Employee)null!;

        Assert.Throws<ArgumentNullException>(labor.ToDto);
    }

    [Fact]
    public void ToDtos_ShouldMapListCorrectly()
    {
        var labor = EmployeeFactory.CreateLabor().Value;
        List<Employee> labors = [labor];

        var dtos = labors.ToDtos();
        Assert.NotNull(dtos);
        Assert.Single(dtos);

        var dto = dtos[0];
        Assert.NotNull(dto);
        Assert.Equal(labor.Id, dto.LaborId);
        Assert.Equal(labor.FullName, dto.Name);
    }
}
