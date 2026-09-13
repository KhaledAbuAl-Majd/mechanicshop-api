using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Enums;

namespace MechanicShop.Application.Features.WorkOrders.Interfaces
{
    public interface IWorkOrderPolicy
    {
        bool IsOutsideOperatingHours(DateTimeOffset startAt, TimeSpan duration);
        Task<Result<Success>> IsLaborOccupied(Guid laborId, Guid? excludedWorkOrderId, DateTimeOffset startAt, DateTimeOffset endAt,
            CancellationToken ct = default);
        Task<Result<Success>> IsVehicleAlreadyScheduled(Guid vehicleId, DateTimeOffset startAt, DateTimeOffset endAt, Guid? excludedWorkOrderId = null,
            CancellationToken ct = default);
        Task<Result<Success>> CheckSpotAvailabilityAsync(Spot spot, DateTimeOffset startAt, DateTimeOffset endAt, Guid? excludeWorkOrderId = null, CancellationToken ct = default);
        Result<Success> ValidateMinimumRequirement(DateTimeOffset startAt, DateTimeOffset endAt);
    }
}
