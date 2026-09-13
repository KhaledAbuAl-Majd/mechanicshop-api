namespace MechanicShop.Api.Requests.V1.WorkOrders
{
    public sealed record UpdateWorkOrderRepairTasksRequest(
      Guid[] RepairTaskIds);

}
