using gView.Framework.Core.IO;
using gView.Framework.Core.Common;
using gView.Framework.Common;

namespace gView.Framework.Data.Calc
{
    [RegisterPlugIn("B8C40E43-71D1-4b0a-95F7-749093547F90")]
    public class Fraction : ISimpleNumberCalculation
    {
        private double _numerator = 1.0;
        private double _zeroValue = 1.0;

        #region Properties
        [System.ComponentModel.DisplayName("Null Value Result")]
        [System.ComponentModel.Description("Returned value, if input value is Zero.")]
        public double NullValueResult
        {
            get { return _zeroValue; }
            set { _zeroValue = value; }
        }

        [System.ComponentModel.DisplayName("Numerator")]
        [System.ComponentModel.Description("Fraction Numerator: f(x) = Numerator / x")]
        public double Numerator
        {
            get { return _numerator; }
            set { _numerator = value; }
        }
        #endregion

        [System.ComponentModel.Browsable(false)]
        public string Name
        {
            get
            {
                return "Fraction";
            }
        }

        public string Description
        {
            get
            {
                return "f(x) = Numerator / x";
            }
        }

        public double Calculate(double val)
        {
            if (val == 0.0)
            {
                return _zeroValue;
            }

            return _numerator / val;
        }

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            _numerator = (double)stream.Load("numerator", (double)1.0);
            _zeroValue = (double)stream.Load("zvr", (double)1.0);
        }

        public void Save(IPersistStream stream)
        {
            stream.Save("numerator", _numerator);
            stream.Save("zvr", _zeroValue);
        }

        #endregion
    }
}
