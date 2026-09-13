using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Labors.Mappers;
using MechanicShop.Application.Features.RepairTasks.Mappers;
using MechanicShop.Application.Features.Scheduling.Dtos;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers.Vehicles;
using MechanicShop.Domain.WorkOrders.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MechanicShop.Application.Features.Scheduling.Queries.GetDailySchedule
{
    public sealed class GetDailyScheduleQueryHandler(IAppDbContext context, TimeProvider datetime) : IRequestHandler<GetDailyScheduleQuery, Result<ScheduleDto>>
    {
        private readonly IAppDbContext _context = context;
        private readonly TimeProvider _datetime = datetime;
        public async Task<Result<ScheduleDto>> Handle(GetDailyScheduleQuery query, CancellationToken ct)
        {
            var localStart = query.ScheduleDate.ToDateTime(TimeOnly.MinValue);//start of day 00:00
            var localEnd = localStart.AddDays(1);

            var utcStart = TimeZoneInfo.ConvertTimeToUtc(localStart, query.TimeZone);
            var utcEnd = TimeZoneInfo.ConvertTimeToUtc(localEnd, query.TimeZone);

            var workOrders = await _context.WorkOrders.AsNoTracking()
                .Where(wo => wo.StartAtUtc < utcEnd &&
                wo.EndAtUtc > utcStart &&
                (query.LaborId == null || wo.LaborId == query.LaborId))
                .Include(wo => wo.RepairTasks)
                .Include(wo => wo.Vehicle)
                .Include(wo => wo.Labor)
                .ToListAsync(ct);

            var now = TimeZoneInfo.ConvertTime(_datetime.GetUtcNow(), query.TimeZone);

            var result = new ScheduleDto
            {
                OnDate = query.ScheduleDate,
                EndOfDay = localEnd < now,
                Spots = []
            };

            foreach (var spot in Enum.GetValues<Spot>())
            {
                var current = localStart;
                var slots = new List<AvailabilitySlotDto>();

                var woBySpot = workOrders.Where(wo => wo.Spot == spot).OrderBy(wo => wo.StartAtUtc).ToList();

                while (current < localEnd)//still at that day
                {
                    var next = current.AddMinutes(15);
                    var currentUtc = TimeZoneInfo.ConvertTimeToUtc(current, query.TimeZone);
                    var nextUtc = TimeZoneInfo.ConvertTimeToUtc(next, query.TimeZone);

                    var wo = woBySpot.FirstOrDefault(wo => wo.StartAtUtc < nextUtc && wo.EndAtUtc > currentUtc);

                    if (wo != null)//work order exsits, spot is not available
                    {
                        if (!slots.Any(s => s.WorkOrderId == wo.Id))// to prevent adding same work order (if it more than 15 minute)
                        {
                            slots.Add(new AvailabilitySlotDto()
                            {
                                WorkOrderId = wo.Id,
                                Spot = spot,
                                StartAt = wo.StartAtUtc,
                                EndAt = wo.EndAtUtc,
                                Vehicle = FormatVehicleInfo(wo.Vehicle!),
                                Labor = wo.Labor!.ToDto(),
                                IsOccupied = true,
                                RepairTasks = wo.RepairTasks.ToDtos().ToArray(),
                                WorkOrderLocked = !wo.IsEditable,
                                State = wo.State,
                                IsAvailable = false
                            });
                        }
                    }
                    else//not found 
                    {
                        slots.Add(new AvailabilitySlotDto
                        {
                            Spot = spot,
                            StartAt = currentUtc,
                            EndAt = nextUtc,
                            WorkOrderLocked = false,
                            IsAvailable = current >= now
                        });

                    }

                    current = next;
                }

                result.Spots.Add(new SpotDto
                {
                    Spot = spot,
                    Slots = slots
                });
            }

            return result;
        }

        private static string? FormatVehicleInfo(Vehicle vehicle)
            => vehicle != null ? $"{vehicle.Make} | {vehicle.LicensePlate}" : null;
    }
}
