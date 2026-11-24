using System;

namespace gView.Framework.Symbology.Models;

/// <summary>
/// Which part of a polygon symbol gets a new colour. Lines and points ignore it.
/// </summary>
[Flags]
public enum SymbolColorTarget
{
    Fill = 1,
    Outline = 2,
    Both = Fill | Outline
}
