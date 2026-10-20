using System;

namespace gView.Framework.Core.Common
{
    /// <summary>
    /// Declares the decorated plugin as fallback for another (original) plugin.
    /// If the original plugin implements <see cref="IPlugInDependencies"/> and has
    /// unsolved dependencies, the PlugInManager creates this plugin instead.
    /// The fallback has to be able to load what the original plugin persisted.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class PlugInFallbackForAttribute : Attribute
    {
        public PlugInFallbackForAttribute(string originalGuid)
        {
            OriginalGuid = new Guid(originalGuid);
        }

        public Guid OriginalGuid { get; }
    }
}
