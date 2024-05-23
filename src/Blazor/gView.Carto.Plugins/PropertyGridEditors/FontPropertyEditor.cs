using gView.Carto.Plugins.PropertyGridEditors.Models;
using gView.GraphicsEngine;
using gView.GraphicsEngine.Abstraction;
using gView.Razor.Abstractions;
using System.ComponentModel;

namespace gView.Carto.Plugins.PropertyGridEditors;

internal class FontPropertyEditor : IPropertyGridInlineEditor
{
    public FontPropertyEditor()
    {
        this.Name = new FontName();
    }

    [Browsable(false)]
    public Type PropertyType => typeof(IFont);

    public void SetInstance(object? instance)
    {
        var font = instance as IFont;

        if (font is not null)
        {
            this.Name = new FontName() { Value = font.Name };
            this.Size = font.Size;
            this.Style = font.Style;
        }
    }

    public object? GetInstance()
    {
        if (String.IsNullOrEmpty(this.Name.Value))
        {
            return null;
        }

        return Current.Engine.CreateFont(Name.Value, this.Size, this.Style);
    }

    [Browsable(true)]
    //[PropertyDescription(SelectOptionsPropertyName = nameof(FontNames))]
    public FontName Name
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
