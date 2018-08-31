using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gView.Framework.IO
{
    public interface IMetadataProvider : IPersistable
    {
        bool ApplyTo(object Object);
        string Name { get; }
    }
}
