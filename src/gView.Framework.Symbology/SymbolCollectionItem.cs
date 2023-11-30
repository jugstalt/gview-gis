using gView.Framework.Core.IO;
using gView.Framework.Core.Symbology;
using gView.Framework.Core.system;

namespace gView.Framework.Symbology
{
    public sealed class SymbolCollectionItem : ISymbolCollectionItem, IPersistable
    {
        private bool _visible;
        private ISymbol _symbol;

        public SymbolCollectionItem(ISymbol symbol, bool visible)
        {
            Symbol = symbol;
            Visible = visible;
        }
        public SymbolCollectionItem Clone(CloneOptions options)
        {
            if (Symbol != null)
            {
                return new SymbolCollectionItem((ISymbol)Symbol.Clone(options), Visible);
            }
            else
            {
                return new SymbolCollectionItem(null, Visible);
            }
        }

        #region IPersistable Member

        public string PersistID
        {
            get
            {
                return null;
            }
        }

        public void Load(IPersistStream stream)
        {
            Visible = (bool)stream.Load("visible", false);
            Symbol = (ISymbol)stream.Load("ISymbol");
        }

        public void Save(IPersistStream stream)
        {
            stream.Save("visible", Visible);
            stream.Save("ISymbol", Symbol);
        }

        #endregion

        #region ISymbolCollectionItem Member

        public bool Visible
        {
            get { return _visible; }
            set { _visible = value; }
        }

        public ISymbol Symbol
        {
            get { return _symbol; }
            set { _symbol = value; }
        }

        #endregion
    }
}
