using gView.Deploy.Reflection;

namespace gView.Deploy.Models;

internal class DeployVersionModel
{
    public string ProfileName { get; set; } = "";

    //[ModelProperty(Prompt = "Company",
    //               DefaultValue = "my-company",
    //               Placeholder = "company",
    //               RegexPattern = "^[a-z0-9-]+$",
    //               RegexNotMatchMessage = "only lowercache letters, numbers und '-' is allowed, eg 'my-company'")]
    //public string Company { get; set; } = "";

    [ModelProperty(Prompt = "Target installation path",
                   DefaultValue = "C:\\apps\\gview6")]
    public string TargetInstallationPath { get; set; } = "";

    [ModelProperty(Prompt = "Repsitory path",
                   DefaultValue = "{TargetInstallationPath}/{ProfileName}/gview-repository",
                   Placeholder = "repository-path")]
    public string RepositoryPath { get; set; } = "";

    [ModelProperty(Prompt = "gView Server online url",
                   DefaultValue = "http://localhost:5050",
                   Placeholder = "server-url")]
    public string ServerOnlineResource { get; set; } = "";
}
