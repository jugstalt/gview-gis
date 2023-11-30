using System;

namespace gView.Framework.Core.UI
{
    public interface IContextType
    {
        string ContextName { get; }
        string ContextGroupName { get; }
        Type ContextType { get; }
        object ContextObject { get; }
    }


}