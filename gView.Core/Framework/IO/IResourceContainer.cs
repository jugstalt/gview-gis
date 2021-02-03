using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Framework.IO
{
    public interface IResourceContainer : IPersistable
    {
        IEnumerable<string> Names { get; }

        byte[] this[string name] { get; set; }

        bool HasResources { get;}
    }

    public interface IResource
    {
        string Name { get; }
        byte[] Data { get; }
    }
}
