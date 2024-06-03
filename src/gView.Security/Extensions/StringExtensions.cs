using System.Security.Cryptography;
using System.Text;

namespace gView.Security.Extensions;
static public class StringExtensions
{
    public static string ToSha256Hash(this string rawData)
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            return sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData)).ToHexString();
        }
    }

    public static string ToSha512Hash(this string rawData)
    {
        using (SHA512 sha256Hash = SHA512.Create())
        {
            return sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData)).ToHexString();
        }
    }

    public static string ToHexString(this byte[] bytes)
    {
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < bytes.Length; i++)
        {
            builder.Append(bytes[i].ToString("x2"));
        }
        return builder.ToString();
    }

    public static bool ValidateHash(this string rawData, string hash)
    {
        var rawDataHash = hash.Length switch
        {
            64 => rawData.ToSha256Hash(),
            128 => rawData.ToSha512Hash(),
            _ => throw new InvalidOperationException("Invalid hash length")
        };

        return rawDataHash.Equals(hash, StringComparison.OrdinalIgnoreCase);
    }
}
