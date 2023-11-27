using gView.Framework.Core.IO;
using gView.Framework.Core.Symbology;
using gView.Framework.system;
using System.ComponentModel;

namespace gView.Framework.Symbology
{
    public class LegendItem : Cloner, ILegendItem, IPersistable
    {
        protected string _legendLabel = "";
        protected bool _showInTOC = true;

        #region ILegendInfo Members

        [Browsable(false)]
        public string LegendLabel
        {
            get
            {
                return _legendLabel;
            }
            set
            {
                _legendLabel = value;
            }
        }

        [Browsable(false)]
        public bool ShowInTOC
        {
            get
            {
                return _showInTOC;
            }
            set
            {
                _showInTOC = value;
            }
        }

        [Browsable(false)]
        virtual public int IconHeight
        {
            get { return 0; }
        }

        #endregion

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            _legendLabel = (string)stream.Load("legendLabel", "");
            _showInTOC = (bool)stream.Load("showInTOC", true);
        }

        public void Save(IPersistStream stream)
        {
            stream.Save("legendLabel", _legendLabel);
            stream.Save("showInTOC", _showInTOC);
        }

        #endregion
    }
}
