using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace gView.Framework.Common
{
    public class Crypto
    {
        public enum ResultType
        {
            Base64,
            Hex
        }

        public static byte[] Encrypt(byte[] clearData, byte[] Key, byte[] IV)
        {

            using (MemoryStream ms = new MemoryStream())
            using (var alg = Aes.Create())
            {
                alg.Key = Key;
                alg.IV = IV;

                CryptoStream cs = new CryptoStream(ms, alg.CreateEncryptor(), CryptoStreamMode.Write);

                cs.Write(clearData, 0, clearData.Length);
                cs.Close();

                byte[] encryptedData = ms.ToArray();

                return encryptedData;
            }

        }
        public static string Encrypt(string clearText, string Password, ResultType resultType = ResultType.Base64)
        {
            if (string.IsNullOrEmpty(clearText))
            {
                return string.Empty;
            }

            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);

            PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password,

                        new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4e, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });

            byte[] encryptedData = Encrypt(clearBytes, pdb.GetBytes(32), pdb.GetBytes(16));

            switch (resultType)
            {
                case ResultType.Hex:
                    return "0x" + string.Concat(encryptedData.Select(b => b.ToString("X2")));
                default: // base64
                    return Convert.ToBase64String(encryptedData);
            }
        }
        public static byte[] Encrypt(byte[] clearData, string Password)
        {
            PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password,
                        new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4e, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });

            return Encrypt(clearData, pdb.GetBytes(32), pdb.GetBytes(16));
        }

        public static byte[] Decrypt(byte[] cipherData, byte[] Key, byte[] IV)
        {
            using (MemoryStream ms = new MemoryStream())
            using (var alg = Aes.Create())
            {
                alg.Key = Key;
                alg.IV = IV;

                CryptoStream cs = new CryptoStream(ms, alg.CreateDecryptor(), CryptoStreamMode.Write);

                cs.Write(cipherData, 0, cipherData.Length);
                cs.Close();

                byte[] decryptedData = ms.ToArray();

                return decryptedData;
            }

        }
        public static string Decrypt(string cipherText, string Password)
        {
            if (string.IsNullOrEmpty(cipherText))
            {
                return string.Empty;
            }

            byte[] cipherBytes = null;
            if (IsHexString(cipherText))
            {
                cipherBytes = StringToByteArray(cipherText);
            }
            else
            {
                cipherBytes = Convert.FromBase64String(cipherText);
            }


            PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password,

                        new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4e, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });


            byte[] decryptedData = Decrypt(cipherBytes, pdb.GetBytes(32), pdb.GetBytes(16));

            return Encoding.Unicode.GetString(decryptedData);
        }
        public static byte[] Decrypt(byte[] cipherData, string Password)
        {
            PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password,
                        new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4e, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });


            return Decrypt(cipherData, pdb.GetBytes(32), pdb.GetBytes(16));
        }

        //public static string Hash64(string password)
        //{
        //    byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
        //    passwordBytes = SHA256.Create().ComputeHash(passwordBytes);
        //    return Convert.ToBase64String(passwordBytes);
        //}

        #region Helper

        static private byte[] StringToByteArray(string hex)
        {
            if (hex.StartsWith("0x"))
            {
                hex = hex.Substring(2, hex.Length - 2);
            }

            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }

            return bytes;
        }

        static private bool IsHexString(string hex)
        {
            if (hex.StartsWith("0x"))
            {
                hex = hex.Substring(2, hex.Length - 2);
            }

            bool isHex;
            foreach (var c in hex)
            {
                isHex = c >= '0' && c <= '9' ||
                         c >= 'a' && c <= 'f' ||
                         c >= 'A' && c <= 'F';

                if (!isHex)
                {
                    return false;
                }
            }
            return true;
        }

        #endregion
    }
}
