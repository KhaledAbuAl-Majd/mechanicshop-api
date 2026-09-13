namespace MechanicShop.Application.Features.RepairTasks.Dtos
{
    public record PartDto(Guid PartId, string Name, decimal Cost, int Quantity);
}
