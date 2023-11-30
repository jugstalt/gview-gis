using System;

/// <summary>
/// The <c>gView.Framework</c> provides all interfaces to develope
/// with and for gView
/// </summary>
namespace gView.Framework.Core.system
{

    [Flags]
    public enum PluginUsage
    {
        Server = 1,
        Desktop = 2
    }

    public enum loggingMethod { request, request_detail, error, request_detail_pro }

    public enum LicenseTypes
    {
        Express = 0,
        Licensed = 1,
        Expired = 2,
        Unknown = 3
    }
}
