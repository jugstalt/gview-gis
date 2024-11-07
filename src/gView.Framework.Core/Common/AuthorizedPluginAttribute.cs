using System;

/// <summary>
/// The <c>gView.Framework</c> provides all interfaces to develope
/// with and for gView
/// </summary>
namespace gView.Framework.Core.Common
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AuthorizedPluginAttribute : Attribute
    {
        public bool RequireAdminRole { get; set; }
    }
}
