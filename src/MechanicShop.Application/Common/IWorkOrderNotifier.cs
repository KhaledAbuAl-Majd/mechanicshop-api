namespace MechanicShop.Application.Common
{
    public interface IWorkOrderNotifier
    {
        Task NotifyWorkOrdersChangedAsync(CancellationToken ct = default);
    }
}
