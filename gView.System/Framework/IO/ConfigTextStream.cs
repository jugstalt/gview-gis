using gView.Framework.system;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace gView.Framework.IO
{
    public class ConfigTextStream
    {
        //private static string _configPath=System.Environment.CurrentDirectory+@"/config";
        private string _path;
        private global::System.IO.StreamReader tr = null;
        private global::System.IO.StreamWriter tw = null;
        private List<string> _ids = new List<string>();
        private string _dp = "&chr(" + ((byte)':').ToString() + ");";

        public ConfigTextStream(string name)
        {
            if (name == "")
            {
                return;
            }

            _path = SystemVariables.MyApplicationConfigPath + @"/" + name + ".config";
            GetIDs();
            try
            {
                FileInfo fi = new FileInfo(_path);
                if (fi.Exists)
                {
                    tr = new global::System.IO.StreamReader(_path);
                }
            }
            catch
            {
                Close();
            }
        }
        public ConfigTextStream(string name, bool write, bool append)
        {
            try
            {
                _path = SystemVariables.MyApplicationConfigPath + @"/" + name + ".config";
                GetIDs();
                if (!write)
                {
                    FileInfo fi = new FileInfo(_path);
                    if (fi.Exists)
                    {
                        tr = new global::System.IO.StreamReader(_path);
                    }
                }
                else
                {
                    tw = new global::System.IO.StreamWriter(_path, append);
                }
            }
            catch/*(Exception ex)*/
            {
                Close();
            }
        }
        private void GetIDs()
        {
            FileInfo fi = new FileInfo(_path);
            if (!fi.Exists)
            {
                return;
            }

            StreamReader sr = new StreamReader(_path);
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                string[] id_ = line.Split(':');
                _ids.Add(id_[0]);
            }
            sr.Close();
        }
        public void Close()
        {
            if (tr != null)
            {
                tr.Close();
            }

            if (tw != null)
            {
                tw.Close();
            }

            tr = null;
            tw = null;
        }

        private string ModifyID(string id)
        {
            int i = 1;
            string id_ = id;
            while (_ids.Contains(id))
            {
                id = id_ + "(" + i.ToString() + ")";
                i++;
            }
            return id;
        }

        public bool Write(string line, ref string id)
        {
            id = id.Replace(":", _dp);

            if (tw == null)
            {
                return false;
            }

            id = ModifyID(id);
            tw.WriteLine(id + ":" + line);
            return true;
        }

        public bool Remove(string id, string line)
        {
            id = id.Replace(":", _dp);

            if (tw == null)
            {
                return false;
            }

            try
            {
                FileInfo fi = new FileInfo(_path);
                fi.CopyTo(_path + ".tmp");

                tw.Close();
                tw = new StreamWriter(_path, false);

                StreamReader sr = new StreamReader(_path + ".tmp");
                string l, i = "";
                while ((l = sr.ReadLine()) != null)
                {
                    int pos = l.IndexOf(":");
                    if (pos != -1)
                    {
                        i = l.Substring(0, pos);
                        l = l.Substring(pos + 1, l.Length - pos - 1);
                    }
                    if (l == line && i == id)
                    {
                        continue;
                    }

                    tw.WriteLine(i + ":" + l);
                }
                sr.Close();
                tw.Close();
                fi = new FileInfo(_path + ".tmp");
                fi.Delete();

                tw = new StreamWriter(_path, true);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Replace(string line, string to)
        {
            if (tw == null)
            {
                return false;
            }

            try
            {
                FileInfo fi = new FileInfo(_path);
                fi.CopyTo(_path + ".tmp");

                tw.Close();
                tw = new StreamWriter(_path, false);

                StreamReader sr = new StreamReader(_path + ".tmp");
                string l, id = "";
                while ((l = sr.ReadLine()) != null)
                {
                    int pos = l.IndexOf(":");
                    if (pos != -1)
                    {
                        id = l.Substring(0, pos);
                        l = l.Substring(pos + 1, l.Length - pos - 1);
                    }
                    if (l == line)
                    {
                        tw.WriteLine(id + ":" + to);
                    }
                    else
                    {
                        tw.WriteLine(id + ":" + l);
                    }
                }
                sr.Close();
                tw.Close();
                fi = new FileInfo(_path + ".tmp");
                fi.Delete();

                tw = new StreamWriter(_path, true);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool ReplaceHoleLine(string line, string to)
        {
            if (tw == null)
            {
                return false;
            }

            try
            {
                bool ret = false;
                FileInfo fi = new FileInfo(_path);
                fi.CopyTo(_path + ".tmp");

                tw.Close();
                tw = new StreamWriter(_path, false);

                StreamReader sr = new StreamReader(_path + ".tmp");
                string l;
                while ((l = sr.ReadLine()) != null)
                {
                    if (l == line)
                    {
                        tw.WriteLine(to);
                        ret = true;
                    }
                    else
                    {
                        tw.WriteLine(l);
                    }
                }
                sr.Close();
                tw.Close();
                fi = new FileInfo(_path + ".tmp");
                fi.Delete();

                tw = new StreamWriter(_path, true);
                return ret;
            }
            catch
            {
                return false;
            }
        }

        public string Read(out string id)
        {
            id = "";
            if (tr == null)
            {
                return null;
            }

            string l = tr.ReadLine();
            if (l == null)
            {
                return null;
            }

            int pos = l.IndexOf(":");
            if (pos != -1)
            {
                id = l.Substring(0, pos).Replace(_dp, ":");
                l = l.Substring(pos + 1, l.Length - pos - 1);
            }
            return l;
        }

        #region Static Members

        public static string ExtractValue(string Params, string Param)
        {
            Param = Param.Trim();

            foreach (string a in Params.Split(';'))
            {
                string aa = a.Trim();
                if (aa.ToLower().IndexOf(Param.ToLower() + "=") == 0)
                {
                    if (aa.Length == Param.Length + 1)
                    {
                        return "";
                    }

                    return aa.Substring(Param.Length + 1, aa.Length - Param.Length - 1);
                }
            }
            return String.Empty;
        }

        public static string ExtractOracleValue(string @params, string param)
        {
            @params = @params.Trim();
            int pos = @params.ToLower().IndexOf("(" + param.ToLower() + "=");
            if (pos >= 0)
            {
                int pos2 = @params.IndexOf(")", pos + param.Length + 1);
                if (pos2 > pos)
                {
                    return @params.Substring(pos + param.Length + 2, pos2 - pos - param.Length - 2);
                }
            }
            return String.Empty;
        }

        public static string RemoveValue(string Params, string Param)
        {
            Param = Param.ToLower().Trim();

            StringBuilder sb = new StringBuilder();
            foreach (string a in Params.Split(';'))
            {
                if (String.IsNullOrEmpty(a.Trim()) ||
                    a.ToLower().StartsWith(Param + "="))
                {
                    continue;
                }

                if (sb.Length > 0)
                {
                    sb.Append(";");
                }

                sb.Append(a);
            }
            return sb.ToString();
        }

        public static string BuildLine(string id, string config)
        {
            return id + ":" + config;
        }

        public static Dictionary<string, string> Extract(string Params)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            foreach (string keyValue in Params.Split(';')
                                              .Select(p => p.Trim())
                                              .Where(p => !String.IsNullOrEmpty(p)))
            {
                int pos = keyValue.IndexOf("=");
                if (pos == -1)
                {
                    parameters.Add(keyValue, null);
                }
                else
                {
                    parameters.Add(
                        keyValue.Substring(0, pos),
                        keyValue.Substring(pos + 1, keyValue.Length - pos - 1));
                }
            }
            return parameters;
        }

        public static string Build(IDictionary<string, string> keyValues)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var key in keyValues.Keys)
            {
                if (sb.Length > 0)
                {
                    sb.Append(";");
                }

                sb.Append($"{key}={keyValues[key]}");
            }

            return sb.ToString();
        }

        public static string SecureConfig(string Params)
        {
            if (!Params.Contains("="))
            {
                return Params;
            }

            List<string> secure1 = new List<string>()/* { "user", "username", "userid", "usr", "usrid", "uid" }*/;
            List<string> secure2 = new List<string>() { "password", "pwd", "passwd", "passwrd", "pw" };
            Dictionary<string, string> parameters = Extract(Params);

            StringBuilder sb = new StringBuilder();
            foreach (string key in parameters.Keys)
            {
                if (key == String.Empty)
                {
                    continue;
                }

                if (sb.Length > 0)
                {
                    sb.Append(";");
                }

                if (secure2.Contains(key.Trim().ToLower()) ||
                    secure2.Contains(key.Trim().ToLower().Replace(" ", "")))
                {
                    sb.Append(key + "=********");
                }
                else if (secure1.Contains(key.Trim().ToLower()) ||
                    secure1.Contains(key.Trim().ToLower().Replace(" ", "")))
                {
                    string v = parameters[key], val = v;
                    if (v.Length > 2)
                    {
                        val = v[0].ToString();
                        for (int i = 1; i < v.Length - 2; i++)
                        {
                            val += "*";
                        }

                        val += v[v.Length - 1];
                    }
                    sb.Append(key + "=" + val);
                }
                else
                {
                    sb.Append(key + "=" + parameters[key]);
                }
            }

            return sb.ToString();
        }

        public static string SecureConfigValue(string paraneterName, string parameterValue)
        {
            List<string> secure = new List<string>() { "password", "pwd", "passwd", "passwrd", "pw" };

            if (secure.Contains(paraneterName?.ToLower()))
            {
                return "*************";
            }

            return parameterValue;
        }

        #endregion
    }
}
