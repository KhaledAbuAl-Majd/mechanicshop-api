using System.Security.Cryptography;
using System.Text;

namespace MechanicShop.Application.Common.Utilities
{
    public static class HashHelper
    {
        public static string ComputeSha256(string input)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(input);

            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = SHA256.HashData(inputBytes);

            return Convert.ToHexStringLower(hashBytes);
        }
    }
}
