using gView.Framework.Core.Reflection;
using gView.GraphicsEngine;
using gView.GraphicsEngine.Abstraction;
using gView.Razor.Abstractions;
using System.ComponentModel;

namespace gView.Carto.Plugins.PropertyGridEditors;

internal class FontPropertyEditor : IPropertyGridInlineEditor
{
    public FontPropertyEditor()
    {
        this.Name = "";
    }

    [Browsable(false)]
    public Type PropertyType => typeof(IFont);

    public void SetInstance(object? instance)
    {
        var font = instance as IFont;

        if (font is not null)
        {
            this.Name = font.Name;
            this.Size = font.Size;
            this.Style = font.Style;
        }
    }

    public object? GetInstance()
    {
        if (String.IsNullOrEmpty(this.Name))
        {
            return null;
        }

        return Current.Engine.CreateFont(Name, this.Size, this.Style);
    }

    [Browsable(true)]
    [PropertyDescription(SelectOptionsPropertyName = nameof(FontNames))]
    public string Name
    {
        get; set;
    }

    [Browsable(true)]
    public float Size
    {
        get; set;
    }

    [Browsable(true)]
    public FontStyle Style
    {
        get; set;
    }

    [Browsable(false)]
    public IEnumerable<string> FontNames
    {
        get => Current.Engine.GetInstalledFontNames();
    }
}
