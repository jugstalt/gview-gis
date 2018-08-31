using System;
using System.Xml;
using System.Text;
using gView.Framework.UI;
using System.Reflection;
using System.Collections.Generic;

namespace gView.Framework.system
{
    /// <summary>
    /// 
    /// </summary>
    internal class Globals
    {
        public Globals()
        {

        }
        static public string getHotlinkUrl(XmlNode feature, string url)
        {
            int pos1 = 0, pos2;
            pos1 = url.IndexOf("[");
            while (pos1 != -1)
            {
                pos2 = url.IndexOf("]", pos1);
                if (pos2 == -1) break;
                string field = url.Substring(pos1 + 1, pos2 - pos1 - 1);
                url = url.Replace("[" + field + "]", getFieldValue(feature, field).Replace(" ", "%20"));
                pos1 = url.IndexOf("[");
            }
            return url;
        }

        static public string getFieldValue(XmlNode feature, string name)
        {
            XmlNodeList fields = feature.SelectNodes("FIELDS/FIELD");
            name = name.ToUpper();
            string shortname = shortName(name);
            foreach (XmlNode field in fields)
            {
                string fieldname = field.Attributes["name"].Value.ToString().ToUpper();
                int index = fieldname.IndexOf("." + name);
                int pos = fieldname.Length - name.Length - 1;
                if (fieldname == name ||
                    (index == -1 && fieldname == shortname) ||
                    (index != -1 && index == pos))
                {
                    string val = field.Attributes["value"].Value.ToString();
                    return val;
                }
            }
            return "";
        }

        static public string shortName(string fieldname)
        {
            int pos = 0;
            string[] fieldnames = fieldname.Split(';');
            fieldname = "";
            for (int i = 0; i < fieldnames.Length; i++)
            {
                while ((pos = fieldnames[i].IndexOf(".")) != -1)
                {
                    fieldnames[i] = fieldnames[i].Substring(pos + 1, fieldnames[i].Length - pos - 1);
                }
                if (fieldname != "") fieldname += ";";
                fieldname += fieldnames[i];
            }

            return fieldname;
        }

        static public string add2Filename(string filename, string add)
        {
            int pos = -1;
            while (filename.IndexOf(".", pos + 1) != -1)
            {
                pos = filename.IndexOf(".", pos + 1);
            }
            if (pos == -1) return filename + add;
            if (pos == 0) return add + filename;
            return filename.Substring(0, pos) + add + filename.Substring(pos, filename.Length - pos);
        }

        static public bool IsNumber(string val)
        {
            try
            {
                foreach (string val_ in val.Split(';'))
                {
                    string v = val_;
                    double test = Convert.ToDouble(v.Replace(".", ","));
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
        static public bool isHNR(string val)
        {
            if (val.Length == 0) return false;
            if (IsNumber(val)) return true;
            if (!IsNumber(val.Substring(0, val.Length - 1))) return false;
            return true;
        }
        static public void splitHNR(string hnr, out string hnr1, out string hnr2)
        {
            if (!IsNumber(hnr))
            {
                hnr1 = hnr.Substring(0, hnr.Length - 1);
                hnr2 = hnr.Substring(hnr.Length - 1, 1);
            }
            else
            {
                hnr1 = hnr;
                hnr2 = "";
            }
        }
        static public int hex2int(string val)
        {
            int ret = 0;
            for (int i = val.Length - 1, p = 0; i >= 0; i--, p++)
            {
                int z = 0;
                val = val.ToLower();
                switch (val[i])
                {
                    case '1': z = 1; break;
                    case '2': z = 2; break;
                    case '3': z = 3; break;
                    case '4': z = 4; break;
                    case '5': z = 5; break;
                    case '6': z = 6; break;
                    case '7': z = 7; break;
                    case '8': z = 8; break;
                    case '9': z = 9; break;
                    case 'a': z = 10; break;
                    case 'b': z = 11; break;
                    case 'c': z = 12; break;
                    case 'd': z = 13; break;
                    case 'e': z = 14; break;
                    case 'f': z = 15; break;
                }
                ret += z * (int)Math.Pow(16, p);
            }
            return ret;
        }
        static public string hex2RGBString(string val)
        {
            try
            {
                int r = hex2int(val.Substring(0, 2)),
                    g = hex2int(val.Substring(2, 2)),
                    b = hex2int(val.Substring(4, 2));

                return r.ToString() + "," + g.ToString() + "," + b.ToString();
            }
            catch
            {
                return "255,0,0";
            }
        }
        static public string Umlaute2Esri(string val)
        {
            val = val.Replace("�", "&#228;");
            val = val.Replace("�", "&#246;");
            val = val.Replace("�", "&#252;");
            val = val.Replace("�", "&#196;");
            val = val.Replace("�", "&#214;");
            val = val.Replace("�", "&#220;");
            val = val.Replace("�", "&#223;");

            return val;
        }
        static public XmlNode getField(XmlNode feature, string name)
        {
            XmlNodeList fields = feature.SelectNodes("FIELDS/FIELD");
            name = name.ToUpper();
            foreach (XmlNode field in fields)
            {
                string fieldname = field.Attributes["name"].Value.ToString().ToUpper();
                int index = fieldname.IndexOf("." + name);
                int pos = fieldname.Length - name.Length - 1;
                if (fieldname == name ||
                    (index != -1 && index == pos))
                {
                    return field;
                }
            }
            return null;
        }
    }
}
