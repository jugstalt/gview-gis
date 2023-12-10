using gView.Framework.Core.IO;
using gView.Framework.Core.Common;
using gView.Framework.Common;

namespace gView.Framework.Data.Calc
{
    [RegisterPlugIn("893A3D1A-BABD-4772-B2CC-A0F638D50B02")]
    public class ReciprocalValue : ISimpleNumberCalculation
    {
        protected double _zeroValue = 0.0;

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
            get { return "Reciprocal Value"; }
        }

        [System.ComponentModel.Browsable(false)]
        virtual public string Description
        {
            get { return "Return Reciprocal Value"; }
        }

        [System.ComponentModel.Browsable(false)]
        public double Calculate(double val)
        {
            if (val == 0.0)
            {
                return _zeroValue;
            }

            return 1.0 / val;
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
