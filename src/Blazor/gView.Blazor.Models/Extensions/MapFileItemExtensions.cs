using gView.Blazor.Models.Settings;
using System;
using System.Linq;

namespace gView.Blazor.Models.Extensions;

static public class MapFileItemExtensions
{
    static public string DisplayPath(this MapFileItem mapFileItem)
        => String.Join("/",
            mapFileItem.Path.Replace("\\", "/")
                           .Split("/")
                           .TakeLast(2));
}
