using gView.Framework.Core.Data;
using gView.Framework.Core.FDB;
using gView.Framework.Core.system;
using gView.Framework.Data;
using System.IO;
using System.Threading.Tasks;

namespace gView.Data.Fields.Framework.Data.Fields.AutoFields
{
    [RegisterPlugIn("683204A1-E77B-4CF9-A68D-831CADDFF1AB")]
    class FileTitleFromPath : Field, IAutoField
    {
        public FileTitleFromPath()
        {
            this.type = AutoFieldType;
            this.size = 512;
        }

        #region IAutoField

        public string AutoFieldName => "Filetitle from Path";

        public string AutoFieldDescription => "If feature contains a PATH field with a filename, only the file title without extension is stored";

        public string AutoFieldPrimayName => "File_Title";

        public FieldType AutoFieldType => FieldType.String;

        public Task<bool> OnInsert(IFeatureClass fc, IFeature feature)
        {
            return OnUpdate(fc, feature);
        }

        public Task<bool> OnUpdate(IFeatureClass fc, IFeature feature)
        {
            var fieldValue = feature?.FindField("PATH");

            if (fieldValue != null)
            {
                string value = fieldValue.Value?.ToString();

                try
                {
                    var fi = new FileInfo(value);
                    value = fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length);
                }
                catch { value = string.Empty; }

                if (feature.FindField(this.name) != null)
                {
                    feature[this.name] = value;
                }
                else
                {
                    feature.Fields.Add(new FieldValue(this.name, value));
                }
            }

            return Task.FromResult(true);
        }

        #endregion
    }
}
