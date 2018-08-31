using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using gView.Framework.system;

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
                return _zeroValue;

            return val * _multiplicator;
        }

        #endregion

        #region IPersistable Member

        public void Load(gView.Framework.IO.IPersistStream stream)
        {
            _multiplicator = (double)stream.Load("multiplicator", 1.0);
            _zeroValue = (double)stream.Load("zvr", (double)1.0);
        }

        public void Save(gView.Framework.IO.IPersistStream stream)
        {
            stream.Save("multiplicator", _multiplicator);
            stream.Save("zvr", _zeroValue);
        }

        #endregion
    }

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
                return _zeroValue;

            return 1.0 / val;
        }

        #endregion

        #region IPersistable Member

        public void Load(gView.Framework.IO.IPersistStream stream)
        {
            _zeroValue = (double)stream.Load("zvr", (double)1.0);
        }

        public void Save(gView.Framework.IO.IPersistStream stream)
        {
            stream.Save("zvr", _zeroValue);
        }

        #endregion
    }

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
                return _zeroValue;

            return _numerator / val;
        }

        #region IPersistable Member

        public void Load(gView.Framework.IO.IPersistStream stream)
        {
            _numerator = (double)stream.Load("numerator", (double)1.0);
            _zeroValue = (double)stream.Load("zvr", (double)1.0);
        }

        public void Save(gView.Framework.IO.IPersistStream stream)
        {
            stream.Save("numerator", _numerator);
            stream.Save("zvr", _zeroValue);
        }

        #endregion
    }

    [RegisterPlugIn("FAE19184-54A6-48a1-A8A7-62A0214E0F66")]
    public class EvalFunction : ISimpleNumberCalculation
    {
        private static IFormatProvider _nhi = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;
        private string _function = "x";

        #region Properties
        [System.ComponentModel.Description("f(x)=x")]
        public string Function
        {
            get { return _function; }
            set { _function = value.Replace("\n", "").Replace("\r", "").ToLower(); }
        }
        #endregion

        #region ISimpleNumberCalculation Member
        [System.ComponentModel.Browsable(false)]
        public string Name
        {
            get { return "Eval Function"; }
        }

        [System.ComponentModel.Browsable(false)]
        public string Description
        {
            get { return String.Empty; }
        }

        public double Calculate(double val)
        {
            string f = _function.Replace("x", val.ToString(_nhi));
            DataTable tab = new DataTable();
            tab.Columns.Add("func", typeof(string), f);
            DataRow row = tab.NewRow();
            tab.Rows.Add(row);
            return double.Parse((string)row["eval"]);
        }

        #endregion

        #region IPersistable Member

        public void Load(gView.Framework.IO.IPersistStream stream)
        {
            _function = (string)stream.Load("func", "x");
        }

        public void Save(gView.Framework.IO.IPersistStream stream)
        {
            stream.Save("func", _function);
        }

        #endregion
    }

    [RegisterPlugIn("7D7A36CF-8F6D-4dbd-B566-8AC93FE82893")]
    public class ConstantValue : ISimpleNumberCalculation
    {
        private double _constValue = 1.0;

        #region Properties
        [System.ComponentModel.DisplayName("Constant Value")]
        [System.ComponentModel.Description("Calculater always returns this value.")]
        public double Constant
        {
            get { return _constValue; }
            set { _constValue = value; }
        }
        #endregion

        #region ISimpleNumberCalculation Member
        [System.ComponentModel.Browsable(false)]
        public string Name
        {
            get { return "Constant Value"; }
        }

        [System.ComponentModel.Browsable(false)]
        public string Description
        {
            get { return "Returns a constant value"; }
        }

        public double Calculate(double val)
        {
            return _constValue;
        }

        #endregion

        #region IPersistable Member

        public void Load(gView.Framework.IO.IPersistStream stream)
        {
            _constValue = (double)stream.Load("constvalue", (double)1.0);
        }

        public void Save(gView.Framework.IO.IPersistStream stream)
        {
            stream.Save("constvalue", _constValue);
        }

        #endregion
    }

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
                return _zeroValue;

            return val;
        }

        #endregion

        #region IPersistable Member

        public void Load(gView.Framework.IO.IPersistStream stream)
        {
            _zeroValue = (double)stream.Load("zvr", (double)1.0);
        }

        public void Save(gView.Framework.IO.IPersistStream stream)
        {
            stream.Save("zvr", _zeroValue);
        }

        #endregion
    }
}
