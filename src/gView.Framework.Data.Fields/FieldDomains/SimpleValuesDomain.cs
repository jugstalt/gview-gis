using gView.Framework.Core.Data;
using gView.Framework.Core.IO;
using gView.Framework.Core.system;
using gView.Framework.Core.UI;
using gView.Framework.Common;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace gView.Framework.Data.Fields.FieldDomains
{
    [RegisterPlugIn("9EDDDC6F-74DC-41fb-84C4-124343880341")]
    public class SimpleValuesDomain : Cloner, IValuesFieldDomain, IPropertyPage
    {
        List<object> _values = new List<object>();

        #region IValuesFieldDomain Member

        public Task<object[]> ValuesAsync()
        {
            return Task.FromResult<object[]>(_values.ToArray());
        }
        public void SetValues(object[] value)
        {
            _values.Clear();
            if (value == null)
            {
                return;
            }

            foreach (object o in value)
            {
                _values.Add(o);
            }
        }

        #endregion

        #region IFieldDomain Member

        virtual public string Name
        {
            get { return "Simple Values"; }
        }

        #endregion

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            _values.Clear();
            object o;
            while ((o = stream.Load("Value", null)) != null)
            {
                _values.Add(o);
            }
        }

        public void Save(IPersistStream stream)
        {
            foreach (object o in _values)
            {
                if (o == null)
                {
                    continue;
                }

                stream.Save("Value", o);
            }
        }

        #endregion

        public object PropertyPage(object initObject)
        {
            string appPath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            Assembly uiAssembly = Assembly.LoadFrom(appPath + @"/gView.Win.Data.Fields.UI.dll");

            IInitializeClass p = uiAssembly.CreateInstance("gView.Framework.Data.Fields.UI.FieldDomains.Control_SimpleValueDomain") as IInitializeClass;
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
    }
}
