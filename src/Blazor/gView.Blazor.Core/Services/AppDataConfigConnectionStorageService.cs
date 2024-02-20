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
        Dictionary<string, string> connections = new Dictionary<string, string>();
        string root=GetRoot(schema);

        if (String.IsNullOrEmpty(root))
        {
            return connections;
        }

        DirectoryInfo di = new DirectoryInfo(root);
        if (!di.Exists)
        {
            return connections;
        }

        foreach (FileInfo fi in di.GetFiles("*.con"))
        {
            StreamReader sr = new StreamReader(fi.FullName, _encoding);
            string conn = sr.ReadToEnd();
            sr.Close();

            string name = InvReplaceSlash(fi.Name.Substring(0, fi.Name.Length - 4));
            connections.Add(name, conn);
        }

        return connections;
    }

    public string GetNewName(string schema, string name)
    {
        string root = GetRoot(schema);

        if (String.IsNullOrEmpty(root))
        {
            return String.Empty;
        }

        DirectoryInfo di = new DirectoryInfo(root);
        string ret = name;

        int i = 1;
        while (di.GetFiles($"{ReplaceSlash(ret)}.con").Length != 0)
        {
            ret = $"{name} ({++i})";
        }

        return ret;
    }

    public bool Store(string schema, string name, string data)
    {
        string root = GetRoot(schema);

        if (String.IsNullOrEmpty(root))
        {
            return false;
        }

        using StreamWriter sw = new StreamWriter(
                Path.Combine(root, $"{ReplaceSlash(name)}.con"), false, _encoding);

        sw.Write(data);
        sw.Close();

        return true;
    }

    public bool Delete(string schema, string name)
    {
        string root = GetRoot(schema);

        if (String.IsNullOrEmpty(root))
        {
            return false;
        }

        try
        {
            FileInfo fi = new FileInfo(Path.Combine(root, $"{ReplaceSlash(name)}.con"));
            if (fi.Exists)
            {
                fi.Delete();
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool Rename(string schema, string oldName, string newName)
    {
        string root = GetRoot(schema);

        if (String.IsNullOrEmpty(root))
        {
            return false;
        }

        try
        {
            FileInfo fi = new FileInfo(Path.Combine(root, $"{ReplaceSlash(oldName)}.con"));
            if (fi.Exists)
            {
                fi.MoveTo(Path.Combine(root, $"{ReplaceSlash(newName)}.con"));
            }

            return true;
        }
        catch
        {
            return false;
        }
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
