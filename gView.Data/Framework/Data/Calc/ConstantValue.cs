using gView.Framework.system;

namespace gView.Framework.Data.Calc
{
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
}
