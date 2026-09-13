namespace MechanicShop.Application.Common.Interfaces
{
    //not optimal way
    public interface INotificationService
    {
        Task SendEmailAsync(string to, CancellationToken cancellationToken = default);

        Task SendSmsAsync(string phoneNumber, CancellationToken cancellationToken = default);
    }
}
