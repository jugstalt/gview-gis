using System;

/// <summary>
/// The <c>gView.Framework</c> provides all interfaces to develope
/// with and for gView
/// </summary>
namespace gView.Framework.system
{
    [global::System.AttributeUsageAttribute(global::System.AttributeTargets.Class)]
    public class RegisterPlugInAttribute : global::System.Attribute
    {
        private Guid _guid;
        public RegisterPlugInAttribute(string guid, PluginUsage usage = PluginUsage.Server | PluginUsage.Desktop)
        {
            _guid = new Guid(guid);
            this.Usage = usage;
        }
        public Guid Value
        {
            get { return _guid; }
        }

        public PluginUsage Usage { get; set; }

        public bool Obsolete { get; set; }
    }
}
