using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Data;

namespace gView.Framework.Data
{
    public class FieldTranslation
    {
        private enum NameCase { ignore = 0, upper = 1, lower = 2 }
        private NameCase _case = NameCase.ignore;
        private Dictionary<IField, string> _fields;

        public FieldTranslation()
        {
            _fields = new Dictionary<IField, string>();
        }

        public void Add(IField field, string destinationName)
        {
            string dest;
            if (_fields.TryGetValue(field, out dest))
            {
                _fields[field] = destinationName;
            }
            else
            {
                _fields.Add(field, destinationName);
            }
        }

        public IFields DestinationFields
        {
            get
            {
                Fields destFields = new Fields();
                foreach (IField field in _fields.Keys)
                {
                    Field f = new Field(field);
                    f.name = _fields[field];
                    switch (_case)
                    {
                        case NameCase.lower:
                            f.name = f.name.ToLower();
                            break;
                        case NameCase.upper:
                            f.name = f.name.ToUpper();
                            break;
                    }
                    destFields.Add(f);
                }
                return destFields;
            }
        }
        public IFields SourceFields
        {
            get
            {
                Fields sourceFields = new Fields();
                foreach (IField field in _fields.Keys)
                {
                    Field f = new Field(field);
                    sourceFields.Add(f);
                }
                return sourceFields;
            }
        }

        public string this[IField field]
        {
            get
            {
                string destFieldname = "";
                _fields.TryGetValue(field, out destFieldname);
                return destFieldname;
            }
        }

        public void RenameFields(IFeature feature)
        {
            foreach (IFieldValue fv in feature.Fields)
            {
                if (!(fv is FieldValue)) continue;

                foreach (IField field in _fields.Keys)
                {
                    if (field.name == fv.Name)
                    {
                        string newName = _fields[field];
                        switch (_case)
                        {
                            case NameCase.lower:
                                newName = newName.ToLower();
                                break;
                            case NameCase.upper:
                                newName = newName.ToUpper();
                                break;
                        }
                        ((FieldValue)fv).Rename(newName);
                        break;
                    }
                }
            }
        }

        public void ToLower()
        {
            _case = NameCase.lower;
        }
        public void ToUpper()
        {
            _case = NameCase.upper;
        }
        public void ToDefaultCase()
        {
            _case = NameCase.ignore;
        }

        static public string CheckName(string fieldname)
        {
            if (fieldname.ToUpper() == "KEY") return "KEY_";
            if (fieldname.ToUpper() == "USER") return "USER_";
            if (fieldname.ToUpper() == "TEXT") return "TEXT_";

            return fieldname.Replace(" ", "");
        }

        static public string CheckNameLength(IFields fields, IField field, string fieldname, int maxLength)
        {
            return CheckNameLength(fields, field, fieldname, maxLength, 2);
        }

        static private string CheckNameLength(IFields fields, IField field, string fieldname, int maxLength, int counter)
        {
            foreach (IField f in fields.ToEnumerable())
            {
                if (f.Equals(field)) 
                    return fieldname.Substring(0, Math.Min(fieldname.Length, maxLength));

                if (f.name.Substring(0, Math.Min(f.name.Length, maxLength)).ToLower() ==
                    fieldname.Substring(0, Math.Min(fieldname.Length, maxLength)).ToLower())
                {
                    fieldname = fieldname.Substring(0, Math.Min(fieldname.Length, maxLength) - counter.ToString().Length) + counter.ToString();
                    return CheckNameLength(fields, field, fieldname, maxLength, counter + 1);
                }
            }
            return fieldname.Substring(0, Math.Min(fieldname.Length, maxLength));
        }
    }
}
