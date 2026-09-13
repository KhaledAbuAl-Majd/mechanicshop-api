namespace MechanicShop.Api.Requests.V1.RepairTasks
{
    public sealed record UpdateRepairTaskPartRequest(Guid? PartId, string Name, decimal Cost, int Quantity);

}
