namespace MechanicShop.Api.Requests.V1.Customers
{
    public sealed record CreateVehicleRequest(string Make, string Model, int Year, string LicensePlate);
}
