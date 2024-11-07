using System.Collections.Generic;

namespace gView.Framework.Core.IO;

public interface IConfigConnectionStorage
{
    bool Store(string schema, string name, string data);
    bool Delete(string schema, string name);
    bool Rename(string schema, string oldName, string newName);

    Dictionary<string, string> GetAll(string schema);
    string GetNewName(string schema, string name);

    ConfigAccessability GetAccessability(string schema, string name);
    bool SetAccessability(string schema, string name, ConfigAccessability accessability);
}
