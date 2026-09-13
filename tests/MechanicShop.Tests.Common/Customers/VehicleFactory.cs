using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers.Vehicles;

namespace MechanicShop.Tests.Common.Customers;

public static class VehicleFactory
{
    private const string DefaultPlateSentinel = "__DEFAULT_UNIQUE_PLATE__";
    public static Result<Vehicle> CreateVehicle(
        Guid? id = null,
        string? make = "BMW",
        string? model = "M5",
        int? year = null,
        string? licensePlate = DefaultPlateSentinel)
    {
        var finalPlate = licensePlate == DefaultPlateSentinel
         ? Guid.NewGuid().ToString("N")[..10]
         : licensePlate;

        return Vehicle.Create(
            id ?? Guid.NewGuid(),
            make!,
            model!,
            year ?? 2025,
            finalPlate!);
    }
}
