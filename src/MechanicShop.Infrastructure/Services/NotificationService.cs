using MechanicShop.Application.Common.Extensions;
using MechanicShop.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Infrastructure.Services
{
    //not optimal way - mocking it
    public sealed class NotificationService(ILogger<NotificationService> logger) : INotificationService
    {
        private const string Message = "Your vehicle service is complete. You may collect it from the shop at your earliest convenience.";

        public async Task SendEmailAsync(string to, CancellationToken cancellationToken = default)
        {
            var maskedEmail = to.MaskEmail();

            logger.LogInformation("[Email] To: {Email} | Message: {Message}", maskedEmail, Message);

            // Simulated email send
            await Task.CompletedTask;
        }

        public async Task SendSmsAsync(string phoneNumber, CancellationToken cancellationToken = default)
        {
            var masked = phoneNumber.MaskPhoneNumber();

            logger.LogInformation("[SMS] To: {Phone} | Message: {Message}", masked, Message);

            // Simulated SMS send
            await Task.CompletedTask;
        }
    }
}
