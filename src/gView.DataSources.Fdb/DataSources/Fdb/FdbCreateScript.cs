using System;
using System.IO;
using System.Reflection;

namespace gView.DataSources.Fdb
{
    /// <summary>
    /// Loads an FDB provider's <c>createdatabase.sql</c> DDL. Prefers the DDL embedded in the
    /// provider assembly (<c>&lt;AssemblyName&gt;.Resources.createdatabase.sql</c>) so a deployment
    /// does not have to ship a loose <c>sql/</c> folder; falls back to the historical file path.
    /// </summary>
    public static class FdbCreateScript
    {
        public static string Load(Assembly providerAssembly, string legacyFilePath)
        {
            string resourceName = providerAssembly.GetName().Name + ".Resources.createdatabase.sql";

            using (Stream stream = providerAssembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    using var reader = new StreamReader(stream);
                    return reader.ReadToEnd();
                }
            }

            if (!String.IsNullOrEmpty(legacyFilePath) && File.Exists(legacyFilePath))
            {
                return File.ReadAllText(legacyFilePath);
            }

            throw new FileNotFoundException(
                $"FDB create script not found (embedded resource '{resourceName}' missing and no file at '{legacyFilePath}').");
        }
    }
}
