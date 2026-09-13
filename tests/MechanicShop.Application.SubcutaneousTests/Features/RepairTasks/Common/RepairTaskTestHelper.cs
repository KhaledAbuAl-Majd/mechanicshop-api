using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Tests.Common.RepairTasks;

namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Common;

public static class RepairTaskTestHelper
{
    public static async Task<RepairTask> CreateValidRepairTask(IAppDbContext context, CancellationToken ct = default)
    {
        var partName = $"OilFilter-{Guid.NewGuid().ToString()[..8]}";
        var part = PartFactory.CreatePart(name: partName).Value;

        var taskName = $"OilChange-{Guid.NewGuid().ToString()[..8]}";
        var reparirTaks = RepairTaskFactory.CreateRepairTask(name: taskName, parts: [part]).Value;

        context.RepairTasks.Add(reparirTaks);
        await context.SaveChangesAsync(ct);

        return reparirTaks;
    }
}
