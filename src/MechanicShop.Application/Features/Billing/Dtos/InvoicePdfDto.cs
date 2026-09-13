namespace MechanicShop.Application.Features.Billing.Dtos
{
    public sealed record InvoicePdfDto
    {
        public byte[]? Content { get; init; }
        public string? FileName { get; init; }
        public string? ContentType { get; init; } = "application/pdf";
    }
}
