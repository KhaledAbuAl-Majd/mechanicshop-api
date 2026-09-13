namespace MechanicShop.Api.Requests.V1
{
    public sealed record PageRequest(int Page = 1, int PageSize = 10);
}
