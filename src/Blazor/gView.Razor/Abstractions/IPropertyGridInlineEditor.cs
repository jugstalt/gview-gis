namespace gView.Razor.Abstractions;

public interface IPropertyGridInlineEditor : IPropertyGridEditor
{
    void SetInstance(object? instance);
    object? GetInstance();
}
