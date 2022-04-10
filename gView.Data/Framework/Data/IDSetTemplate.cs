using System.Collections.Generic;

namespace gView.Framework.Data
{
    abstract public class IDSetTemplate<T>
    {
        abstract public List<T> IDs
        {
            get;
        }
    }
}
