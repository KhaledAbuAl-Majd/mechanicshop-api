using MechanicShop.Application.Features.RepairTasks.Dtos;
using MechanicShop.Domain.Common;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.RepairTasks.Parts;

namespace MechanicShop.Application.Features.RepairTasks.Mappers
{
    public static class RepairTaskMapper
    {
        public static RepairTaskDto ToDto(this RepairTask entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            return new RepairTaskDto(
                entity.Id,
                entity.Name,
                entity.EstimatedDurationInMins,
                entity.LaborCost,
                entity.TotalCost,
                entity.Parts.ToDtos());
        }
        public static List<RepairTaskDto> ToDtos(this IEnumerable<RepairTask> entities)
        {
            ArgumentNullException.ThrowIfNull(entities);

            return [.. entities.Select(ToDto)];
        }

        public static PartDto ToDto(this Part entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            return new PartDto(entity.Id, entity.Name!, entity.Cost, entity.Quantity);
        }
        public static List<PartDto> ToDtos(this IEnumerable<Part> entities)
        {
            ArgumentNullException.ThrowIfNull(entities);

            return [.. entities.Select(ToDto)];
        }
    }
}
