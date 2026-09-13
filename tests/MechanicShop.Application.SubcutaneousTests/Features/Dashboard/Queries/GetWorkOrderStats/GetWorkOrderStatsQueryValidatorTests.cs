using MechanicShop.Application.Features.Dashboard.Queries.GetWorkOrderStats;

namespace MechanicShop.Application.SubcutaneousTests.Features.Dashboard.Queries.GetWorkOrderStats;

public class GetWorkOrderStatsQueryValidatorTests
{
    private readonly GetWorkOrderStatsQueryValidator _validator = new();

    [Fact]
    public async Task Validate_ShouldSuccess_WhenValidData()
    {
        CancellationToken ct = CancellationToken.None;
        var query = new GetWorkOrderStatsQuery(TimeZoneInfo.Utc, DateOnly.FromDateTime(DateTime.Today));

        var result = await _validator.ValidateAsync(query, ct);

        Assert.True(result.IsValid);
    }
}
