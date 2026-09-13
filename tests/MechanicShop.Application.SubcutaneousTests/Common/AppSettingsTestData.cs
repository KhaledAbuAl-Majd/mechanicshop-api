namespace MechanicShop.Application.SubcutaneousTests.Common;

public static class AppSettingsTestData
{
    public static readonly TimeOnly DefaultOpeningTime = new(9, 0);
    public static readonly TimeOnly DefaultClosingTime = new(18, 0);
    public const int MinimumAppointmentDurationInMinutes = 30;
}
