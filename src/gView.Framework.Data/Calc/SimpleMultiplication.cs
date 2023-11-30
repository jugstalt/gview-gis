using gView.Framework.Core.IO;
using gView.Framework.Core.system;
using gView.Framework.Common;
using System;

namespace gView.Framework.Data.Calc
{
    [RegisterPlugIn("C25E2681-E904-4d4c-A094-3ADD71E8BC54")]
    public class SimpleMultiplication : ISimpleNumberCalculation
    {
        private double _multiplicator = 1.0;
        private double _zeroValue = 0.0;

        #region Properties
        public double Multiplicator
        {
            get { return _multiplicator; }
            set { _multiplicator = value; }
        }

        [System.ComponentModel.DisplayName("Null Value Result")]
        [System.ComponentModel.Description("Returned value, if input value is Zero.")]
        public double NullValueResult
        {
            get { return _zeroValue; }
            set { _zeroValue = value; }
        }
        #endregion

        #region ISimpleNumberCalculation Member
        [System.ComponentModel.Browsable(false)]
        public string Name
        {
            get { return "Multiplication"; }
        }
        [System.ComponentModel.Browsable(false)]
        public string Description
        {
            get { return String.Empty; }
        }

        virtual public double Calculate(double val)
        {
            if (val == 0.0)
            {
                return _zeroValue;
            }

            return val * _multiplicator;
        }

        #endregion

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            _multiplicator = (double)stream.Load("multiplicator", 1.0);
            _zeroValue = (double)stream.Load("zvr", (double)1.0);
        }

        public void Save(IPersistStream stream)
        {
            stream.Save("multiplicator", _multiplicator);
            stream.Save("zvr", _zeroValue);
        }

        #endregion
    }
}
