using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace gView.Server.AppCode.Extensions
{
    static public class ValidationExtensions
    {
        public const string ServiceNameRegex = "^(?=.{3,128}$)(?![_-])(?!.*[_-]{2})[a-zA-Z0-9-_]+(?<![_-])$";
        public const string FolderNameRegex = "^(?=.{3,64}$)(?![_-])(?!.*[_-]{2})[a-zA-Z0-9-_]+(?<![_-])$";

        public static bool CheckRegex(this string input, string regex)
        {
            if (String.IsNullOrWhiteSpace(regex))
                return true;

            return new Regex(regex).IsMatch(input);
        }

        public static bool IsValidServiceName(this string serviceName)
        {
            if (String.IsNullOrWhiteSpace(serviceName))
            {
                return false;
            }

            return serviceName.CheckRegex(ServiceNameRegex);
        }

        public static bool IsValidFolderName(this string folderName)
        {
            if (String.IsNullOrWhiteSpace(folderName))
            {
                return false;
            }

            return folderName.CheckRegex(FolderNameRegex);
        }
    }
}
