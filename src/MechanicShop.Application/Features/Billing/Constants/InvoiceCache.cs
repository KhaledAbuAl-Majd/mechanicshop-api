namespace MechanicShop.Application.Features.Billing.Constants
{
    public static class InvoiceCache
    {
        private const string BaseName = "invoices";

        public const string Tag = BaseName;

        public const string AllKey = $"{BaseName}:all";
        public static string ByIdKey(Guid id) => ByIdKey(id.ToString());
        public static string ByIdKey(string id) => $"{BaseName}:{id}";
        public static string PdfByIdKey(Guid id) => PdfByIdKey(id.ToString());
        public static string PdfByIdKey(string id) => $"{BaseName}:pdf:{id}";
    }
}
