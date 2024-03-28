namespace gView.Deploy.Reflection;

internal class ModelPropertyAttribute : Attribute
{
    public string Prompt { get; set; } = String.Empty;
    public string Description { get; set; } = String.Empty;
    public string DefaultValue { get; set; } = String.Empty;
    public bool Required { get; set; } = true;

    public string Placeholder { get; set; } = String.Empty;

    public string RegexPattern { get; set; } = String.Empty;
    public string RegexNotMatchMessage { get; set; } = String.Empty;
}
