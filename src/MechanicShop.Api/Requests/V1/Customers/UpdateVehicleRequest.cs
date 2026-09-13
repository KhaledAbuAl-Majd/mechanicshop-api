namespace MechanicShop.Api.Requests.V1.Customers
{
    public sealed record UpdateVehicleRequest(Guid? VehicleId, string Make, string Model, int Year, string LicensePlate);
}
