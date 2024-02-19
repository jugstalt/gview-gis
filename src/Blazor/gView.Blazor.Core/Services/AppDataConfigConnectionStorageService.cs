using gView.Blazor.Core.Services.Abstraction;
using gView.Framework.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace gView.Blazor.Core.Services;

public class AppDataConfigConnectionStorageService : IConfigConnectionStorageService
{
    private readonly Encoding _encoding = Encoding.Default;

    public Dictionary<string, string> GetAll(string schema)
    {
        throw new NotImplementedException();
    }

    public bool Store(string schema, string name, string data)
    {
        using StreamWriter sw = new StreamWriter(
                Path.Combine(GetRoot(schema), $"{ReplaceSlash(name)}.con"), false, _encoding);

        sw.Write(data);
        sw.Close();

        return true;
    }

    public bool Delete(string schema, string name)
    {
        throw new NotImplementedException();
    }

    public bool Rename(string schema, string oldName, string newName)
    {
        throw new NotImplementedException();
    }

    #region Helper

    private string GetRoot(string schema)
    {
        string root = System.IO.Path.Combine(SystemVariables.MyApplicationConfigPath, "connections", schema);
        DirectoryInfo di = new DirectoryInfo(root);

        if (!di.Exists)
        {
            di.Create();
        }

        return root;
    }

    private string ReplaceSlash(string name)
    {
        return name.Replace("/", "&slsh;").Replace("\\", "&bkslsh;").Replace(":", "&colon;");
    }
    private string InvReplaceSlash(string name)
    {
        return name.Replace("&slsh;", "/").Replace("&bkslsh;", "\\").Replace("&colon;", ":");
    }

    #endregion
}
