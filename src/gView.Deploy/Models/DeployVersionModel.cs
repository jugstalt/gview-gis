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
                   DefaultValue = "C:\\apps\\gview-gis")]
    public string TargetInstallationPath { get; set; } = "";

    [ModelProperty(Prompt = "Repsitory path",
                   DefaultValue = "{TargetInstallationPath}/{ProfileName}/gview-repository",
                   Placeholder = "repository-path")]
    public string RepositoryPath { get; set; } = "";

    [ModelProperty(Prompt = "gView Server online url",
                   DefaultValue = "http://localhost:5050",
                   Placeholder = "server-url")]
    public string ServerOnlineResource { get; set; } = "";

    [ModelProperty(Prompt = "gView Admin Username",
                   DefaultValue = "admin",
                   Placeholder = "admim-username")]
    public string AdminUsername { get; set; } = "";

    [ModelProperty(Prompt = "gView Admin Password",
                   DefaultValue = "*****",
                   Placeholder = "admim-password",
                   PropertyFormat = PropertyFormat.Hash512,
                   RegexPattern = "^(?=.*[A-Z])(?=.*\\d)(?=.*[!@#$%^&*()_+{}:\"<>?~;']).{8,}$",
                   RegexNotMatchMessage = "the password must be at least 8 characters long and include at least one uppercase letter, one number, and one special character (e.g., !@#$%^&*()_+{}:\"<>?~;').")]
    public string AdminPassword { get; set; } = "";


    [ModelProperty(Prompt = "gView User Username",
                   DefaultValue = "carto",
                   Placeholder = "carto-username")]
    public string CartoUsername { get; set; } = "";

    [ModelProperty(Prompt = "gView Carto Password",
                   DefaultValue = "*****",
                   Placeholder = "carto-password",
                   PropertyFormat = PropertyFormat.Hash256,
                   RegexPattern = "^(?=.*[A-Z])(?=.*\\d)(?=.*[!@#$%^&*()_+{}:\"<>?~;']).{8,}$",
                   RegexNotMatchMessage = "the password must be at least 8 characters long and include at least one uppercase letter, one number, and one special character (e.g., !@#$%^&*()_+{}:\"<>?~;').")]
    public string CartoPassword { get; set; } = "";
}
