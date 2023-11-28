using gView.Framework.Core.IO;
using gView.Framework.Core.system;
using gView.Framework.system;
using System;
using System.Data;

namespace gView.Framework.Data.Calc
{
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

        public void Load(IPersistStream stream)
        {
            _function = (string)stream.Load("func", "x");
        }

        public void Save(IPersistStream stream)
        {
            stream.Save("func", _function);
        }

        #endregion
    }
}
