namespace MechanicShop.Application.Features.Customers.Dtos
{
    public sealed record CustomerListItemDto(
     Guid CustomerId,
     string Name,
     string PhoneNumber,
     string Email,
     int VehiclesCount
 );
}
