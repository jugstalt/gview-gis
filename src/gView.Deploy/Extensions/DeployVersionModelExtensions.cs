using gView.Deploy.Models;
using gView.Deploy.Reflection;
using System.Reflection;

namespace gView.Deploy.Extensions;

static internal class DeployVersionModelExtensions
{
    static public string ReplaceModelProperties(this DeployVersionModel model, string input)
    {
        if (model != null && !String.IsNullOrEmpty(input))
        {
            bool isJson = input.Trim().StartsWith("{") || input.Trim().StartsWith("[");

            var modelType = model.GetType();

            foreach (var property in modelType.GetProperties())
            {
                var modelPropertyAttribute = property.GetCustomAttribute<ModelPropertyAttribute>();

                if (!String.IsNullOrEmpty(modelPropertyAttribute?.Placeholder))
                {
                    string val = property.GetValue(model)?.ToString() ?? String.Empty;

                    if (isJson)
                    {
                        val = val.Replace("\\", "\\\\");
                    }

                    input = input.Replace($"{{{modelPropertyAttribute.Placeholder}}}", val);
                }
            }
        }

        return input;
    }

    static public string ProfileTargetInstallationPath(this DeployVersionModel model, string profile, string version)
    {
        if(model.TargetInstallationPath.EndsWith("!"))
        {
            return Path.Combine(
                    model.TargetInstallationPath.Substring(0, model.TargetInstallationPath.Length-1), 
                    version
                );
        }

        return Path.Combine(model.TargetInstallationPath, profile, version);
    }
}
