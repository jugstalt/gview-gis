using gView.Framework.Core.IO;
using gView.Framework.Core.Symbology;
using System.ComponentModel;

namespace gView.Framework.Symbology
{
    public class Symbol : LegendItem
    {
        private SymbolSmoothing _smothingMode = SymbolSmoothing.None;

        public Symbol() { }

        [Browsable(true)]
        public SymbolSmoothing Smoothingmode
        {
            get { return _smothingMode; }
            set { _smothingMode = value; }
        }

        new public void Load(IPersistStream stream)
        {
            base.Load(stream);

            this.Smoothingmode = (SymbolSmoothing)stream.Load("smoothing", (int)SymbolSmoothing.None);
        }

        new public void Save(IPersistStream stream)
        {
            base.Save(stream);

            stream.Save("smoothing", (int)this.Smoothingmode);
        }
    }
}
