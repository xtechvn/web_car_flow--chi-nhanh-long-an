using System.Security.Cryptography;
using System.Text;

namespace LIB.Utilities.Common
{
    public static class EncryptionHelper
    {
        public static string MD5Hash(string input)
        {
            StringBuilder hash = new StringBuilder();
            byte[] bytes = MD5.HashData(new UTF8Encoding().GetBytes(input));

            for (int i = 0; i < bytes.Length; i++)
            {
                hash.Append(bytes[i].ToString("x2"));
            }
            return hash.ToString();
        }
    }
}
