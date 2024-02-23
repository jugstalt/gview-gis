using gView.Blazor.Core.Services.Abstraction;
using gView.Framework.Common;
using gView.Framework.Core.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace gView.Blazor.Core.Services;

public class AppDataConfigConnectionStorageService : IConfigConnectionStorageService
{
    private const string ConFileExtension = ".con";
    private const string AclFileExtension = ".acl";
    private readonly IAppIdentityProvider _identityProvider;

    public AppDataConfigConnectionStorageService(IAppIdentityProvider identityProvider)
    {
        _identityProvider = identityProvider;
    }

    public Dictionary<string, string> GetAll(string schema)
    {
        Dictionary<string, string> connections = new Dictionary<string, string>();
        string root = GetRoot(schema);

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
            var acl = Acl.Open(
                    _identityProvider,
                    fi.FullName.Substring(0, fi.FullName.LastIndexOf(".")) + AclFileExtension
                );

            if (acl.IsAuthorized())
            {
                StreamReader sr = new StreamReader(fi.FullName, Encoding.UTF8);
                string conn = sr.ReadToEnd();
                sr.Close();

                string name = InvReplaceSlash(fi.Name.Substring(0, fi.Name.Length - 4));
                connections.Add(name, conn);
            }
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
        while (di.GetFiles($"{ReplaceSlash(ret)}{ConFileExtension}").Length != 0)
        {
            ret = $"{name} ({++i})";
        }

        return ret;
    }

    public bool Store(string schema, string name, string data)
    {
        try
        {
            using StreamWriter sw = new StreamWriter(
                    GetConFilePath(schema, name), false, Encoding.UTF8);

            sw.Write(data);
            sw.Close();

            var acl = Acl.Create(_identityProvider, GetAclFilePath(schema, name));
            acl.Write();

            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool Delete(string schema, string name)
    {
        var acl = Acl.Open(_identityProvider, GetAclFilePath(schema, name));
        if (acl.IsCreator() || _identityProvider.Identity.IsAdministrator)
        {
            DeleteFile(schema, name, AclFileExtension);
            return DeleteFile(schema, name, ConFileExtension);
        }
        return false;
    }

    public bool Rename(string schema, string oldName, string newName)
    {
        var acl = Acl.Open(_identityProvider, GetAclFilePath(schema, oldName));
        if (acl.IsCreator() || _identityProvider.Identity.IsAdministrator)
        {
            RenameFile(schema, oldName, newName, AclFileExtension);
            return RenameFile(schema, oldName, newName, ConFileExtension);
        }
        return false;
    }

    public ConfigAccessability GetAccessability(string schema, string name)
    {
        var acl = Acl.Open(_identityProvider, GetAclFilePath(schema, name));

        return acl switch
        {
            { AllowUsers: true } => ConfigAccessability.Users,
            { AllowAdministrors: true } => ConfigAccessability.Administrators,
            _ => ConfigAccessability.Creator
        };
    }
    public bool SetAccessability(string schema, string name, ConfigAccessability accessability)
    {
        var acl = Acl.Open(_identityProvider, GetAclFilePath(schema, name));
        if (acl.IsCreator())
        {
            switch (accessability)
            {
                case ConfigAccessability.Creator:
                    acl.AllowUsers = acl.AllowAdministrors = false;
                    break;
                case ConfigAccessability.Administrators:
                    acl.AllowAdministrors = true;
                    acl.AllowUsers = false;
                    break;
                case ConfigAccessability.Users:
                    acl.AllowUsers = acl.AllowAdministrors = true;
                    break;
            }

            acl.Write();
        }


        return false;

    }

    #region Helper

    private bool DeleteFile(string schema, string name, string extension)
    {
        try
        {
            FileInfo fi = new FileInfo(GetFilePath(schema, name, extension));
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

    private bool RenameFile(string schema, string oldName, string newName, string extension)
    {
        try
        {
            FileInfo fi = new FileInfo(GetFilePath(schema, oldName, extension));
            if (fi.Exists)
            {
                fi.MoveTo(GetFilePath(schema, newName, extension));
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    private string GetRoot(string schema)
    {
        string root = Path.Combine(
                SystemVariables.MyApplicationConfigPath,
                "connections",
                schema
            );
        DirectoryInfo di = new DirectoryInfo(root);

        if (!di.Exists)
        {
            di.Create();
        }

        return root;
    }
    private string GetFilePath(string schema, string name, string extension)
        => Path.Combine(GetRoot(schema), $"{ReplaceSlash(name)}{extension}");

    private string GetConFilePath(string schema, string name)
        => GetFilePath(schema, name, ConFileExtension);

    private string GetAclFilePath(string schema, string name)
        => GetFilePath(schema, name, AclFileExtension);

    private string ReplaceSlash(string name)
    {
        return name.Replace("/", "&slsh;").Replace("\\", "&bkslsh;").Replace(":", "&colon;");
    }
    private string InvReplaceSlash(string name)
    {
        return name.Replace("&slsh;", "/").Replace("&bkslsh;", "\\").Replace("&colon;", ":");
    }

    #endregion

    #region Classes

    public class Acl
    {
        private readonly IAppIdentityProvider _identityProvider;
        private readonly string _filename;

        private Acl(IAppIdentityProvider identityProvider,
                   string filename)
        {
            _identityProvider = identityProvider;
            _filename = filename;

            var fi = new FileInfo(filename);
            if (fi.Exists)
            {
                var lines = File.ReadAllLines(fi.FullName)
                    .Where(l => !String.IsNullOrWhiteSpace(l))
                    .ToArray();

                this.Creator = lines
                    .Where(l => l.StartsWith("creator:"))
                    .Select(l => l.Substring("creator:".Length))
                    .FirstOrDefault() ?? "";

                AllowAdministrors = lines.Contains($"role:{identityProvider.AdministratorsRoleName}");
                AllowUsers = lines.Contains($"role:{identityProvider.UsersRoleName}");
            }
        }

        public static Acl Create(IAppIdentityProvider identityProvider,
                                 string filename)
        {
            return new Acl(identityProvider, filename) { Creator = identityProvider.Identity.Username };
        }

        public static Acl Open(IAppIdentityProvider identityProvider,
                               string filename)
        {
            return new Acl(identityProvider, filename);
        }

        public string Creator { get; private set; } = "";
        public bool AllowAdministrors { get; set; }
        public bool AllowUsers { get; set; }

        public bool Write()
        {
            try
            {
                FileInfo fi = new FileInfo(_filename);
                if (fi.Exists) { fi.Delete(); }

                List<string> lines = new();
                lines.Add($"creator:{this.Creator}");
                if (AllowAdministrors && !String.IsNullOrEmpty(_identityProvider.AdministratorsRoleName))
                {
                    lines.Add($"role:{_identityProvider.AdministratorsRoleName}");
                }
                if (AllowUsers && !String.IsNullOrEmpty(_identityProvider.UsersRoleName))
                {
                    lines.Add($"role:{_identityProvider.UsersRoleName}");
                }

                File.WriteAllLines(fi.FullName, lines);

                return true;
            }
            catch { return false; }
        }

        public bool IsCreator()
            => !String.IsNullOrEmpty(this.Creator)
                && this.Creator.Equals(_identityProvider.Identity.Username, StringComparison.OrdinalIgnoreCase);

        public bool IsAuthorized()
        {
            if (IsCreator())
            {
                return true;
            }

            if (AllowAdministrors && _identityProvider.Identity.IsAdministrator)
            {
                return true;
            }

            if (AllowUsers
                && (_identityProvider.Identity.IsAuthorizedUser || _identityProvider.Identity.IsAdministrator))
            {
                return true;
            }

            return false;
        }
    }

    #endregion
}
