using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.IO;
using gView.Framework.UI;
using System.Reflection;
using gView.Framework.system;

namespace gView.Framework.Metadata
{
    [gView.Framework.system.RegisterPlugIn("32C9CDB8-5A2D-4fc1-87AD-2AE960D1E31A")]
    public class GeneralMetadata : IMetadataProvider, IPropertyPage
    {
        private string _abstract = "", _purpose = "", _language = "", _subl_info = "";
        private string _access_constraints = "", _use_constraints = "";
        private string _credits = "", _contact = "";

        public GeneralMetadata()
        {
            _abstract = String.Empty;
        }

        #region Properties
        public string Abstract
        {
            get { return _abstract; }
            set { _abstract = value; }
        }
        public string Purpose
        {
            get { return _purpose; }
            set { _purpose = value; }
        }
        public string Language
        {
            get { return _language; }
            set { _language = value; }
        }
        public string Supplemental_Information
        {
            get { return _subl_info; }
            set { _subl_info = value; }
        }
        public string Access_Constraints
        {
            get { return _access_constraints; }
            set { _access_constraints = value; }
        }
        public string Use_Constraints
        {
            get { return _use_constraints; }
            set { _use_constraints = value; }
        }
        public string Contact
        {
            get { return _contact; }
            set { _contact = value; }
        }
        public string Credits
        {
            get { return _credits; }
            set { _credits = value; }
        }
        #endregion

        #region IMetadataProvider Member

        public bool ApplyTo(object Object)
        {
            return true;
        }

        public string Name
        {
            get { return "General"; }
        }

        #endregion

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            _abstract = (string)stream.Load("Abstract", String.Empty);
            _purpose = (string)stream.Load("Purpose", String.Empty);
            _language = (string)stream.Load("Language", String.Empty);
            _subl_info = (string)stream.Load("Supplemental_Information", String.Empty);
            _access_constraints = (string)stream.Load("Access_Constraints", String.Empty);
            _use_constraints = (string)stream.Load("Use_Constraints", String.Empty);
            _contact = (string)stream.Load("Contact", String.Empty);
            _credits = (string)stream.Load("Credits", String.Empty);
        }

        public void Save(IPersistStream stream)
        {
            stream.Save("Abstract", _abstract);
            stream.Save("Purpose", _purpose);
            stream.Save("Language", _language);
            stream.Save("Supplemental_Information", _subl_info);
            stream.Save("Access_Constraints", _access_constraints);
            stream.Save("Use_Constraints", _use_constraints);
            stream.Save("Contact", _contact);
            stream.Save("Credits", _credits);
        }

        #endregion

        #region IPropertyPage Member

        public object PropertyPage(object initObject)
        {
            string appPath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            Assembly uiAssembly = Assembly.LoadFrom(appPath + @"\gView.Metadata.UI.dll");

            IPlugInParameter p = uiAssembly.CreateInstance("gView.Framework.Metadata.UI.GeneralMetadataControl") as IPlugInParameter;
            if (p != null)
                p.Parameter = this;

            return p;
        }

        public object PropertyPageObject()
        {
            return this;
        }

        #endregion
    }

    //[gView.Framework.system.RegisterPlugIn("2C1EA3BA-92E6-4B0D-B3C2-2802A1264683")]
    //public class AdvancedMetadata : IMetadataProvider
    //{
    //    static private List<Uri> _servers = new List<Uri>();

    //    #region IMetadataProvider Member

    //    public bool ApplyTo(object Object)
    //    {
    //        return true;
    //    }

    //    public string Name
    //    {
    //        get { return "Advanced"; }
    //    }

    //    #endregion

    //    #region IPersistable Member

    //    public void Load(IPersistStream stream)
    //    {
            
    //    }

    //    public void Save(IPersistStream stream)
    //    {
            
    //    }

    //    #endregion
    //}
}
