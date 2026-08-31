using gView.Blazor.Models.Dialogs;
using gView.Framework.Core.Data;

namespace gView.Carto.Razor.Components.Dialogs.Models;
public class LabelExpressionModel : IDialogResultItem
{
    public ITableClass? TableClass { get; set; }
    public string Expression { get; set; } = "";

    /// <summary>
    /// When <see langword="true"/> the dialog also shows the accepted colour-value formats:
    /// the expression result is fed to <c>ArgbColor.TryFromString(...)</c> and must resolve
    /// to a colour (see <see cref="LabelExpressionHelp.ColorValues"/>).
    /// </summary>
    public bool IsColorExpression { get; set; }
}

public static class LabelExpressionHelp
{
    /// <summary>
    /// The colour formats accepted by <c>gView.GraphicsEngine.ArgbColor.TryFromString(...)</c>.
    /// Keep in sync with <c>ArgbColor.FromString</c>.
    /// </summary>
    public const string ColorValues =
@"The expression result is passed to ArgbColor.TryFromString() and must
resolve to a colour. Accepted formats (spaces are ignored):

  #rgb  #rgba  #rrggbb  #aarrggbb   hex, leading '#' required
  r,g,b            r,g,b,a          0-255 per channel (a optional, default 255)
  rgb:r,g,b                         0-255
  rgb(r,g,b)                        0-255
  rgba(r,g,b,a)                     r,g,b 0-255 ; a = 0..1
  hsl(h,s%,l%)                      h 0-360 ; s,l 0-100
  hsla(h,s%,l%,a)                   a = 0..1
  cmyk(c,m,y,k)                     0-100 per channel
  <name>                           black white red green blue yellow cyan
                                   magenta silver gray maroon olive purple
                                   teal navy lime orange brown pink gold
                                   beige coral indigo violet turquoise tan
                                   skyblue salmon plum orchid mint ivory
                                   azure lavender

An empty result - or anything not recognised as a colour - means
'use the text symbol's own colour for this feature'.";
}
