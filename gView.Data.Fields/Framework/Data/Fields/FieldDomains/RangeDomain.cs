using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Data;
using gView.Framework.UI;
using gView.Framework.system;
using System.Reflection;

namespace gView.Framework.Data.Fields.FieldDomains
{
    [gView.Framework.system.RegisterPlugIn("D2AA566F-362E-4ccc-93E8-4B37FF371104")]
    public class RangeDomain : Cloner, IRangeFieldDomain, IPropertyPage
    {
        double _minValue = (double)int.MinValue, _maxValue = (double)int.MaxValue;

        #region IRangeFieldDomain Member

        public double MinValue
        {
            get { return _minValue; }
            set { _minValue = value; }
        }

        public double MaxValue
        {
            get { return _maxValue; }
            set { _maxValue = value; }
        }

        #endregion

        #region IFieldDomain Member

        public string Name
        {
            get { return "Range"; }
        }

        #endregion

        #region IPersistable Member

        public void Load(gView.Framework.IO.IPersistStream stream)
        {
            _minValue = (double)stream.Load("minValue", double.MinValue);
            _maxValue = (double)stream.Load("maxValue", double.MaxValue);
        }

        public void Save(gView.Framework.IO.IPersistStream stream)
        {
            stream.Save("minValue", _minValue);
            stream.Save("maxValue", _maxValue);
        }

        #endregion

        #region IPropertyPage Member

        public object PropertyPage(object initObject)
        {
            string appPath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            Assembly uiAssembly = Assembly.LoadFrom(appPath + @"\gView.Data.Fields.UI.dll");

            IInitializeClass p = uiAssembly.CreateInstance("gView.Framework.Data.Fields.UI.FieldDomains.Control_RangeDomain") as IInitializeClass;
            if (p != null)
            {
                p.Initialize(this);
            }
            return p;
        }

        public object PropertyPageObject()
        {
            return this;
        }

        #endregion
    }
}
