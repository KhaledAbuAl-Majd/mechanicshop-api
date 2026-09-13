namespace MechanicShop.Api.IntegrationTests.Common.EndpointsPath;

public static class InvoiceEndpointsPath
{
    public const string Version = "1.0";

    public const string BasePath = $"v{Version}/invoices";
    public static string IssueInvoiceForWorkOrder(Guid id) => $"{BasePath}/work-orders/{id}";
    public static string GetInvoiceById(Guid id) => $"{BasePath}/{id}";
    public static string GetInvoicePdfById(Guid id) => $"{BasePath}/{id}/pdf";
    public static string SettleInvoiceById(Guid id) => $"{BasePath}/{id}/payments";
}