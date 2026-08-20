namespace gView.Deploy.Reflection;

public enum PropertyFormat
{
    Normal,
    Hash256,
    Hash512
}

internal class ModelPropertyAttribute : Attribute
{
    public string Prompt { get; set; } = String.Empty;
    public string Description { get; set; } = String.Empty;
    public string DefaultValue { get; set; } = String.Empty;
    public bool Required { get; set; } = true;

    public string Placeholder { get; set; } = String.Empty;

    /// <summary>
    /// Name of the command line parameter (without leading "--") used to pass this
    /// property's value non-interactively, e.g. "server-url". If empty, the flag is
    /// derived from the property name (kebab-case), e.g. "RepositoryPath" -> "repository-path".
    /// </summary>
    public string CliName { get; set; } = String.Empty;

    public string RegexPattern { get; set; } = String.Empty;
    public string RegexNotMatchMessage { get; set; } = String.Empty;

    public PropertyFormat PropertyFormat { get; set;} = PropertyFormat.Normal;

    public bool IsPassword { get; set; } = false;
}
