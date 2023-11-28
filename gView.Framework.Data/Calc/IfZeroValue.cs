using gView.Framework.Core.IO;
using gView.Framework.Core.system;
using gView.Framework.system;

namespace gView.Framework.Data.Calc
{

    [RegisterPlugIn("AEE5A60F-3743-4c66-93A4-02FD24527C24")]
    public class IfZeroValue : ISimpleNumberCalculation
    {
        private double _zeroValue = 0.0;

        #region Properties
        [System.ComponentModel.DisplayName("Null Value Result")]
        [System.ComponentModel.Description("Returned value, if input value is Zero.")]
        public double NullValueResult
        {
            get { return _zeroValue; }
            set { _zeroValue = value; }
        }
        #endregion

        #region ISimpleNumberCalculation Member

        public string Name
        {
            get { return "If Zero"; }
        }

        [System.ComponentModel.Browsable(false)]
        public string Description
        {
            get { return "Return value if input is zero."; }
        }

        [System.ComponentModel.Browsable(false)]
        public double Calculate(double val)
        {
            if (val == 0.0)
            {
                return _zeroValue;
            }

            return val;
        }

        #endregion

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            _zeroValue = (double)stream.Load("zvr", (double)1.0);
        }

        public void Save(IPersistStream stream)
        {
            stream.Save("zvr", _zeroValue);
        }

        #endregion
    }
}
