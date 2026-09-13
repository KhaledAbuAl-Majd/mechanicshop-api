namespace MechanicShop.Api.Requests.V1.Customers
{
    public sealed record CreateCustomerRequest(string Name, string PhoneNumber, string Email, List<CreateVehicleRequest> Vehicles);
}
