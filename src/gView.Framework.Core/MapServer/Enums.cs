using System;

namespace gView.Framework.Core.MapServer
{

    public enum MapServiceType { MXL, SVC, GDI, Folder }

    public enum MapServiceStatus
    {
        Running = 0,
        Stopped = 1,
        Idle = 2
    }

    [Flags]
    public enum AccessTypes
    {
        None = 0,
        Map = 1,
        Query = 2,
        Edit = 4
    }

    public enum FolderAccessTypes
    {
        None = 0,
        Map = 1,
        Query = 2,
        Edit = 4,
        Publish = 8
    }
}
