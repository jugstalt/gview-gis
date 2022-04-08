using System;

namespace gView.Framework.UI
{
    public interface IContextType
    {
        string ContextName { get; }
        string ContextGroupName { get; }
        Type ContextType { get; }
        object ContextObject { get; }
    }


}