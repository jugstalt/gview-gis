namespace gView.Blazor.Core.Services;

public enum IconSize
{
    Small,
    Medium,
    Large
}

public class IconService
{
    public string FromString(string iconName, IconSize size = IconSize.Medium)
     => iconName?.Contains(":") == true ?
            $"sketchpen-icon-{iconName.Split(':')[0]}-bg-light-{IconSizePixels(size)} {iconName.Split(':')[1]}" :
            $"sketchpen-icon-default-bg-light-24 {IconSizePixels(size)}";

    private int IconSizePixels(IconSize size)
        => size switch
        {
            IconSize.Small => 16,
            IconSize.Large => 32,
            _ => 24
        };
}
