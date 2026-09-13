namespace MechanicShop.Application.Common.Extensions
{
    public static class StringExtensions
    {
        public static string MaskEmail(this string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return string.Empty;

            int atIndex = email.IndexOf('@');

            if (atIndex <= 1)
            {
                return atIndex == -1
                    ? "****"
                    : $"****{email[atIndex..]}";
            }

            return $"{email[0]}****{email[atIndex - 1]}{email[atIndex..]}";
        }

        public static string MaskPhoneNumber(this string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return string.Empty;

            return phoneNumber.Length >= 4
                ? $"{new string('*', phoneNumber.Length - 4)}{phoneNumber[^4..]}"
                : "****";
        }
    }
}
