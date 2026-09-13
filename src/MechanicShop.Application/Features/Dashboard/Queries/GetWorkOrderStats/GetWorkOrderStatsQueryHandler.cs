using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Dashboard.Dtos;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MechanicShop.Application.Features.Dashboard.Queries.GetWorkOrderStats
{
    public sealed class GetWorkOrderStatsQueryHandler(IAppDbContext context) :
        IRequestHandler<GetWorkOrderStatsQuery, Result<TodayWorkOrderStatsDto>>
    {
        private readonly IAppDbContext _context = context;

        public async Task<Result<TodayWorkOrderStatsDto>> Handle(GetWorkOrderStatsQuery request, CancellationToken ct)
        {
            var localStart = request.Date.ToDateTime(TimeOnly.MinValue);
            var localEnd = localStart.AddDays(1);

            var utcStart = TimeZoneInfo.ConvertTimeToUtc(localStart, request.TimeZone);
            var utcEnd = TimeZoneInfo.ConvertTimeToUtc(localEnd, request.TimeZone);

            var workOrders = await _context.WorkOrders.AsNoTracking()
               .Include(wo => wo.Vehicle)
               .Include(wo => wo.RepairTasks).ThenInclude(rt => rt.Parts)
               .Include(wo => wo.Invoice)
               .Where(wo => wo.StartAtUtc >= utcStart && wo.StartAtUtc < utcEnd)
               .ToListAsync(ct);

            var total = workOrders.Count;

            if (total == 0)
            {
                return new TodayWorkOrderStatsDto
                {
                    Date = request.Date,
                    Total = 0,
                    Scheduled = 0,
                    InProgress = 0,
                    Completed = 0,
                    Cancelled = 0,
                    TotalRevenue = 0,
                    TotalPartsCost = 0,
                    TotalLaborCost = 0,
                    UniqueVehicles = 0,
                    UniqueCustomers = 0
                };
            }

            var totalRevenue = workOrders.Sum(wo => wo.Invoice?.Total ?? 0);
            var totalPartsCost = workOrders.Where(wo => wo.Invoice != null).Sum(wo => wo.TotalPartsCost ?? 0);//must invoice be issue
            var totalLaborCost = workOrders.Where(wo => wo.Invoice != null).Sum(wo => wo.TotalLaborCost ?? 0);
            var uniqueVehicles = workOrders.Select(wo => wo.VehicleId).Distinct().Count();
            var uniqueCustomers = workOrders.Select(wo => wo.Vehicle!.CustomerId).Distinct().Count();

            var netProfit = totalRevenue - totalPartsCost - totalLaborCost;

            return new TodayWorkOrderStatsDto
            {
                Date = request.Date,
                Total = total,
                Scheduled = workOrders.Count(wo => wo.State == WorkOrderState.Scheduled),
                InProgress = workOrders.Count(wo => wo.State == WorkOrderState.InProgress),
                Completed = workOrders.Count(wo => wo.State == WorkOrderState.Completed),
                Cancelled = workOrders.Count(wo => wo.State == WorkOrderState.Cancelled),
                TotalRevenue = totalRevenue,
                TotalPartsCost = totalPartsCost,
                TotalLaborCost = totalLaborCost,
                UniqueVehicles = uniqueVehicles,
                UniqueCustomers = uniqueCustomers,
                NetProfit = netProfit,
                ProfitMargin = totalRevenue > 0 ? (netProfit / totalRevenue) * 100 : 0,
                CompletionRate = total > 0 ? ((decimal)workOrders.Count(wo => wo.State == WorkOrderState.Completed) / total) * 100 : 0,
                AverageRevenuePerOrder = total > 0 ? totalRevenue / total : 0,
                OrdersPerVehicle = uniqueVehicles > 0 ? (decimal)total / uniqueVehicles : 0,
                PartsCostRatio = totalRevenue > 0 ? ((decimal)totalPartsCost / totalRevenue) * 100 : 0,
                LaborCostRatio = totalRevenue > 0 ? ((decimal)totalLaborCost / totalRevenue) * 100 : 0,
                CancellationRate = total > 0 ? ((decimal)workOrders.Count(wo => wo.State == WorkOrderState.Cancelled) / total) * 100 : 0
            };
        }
    }
}
