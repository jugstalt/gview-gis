using gView.Framework.Core.Carto;
using System;

namespace gView.Framework.Core.Symbology
{
    public interface IIconSymbol
    {
        string Filename { get; set; }

        float SizeX { get; set; }

        float SizeY { get; set; }

        void ReloadIfEmpty(IDisplay display, bool setSize);
    }
}
