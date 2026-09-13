using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Common.Settings;
using MechanicShop.Application.Features.WorkOrders.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Enums;
using Microsoft.EntityFrameworkCore;

namespace MechanicShop.Application.Features.WorkOrders.Services
{
    public class WorkOrderPolicy(AppSettings appSettings, IAppDbContext context) : IWorkOrderPolicy
    {
        private readonly AppSettings _appSettings = appSettings;
        private readonly IAppDbContext _context = context;


        public async Task<Result<Success>> CheckSpotAvailabilityAsync(
            Spot spot,
            DateTimeOffset startAt,
            DateTimeOffset endAt,
            Guid? excludedWorkOrderId = null,
            CancellationToken ct = default)
        {
            var isOccupied = await _context.WorkOrders.AnyAsync(
                wo =>
                wo.Spot == spot &&
                wo.StartAtUtc < endAt &&
                wo.EndAtUtc > startAt &&
                (excludedWorkOrderId == null || wo.Id != excludedWorkOrderId.Value),
                ct);

            return isOccupied ?
                Error.Conflict("MechanicShop.Spot.Full", "The selected time slot is unavailable for the requested services.") :
                Result.Success;
        }

        public async Task<Result<Success>> IsLaborOccupied(
            Guid laborId,
            Guid? excludedWorkOrderId,
            DateTimeOffset startAt,
            DateTimeOffset endAt,
            CancellationToken ct = default)
        {
            var isOccupied = await _context.WorkOrders.AnyAsync(
                wo =>
                wo.LaborId == laborId &&
                wo.StartAtUtc < endAt &&
                wo.EndAtUtc > startAt &&
                 (excludedWorkOrderId == null || wo.Id != excludedWorkOrderId.Value),
                ct);

            return isOccupied ?
                Error.Conflict(
                   code: "Labor.Occupied",
                   description: "Labor is already occupied during the requested time.")
                : Result.Success;
        }

        public bool IsOutsideOperatingHours(DateTimeOffset startAt, TimeSpan duration)
        {
            var endAt = startAt.Add(duration);

            var opening = TimeOnly.FromDateTime(startAt.DateTime);
            var closing = TimeOnly.FromDateTime(endAt.DateTime);

            return startAt.Date != endAt.Date
                || opening < _appSettings.OpeningTime
                || closing > _appSettings.ClosingTime;
        }

        public async Task<Result<Success>> IsVehicleAlreadyScheduled(
            Guid vehicleId,
            DateTimeOffset startAt,
            DateTimeOffset endAt,
            Guid? excludedWorkOrderId = null,
            CancellationToken ct = default)
        {
            var hasConflict = await _context.WorkOrders.AnyAsync(wo =>
                wo.VehicleId == vehicleId &&
                wo.StartAtUtc < endAt &&
                wo.EndAtUtc > startAt &&
                (excludedWorkOrderId == null || wo.Id != excludedWorkOrderId.Value)
              , ct);

            return hasConflict ?
                Error.Conflict(
                    code: "Vehicle.Overlapping.WorkOrders",
                    description: "The vehicle already has an overlapping WorkOrder.")
                : Result.Success;
        }

        public Result<Success> ValidateMinimumRequirement(DateTimeOffset startAt, DateTimeOffset endAt)
        {
            if ((endAt - startAt) < TimeSpan.FromMinutes(_appSettings.MinimumAppointmentDurationInMinutes))
            {
                return Error.Conflict(
                    "WorkOrder.TooShort",
                    $"WorkOrder duration must be at least {_appSettings.MinimumAppointmentDurationInMinutes} minutes.");
            }

            return Result.Success;
        }
    }
}
