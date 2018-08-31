using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using RTools.Util;

namespace Proj4Net.IO
{
    public class Proj4FileReader
    {
        private static readonly Assembly _asm = Assembly.GetExecutingAssembly();
        
        private static MemoryStream GetResource(string name)
        {
            var mrs = "Proj4Net.Resources." + name;
            var s = _asm.GetManifestResourceStream(mrs);
            if (s == null)
                return new MemoryStream();
            using (var sr = new BinaryReader(s))
            {
                return new MemoryStream(sr.ReadBytes((int) s.Length));
            }
        }
        
        public String[] ReadParametersFromFile(String file, String name)
        {
            // TODO: read comment preceding CS string as CS description
            // TODO: use simpler parser than StreamTokenizer for speed and flexibility
            // TODO: parse CSes line-at-a-time (this allows preserving CS param string for later access)

            using (var stream = GetResource(file.ToLower()))
            /*
            switch (file.ToLower())
            {
                case "epsg":
                    stream = GetResource("epsg");
                    break;
                case "esri":
                    stream = GetResource("esri");
                    break;
                case "nad27":
                    stream = new MemoryStream(Properties.Resources.nad27);
                    break;
                case "nad83":
                    stream = new MemoryStream(Properties.Resources.nad83);
                    break;
                case "world":
                    stream = new MemoryStream(Properties.Resources.world);
                    break;
                default:
                    throw new Exception();
            }*/
            using (var sr = new StreamReader(stream))
            {
                return ReadFile(sr, name);
            }

        }

        private static StreamTokenizer CreateTokenizer(TextReader reader)
        {
            var t = new StreamTokenizer(reader);
            t.Settings.CommentChar('#');
            t.Settings.OrdinaryChars('0', '9');
            t.Settings.OrdinaryChars('.', '.');
            t.Settings.OrdinaryChars('-', '-');
            t.Settings.OrdinaryChars('+', '+');
            t.Settings.WordChars('0', '9');
            t.Settings.WordChars('\'', '\'');
            t.Settings.WordChars('"', '"');
            t.Settings.WordChars('_', '_');
            t.Settings.WordChars('.', '.');
            t.Settings.WordChars('-', '-');
            t.Settings.WordChars('+', '+');
            t.Settings.WordChars(',', ',');
            t.Settings.WordChars('@', '@');
            t.Settings.GrabComments = true;

            return t;
        }

        private static String[] ReadFile(TextReader reader, String name)
        {
            var t = CreateTokenizer(reader);

            Token nt;
            var comment = "";
            while (t.NextToken(out nt))
            {
                while (nt is CommentToken)
                {
                    comment += "\n" + ((nt is CommentToken)
                                           ? (nt.StringValue.Length > 1) ? nt.StringValue.Substring(2) : ""
                                           : "");
                    if (!t.NextToken(out nt)) break;
                }
                if (comment.Length > 1)
                    comment = comment.Substring(1);

                //              if (!t.NextToken(out nt)) 
                //                  break;

                if (nt.Equals('<'))
                {
                    t.NextToken(out nt);
                    if (!(nt is WordToken))
                        throw new IOException(nt.LineNumber + ": Word expected after '<'");
                    var crsName = nt.StringValue;
                    t.NextToken(out nt);
                    if (!nt.Equals('>'))
                        throw new IOException(nt.LineNumber + ": '>' expected");
                    t.NextToken(out nt);
                    List<String> v = new List<String>();

                    while (!nt.Equals('<'))
                    {
                        if (nt.Equals('+'))
                            t.NextToken(out nt);
                        if (!(nt is WordToken))
                            throw new IOException(nt.LineNumber + ": Word expected after '+'");
                        String key = nt.StringValue;
                        t.NextToken(out nt);


                        // parse =arg, if any
                        if (nt.Equals('='))
                        {
                            t.NextToken(out nt);
                            //Removed check to allow for proj4 hack +nadgrids=@null 
                            //if ( t.ttype != StreamTokenizer.TT_WORD ) 
                            //  throw new IOException( t.lineno()+": Value expected after '='" );
                            String value = nt.StringValue;
                            t.NextToken(out nt);
                            AddParam(v, key, value);
                        }
                        else
                        {
                            // add param with no value
                            AddParam(v, key, null);
                        }
                    }
                    t.NextToken(out nt);
                    if (!nt.Equals('>'))
                        throw new IOException(nt.LineNumber + ": '<>' expected");
                    //t.NextToken(out nt);

                    // found requested CRS?
                    if (crsName.Equals(name))
                    {
                        return v.ToArray();
                    }
                    comment = "";
                }
            }

            return null;
        }

        private static void AddParam(ICollection<String> v, String key, String value)
        {
            String plusKey = key;
            if (!key.StartsWith("+"))
                plusKey = "+" + key;

            if (value != null)
                v.Add(plusKey + "=" + value);
            else
                v.Add(plusKey);

        }

        /// <summary>
        /// Gets the list of PROJ.4 parameters which define
        /// the coordinate system specified by <tt>name</tt>.
        /// </summary>
        /// <param name="crsName">The name of the coordinate system</param>
        /// <returns>The PROJ.4 projection parameters which define the coordinate system</returns>
        public String[] GetParameters(String crsName)
        {
            try
            {
                var p = crsName.IndexOf(':');
                if (p >= 0)
                {
                    var auth = crsName.Substring(0, p);
                    var id = crsName.Substring(p + 1);
                    return ReadParametersFromFile(auth, id);
                }
            }
            catch (IOException e)
            {
#if !SILVERLIGHT
                Trace.WriteLine(e.StackTrace);
#endif
            }
            return null;
        }

    }
}
