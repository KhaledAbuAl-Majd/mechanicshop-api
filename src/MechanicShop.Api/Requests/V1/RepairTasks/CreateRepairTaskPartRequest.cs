namespace MechanicShop.Api.Requests.V1.RepairTasks
{
    public sealed record CreateRepairTaskPartRequest(string Name, decimal Cost, int Quantity);

}
