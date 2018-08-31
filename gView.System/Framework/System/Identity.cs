using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace gView.Framework.system
{
    public class Identity : IIdentity
    {
        private string _name;
        private List<string> _roles;
        private string _hashedPassword = String.Empty;

        public Identity(string username)
            : this(username, null)
        {
        }
        public Identity(string username, List<string> roles)
        {
            _name = username;
            _roles = (roles != null) ? roles : new List<string>();
        }

        public string UserName
        {
            get { return _name; }
        }

        public List<string> UserRoles
        {
            get { return _roles; }
        }

        public string ToFormattedString()
        {
            if (String.IsNullOrEmpty(_name)) return String.Empty;

            StringBuilder sb = new StringBuilder();
            sb.Append(encodeString(_name));

            if (_roles != null)
            {
                foreach (string role in _roles)
                {
                    if (String.IsNullOrEmpty(role)) continue;
                    sb.Append("|" + encodeString(role));
                }
            }
            return sb.ToString();
        }

        public string HashedPassword
        {
            get { return _hashedPassword; }
            set { _hashedPassword = value; }
        }

        public static Identity FromFormattedString(string str)
        {
            if (String.IsNullOrEmpty(str)) return new Identity("");
            string[] parts = str.Split('|');

            string name = decodeString(parts[0]);
            List<string> roles=new List<string>();
            for (int i = 1; i < parts.Length; i++)
                roles.Add(decodeString(parts[i]));

            return new Identity(name, roles);
        }

        #region Helper
        static private string encodeString(string str)
        {
            return str.Replace("|", "&pipe;");
        }
        static private string decodeString(string str)
        {
            return str.Replace("&pipe;", "|");
        }
        #endregion

        #region PasswordHash
        static public string HashPassword(string password)
        {
            if (String.IsNullOrEmpty(password)) return String.Empty;

            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] byteValue = UTF8Encoding.UTF8.GetBytes(password);
            byte[] byteHash = md5.ComputeHash(byteValue);
            md5.Clear();

            return Convert.ToBase64String(byteHash);
        }
        #endregion
    }
}
