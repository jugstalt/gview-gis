using System;
using System.IO;

namespace gView.Blazor.Core.Extensions;

static public class IOExtensions
{
    static public bool TryCreateDirectory(this string path)
    {
        try
        {
            if(!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return true;
        }
        catch { return false; }
    }

    static public string UserNameToFolder(this string username)
    {
        char[] invalidChars = Path.GetInvalidFileNameChars();
        string validName = string.Join("_", username.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));

        validName = validName.Trim();
        validName = validName.Replace(" ", "_");

        if (string.IsNullOrWhiteSpace(validName))
        {
            throw new ArgumentException("the resulting foldername is empty");
        }

        return validName.ToLower();
    }
}
