using System;

/// <summary>
/// The <c>gView.Framework</c> provides all interfaces to develope
/// with and for gView
/// </summary>
namespace gView.Framework.Core.system
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RegisterPlugInAttribute : Attribute
    {
        private Guid _guid;
        public RegisterPlugInAttribute(string guid, PluginUsage usage = PluginUsage.Server | PluginUsage.Desktop)
        {
            _guid = new Guid(guid);
            Usage = usage;
        }
        public Guid Value
        {
            get { return _guid; }
        }

        public PluginUsage Usage { get; set; }

        public bool Obsolete { get; set; }
    }
}
