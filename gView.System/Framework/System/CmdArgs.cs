using System;
using System.IO;
using System.Text;

namespace gView.Framework.system
{
    public class CmdArgs
    {
        static public void ReplaceUmlaute(string[] args)
        {
            StreamWriter sr = new StreamWriter(@"C:\args.xxx");

            if (args == null)
            {
                return;
            }

            for (int i = 0; i < args.Length; i++)
            {
                StringBuilder arg = new StringBuilder();
                for (int t = 0; t < args[i].Length; t++)
                {
                    switch ((int)args[i][t])
                    {
                        case 245:
                            arg.Append('ä');
                            break;
                        case 247:
                            arg.Append('ö');
                            break;
                        case 179:
                            arg.Append('ü');
                            break;
                        case 9472:
                            arg.Append('Ä');
                            break;
                        case 205:
                            arg.Append('Ö');
                            break;
                        case 9604:
                            arg.Append('Ü');
                            break;
                        case 9600:
                            arg.Append('ß');
                            break;
                        default:
                            arg.Append(args[i][t]);
                            break;
                    }
                }
                byte[] bytes = global::System.Text.ASCIIEncoding.Convert(
                    Encoding.Unicode,
                    Encoding.UTF8,
                    UnicodeEncoding.Unicode.GetBytes(args[i]));

                for (int b = 0; b < bytes.Length; b++)
                {
                    sr.Write((char)bytes[b] + ":" + bytes[b].ToString() + " ");
                }
                sr.WriteLine();
                //args[i] = System.Text.ASCIIEncoding.UTF8.GetString(bytes);

                sr.WriteLine(args[i]);

                Console.WriteLine(args[i]);
            }
            sr.Close();
        }
    }
}
