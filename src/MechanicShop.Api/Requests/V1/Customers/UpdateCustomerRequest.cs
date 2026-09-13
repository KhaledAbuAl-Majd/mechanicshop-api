namespace MechanicShop.Api.Requests.V1.Customers
{
    public sealed record UpdateCustomerRequest(string Name, string PhoneNumber, string Email, List<UpdateVehicleRequest> Vehicles);
}
