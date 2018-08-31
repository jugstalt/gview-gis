using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Data;
using gView.Framework.Data;

namespace gView.DataSources.Shape
{
    internal class DBFFileHader
    {
        public DBFFileHader(BinaryReader br)
        {
            version = br.ReadByte();
            YY = br.ReadByte();
            MM = br.ReadByte();
            DD = br.ReadByte();
            recordCount = (uint)br.ReadInt32();
            headerLength = br.ReadInt16();
            LegthOfEachRecord = br.ReadInt16();
            Reserved1 = br.ReadInt16();
            IncompleteTransac = br.ReadByte();
            EncryptionFlag = br.ReadByte();
            FreeRecordThread = (uint)br.ReadInt32();
            Reserved2 = br.ReadInt32();
            Reserved3 = br.ReadInt32();
            MDX = br.ReadByte();
            LanguageDriver = br.ReadByte();
            Reserved4 = br.ReadInt16();
        }

        public static bool Write(BinaryWriter bw, Fields fields)
        {
            if (bw == null || fields == null) return false;

            int c = 0, rl = 1;  // deleted Flag
            foreach (IField field in fields.ToEnumerable())
            {
                switch (field.type)
                {
                    case FieldType.biginteger:
                        c++; rl += 18;
                        break;
                    case FieldType.boolean:
                        c++; rl += 1;
                        break;
                    case FieldType.character:
                        c++; rl += 1;
                        break;
                    case FieldType.Date:
                        c++; rl += 8;
                        break;
                    case FieldType.Double:
                        c++; rl += 31;
                        break;
                    case FieldType.Float:
                        c++; rl += 11;
                        break;
                    case FieldType.ID:
                        c++; rl += 9;
                        break;
                    case FieldType.integer:
                        c++; rl += 9;
                        break;
                    case FieldType.Shape:
                        break;
                    case FieldType.smallinteger:
                        c++; rl += 6;
                        break;
                    case FieldType.String:
                        c++; rl += (field.size > 255) ? 255 : field.size;
                        break;
                    default:
                        c++; rl += (field.size <= 0) ? 255 : field.size;
                        break;
                }
            }

            short hLength = (short)(32 * c + 33);

            bw.Write((byte)3);   // Version
            bw.Write((byte)106); // YY
            bw.Write((byte)6);   // MM
            bw.Write((byte)12);  // DD
            bw.Write((int)0);    // recordCount
            bw.Write(hLength);    // headerLength
            bw.Write((short)rl); // Length of each record
            bw.Write((short)0);  // Reserved1
            bw.Write((byte)0);   // IncompleteTransac
            bw.Write((byte)0);   // EncryptionFlag
            bw.Write((int)0);    // FreeRecordThread
            bw.Write((int)0);    // Reserved2
            bw.Write((int)0);    // Reserved3
            bw.Write((byte)0);   // MDX
            bw.Write((byte)CodePage.DOS_Multilingual);   // LanguageDriver  Codepage 850
            bw.Write((short)0);  // Reserved4

            foreach (IField field in fields.ToEnumerable())
            {
                FieldDescriptor.Write(bw, field);
            }

            bw.Write((byte)13); // Terminator 0x0D
            return true;
        }

        public int FieldsCount
        {
            get
            {
                return (headerLength - 1) / 32 - 1;
            }
        }

        public byte version;
        public byte YY, MM, DD;
        public uint recordCount;
        public short headerLength;
        public short LegthOfEachRecord;
        public short Reserved1;
        public byte IncompleteTransac;
        public byte EncryptionFlag;
        public uint FreeRecordThread;
        public int Reserved2;
        public int Reserved3;
        public byte MDX;
        public byte LanguageDriver;
        public short Reserved4;
    }

    internal class FieldDescriptor
    {
        public FieldDescriptor(BinaryReader br)
        {
            br.Read(fieldName, 0, 11);
            FieldType = br.ReadChar();
            FieldDataAddress = (uint)br.ReadInt32();
            FieldLength = br.ReadByte();
            DecimalCount = br.ReadByte();
            Reserved1 = br.ReadInt16();
            WorkAreaID = br.ReadByte();
            Reserved2 = br.ReadInt16();
            FlagForSET_FIELDS = br.ReadByte();
            Reserved3 = br.ReadByte();
            Reserved4 = br.ReadByte();
            Reserved5 = br.ReadByte();
            Reserved6 = br.ReadByte();
            Reserved7 = br.ReadByte();
            Reserved8 = br.ReadByte();
            Reserved9 = br.ReadByte();
            IndexFieldFlag = br.ReadByte();
        }

        public static bool Write(BinaryWriter bw, IField field)
        {
            if (bw == null || field == null) return false;

            byte decimalCount = 0, fieldLength = 0;
            char fieldType = 'C';
            switch (field.type)
            {
                case gView.Framework.Data.FieldType.biginteger:
                    fieldLength = 18;
                    fieldType = 'N';
                    break;
                case gView.Framework.Data.FieldType.boolean:
                    fieldLength = 1;
                    fieldType = 'L';
                    break;
                case gView.Framework.Data.FieldType.character:
                    fieldLength = 1;
                    fieldType = 'C';
                    break;
                case gView.Framework.Data.FieldType.Date:
                    fieldLength = 8;
                    fieldType = 'D';
                    break;
                case gView.Framework.Data.FieldType.Double:
                    fieldLength = 31;
                    decimalCount = 31;
                    fieldType = 'F';
                    break;
                case gView.Framework.Data.FieldType.Float:
                    fieldLength = 11;
                    fieldType = 'F';
                    break;
                case gView.Framework.Data.FieldType.ID:
                    fieldLength = 9;
                    fieldType = 'N';
                    break;
                case gView.Framework.Data.FieldType.integer:
                    fieldLength = 9;
                    fieldType = 'N';
                    break;
                case gView.Framework.Data.FieldType.Shape:
                    return false;

                case gView.Framework.Data.FieldType.smallinteger:
                    fieldLength = 6;
                    fieldType = 'N';
                    break;
                case gView.Framework.Data.FieldType.String:
                    fieldLength = (byte)(field.size > 255 ? 255 : field.size);
                    fieldType = 'C';
                    break;
                default:
                    fieldLength = (byte)(field.size > 0 ? field.size : 255);
                    fieldType = 'C';
                    break;
            }

            // fieldName
            for (int i = 0; i < 10; i++)
            {
                if (i < field.name.Length)
                    bw.Write((byte)field.name[i]);
                else
                    bw.Write((byte)0);
            }
            bw.Write((byte)0);

            bw.Write((byte)fieldType);     // FieldType
            bw.Write((int)0);              // FieldDataAddress
            bw.Write((byte)fieldLength);   // FieldLength
            bw.Write((byte)decimalCount);  // DecimalCount
            bw.Write((short)0);            // Reserved1
            bw.Write((byte)0);             // WorkAreaID
            bw.Write((short)0);            // Reserved2
            bw.Write((byte)0);             // FlagForSET_FIELDS
            bw.Write((byte)0);             // Reserved3
            bw.Write((byte)0);             // Reserved4
            bw.Write((byte)0);             // Reserved5
            bw.Write((byte)0);             // Reserved6
            bw.Write((byte)0);             // Reserved7
            bw.Write((byte)0);             // Reserved8
            bw.Write((byte)0);             // Reserved9
            bw.Write((byte)0);             // IndexFieldFlag

            return true;
        }

        public string FieldName
        {
            get
            {
                char[] trims = { '\0' };
                System.Text.ASCIIEncoding encoder = new System.Text.ASCIIEncoding();
                return encoder.GetString(fieldName).TrimEnd(trims);
            }
        }
        private byte[] fieldName = new byte[11];
        public char FieldType;
        public uint FieldDataAddress;
        public byte FieldLength;
        public byte DecimalCount;
        public short Reserved1;
        public byte WorkAreaID;
        public short Reserved2;
        public byte FlagForSET_FIELDS;
        public byte Reserved3;
        public byte Reserved4;
        public byte Reserved5;
        public byte Reserved6;
        public byte Reserved7;
        public byte Reserved8;
        public byte Reserved9;
        public byte IndexFieldFlag;
    }

    internal enum CodePage
    {
        DOS_USA = 0x01,
        DOS_Multilingual = 0x02,
        Windows_ANSI = 0x03,
        EE_MS_DOS = 0x64,
        Nordic_MS_DOS = 0x65,
        Russian_MS_DOS = 0x66,
        Windows_EE = 0xc8,
        UTF_7 = 0x57
    }

    internal class DBFFile
    {
        private string _filename;
        private DBFFileHader _header;
        private List<FieldDescriptor> _fields;
        private Encoding _encoder = null;
        private char[] _trims = { '\0', ' ' };
        private static IFormatProvider _nhi = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;
        private string _idField = "FID";

        public DBFFile(string filename)
        {
            try
            {
                FileInfo fi = new FileInfo(filename);
                if (!fi.Exists) return;
                _filename = filename;

                StreamReader sr = new StreamReader(filename);
                BinaryReader br = new BinaryReader(sr.BaseStream);

                _header = new DBFFileHader(br);
                _fields = new List<FieldDescriptor>();
                for (int i = 0; i < _header.FieldsCount; i++)
                {
                    FieldDescriptor field = new FieldDescriptor(br);
                    _fields.Add(field);
                }
                sr.Close();

                int c = 1;
                string idFieldName = _idField;
                while (HasField(idFieldName))
                    idFieldName = _idField + "_" + c++;
                _idField = idFieldName;

                _encoder = null;
                try
                {
                    switch ((CodePage)_header.LanguageDriver)
                    {
                        case CodePage.DOS_USA:
                            _encoder = EncodingFromCodePage(437);
                            break;
                        case CodePage.DOS_Multilingual:
                            _encoder = EncodingFromCodePage(850);
                            break;
                        case CodePage.Windows_ANSI:
                            _encoder = EncodingFromCodePage(1252);
                            break;
                        case CodePage.EE_MS_DOS:
                            _encoder = EncodingFromCodePage(852);
                            break;
                        case CodePage.Nordic_MS_DOS:
                            _encoder = EncodingFromCodePage(865);
                            break;
                        case CodePage.Russian_MS_DOS:
                            _encoder = EncodingFromCodePage(866);
                            break;
                        case CodePage.Windows_EE:
                            _encoder = EncodingFromCodePage(1250);
                            break;
                        case CodePage.UTF_7:
                            _encoder = new UTF7Encoding();
                            break;
                    }
                }
                catch { }
                if (_encoder == null)
                {
                    FileInfo encFi = new FileInfo(fi.Directory.FullName + @"\dbf_default_encoding.txt");
                    if (encFi.Exists)
                    {
                        using (StreamReader encSr = new StreamReader(encFi.FullName))
                        {
                            switch (encSr.ReadLine().ToLower())
                            {
                                case "utf8":
                                case "utf-8":
                                    _encoder = new UTF8Encoding();
                                    break;
                                case "unicode":
                                    _encoder = new UnicodeEncoding();
                                    break;
                                case "ascii":
                                    _encoder = new ASCIIEncoding();
                                    break;
                            }

                        }
                    }

                    if (_encoder == null)
                        _encoder = new UTF7Encoding();
                }
                //Record(0);
                //Record(1);
            }
            catch
            {

            }
        }

        private Encoding EncodingFromCodePage(int codePage)
        {
            foreach (EncodingInfo ei in Encoding.GetEncodings())
            {
                if (ei.CodePage == codePage)
                {
                    return ei.GetEncoding();
                }
            }
            return null;
        }


        private bool HasField(string name)
        {
            foreach (FieldDescriptor fd in _fields)
            {
                if (fd.FieldName == name) return true;
            }
            return false;
        }
        public string Filename
        {
            get { return _filename; }
        }

        internal DataTable DataTable()
        {
            return DataTable(null);
        }

        internal DataTable DataTable(string[] fieldnames)
        {
            DataTable tab = new DataTable();

            if (fieldnames != null)
            {
                foreach (string fieldname in fieldnames)
                {
                    if (fieldname == _idField)
                    {
                        tab.Columns.Add(_idField, typeof(uint));
                    }
                    foreach (FieldDescriptor field in _fields)
                    {
                        if (field.FieldName == fieldname)
                        {
                            if (tab.Columns[fieldname] == null)
                                tab.Columns.Add(fieldname, DataType(field));
                        }
                    }
                }
            }
            else
            {
                tab.Columns.Add(_idField, typeof(uint));
                foreach (FieldDescriptor field in _fields)
                {
                    if (tab.Columns[field.FieldName] == null)
                        tab.Columns.Add(field.FieldName, DataType(field));
                }
            }

            return tab;
        }
        private Type DataType(FieldDescriptor fd)
        {
            switch (fd.FieldType)
            {
                case 'C': return typeof(string);
                case 'F':
                case 'N':
                    if (fd.DecimalCount == 0)
                    {
                        if (fd.FieldLength <= 6) return typeof(short);
                        if (fd.FieldLength <= 9) return typeof(int);
                        return typeof(long);
                    }
                    else // if( fd.DecimalCount==9 && fd.FieldLength==31 )
                    {
                        if (fd.DecimalCount <= 9) return typeof(float);
                        return typeof(double);
                    }
                case 'L': return typeof(bool);
                case 'D': return typeof(DateTime);
                case 'I': return typeof(int);
                case 'O': return typeof(double);
                case '+': return typeof(int); // Autoincrement
                default: return typeof(string);
            }
        }
        private FieldType FieldType(FieldDescriptor fd)
        {
            switch (fd.FieldType)
            {
                case 'C': return gView.Framework.Data.FieldType.String;
                case 'F':
                case 'N':
                    if (fd.DecimalCount == 0)
                    {
                        if (fd.FieldLength <= 6) return gView.Framework.Data.FieldType.smallinteger;
                        if (fd.FieldLength <= 9) return gView.Framework.Data.FieldType.integer;
                        return gView.Framework.Data.FieldType.biginteger;
                    }
                    else  // if( fd.DecimalCount==9 && fd.FieldLength==31 ) 
                    {
                        if (fd.DecimalCount <= 9) return gView.Framework.Data.FieldType.Float;
                        return gView.Framework.Data.FieldType.Double;
                    }
                case 'L': return gView.Framework.Data.FieldType.boolean;
                case 'D': return gView.Framework.Data.FieldType.Date;
                case 'I': return gView.Framework.Data.FieldType.integer;
                case 'O': return gView.Framework.Data.FieldType.Double;
                case '+': return gView.Framework.Data.FieldType.integer; // Autoincrement
                default: return gView.Framework.Data.FieldType.String;
            }
        }

        public DataTable Record(uint index)
        {
            return Record(index, "*");
        }

        public DataTable Record(uint index, string fieldnames)
        {
            StreamReader sr = new StreamReader(_filename);
            BinaryReader br = new BinaryReader(sr.BaseStream);

            string[] names = null;
            fieldnames = fieldnames.Replace(" ", "");
            if (fieldnames != "*") names = fieldnames.Split(',');

            DataTable tab = DataTable(names);
            Record(index, tab, br);

            sr.Close();
            return tab;
        }

        internal void Record(uint index, DataTable tab, BinaryReader br)
        {
            if (index > _header.recordCount || index < 1) return;

            br.BaseStream.Position = _header.headerLength + _header.LegthOfEachRecord * (index - 1);

            char deleted = br.ReadChar();
            if (deleted != ' ') return;

            DataRow row = tab.NewRow();
            foreach (FieldDescriptor field in _fields)
            {
                if (tab.Columns[field.FieldName] == null)
                {
                    br.BaseStream.Position += field.FieldLength;
                    continue;
                }

                switch ((char)field.FieldType)
                {
                    case 'C':
                        row[field.FieldName] = _encoder.GetString(br.ReadBytes(field.FieldLength)).TrimEnd(_trims);
                        break;
                    case 'F':
                    case 'N':
                        string str2 = _encoder.GetString(br.ReadBytes(field.FieldLength)).TrimEnd(_trims);
                        if (str2 != "")
                        {
                            try
                            {
                                if (field.DecimalCount == 0)
                                {
                                    row[field.FieldName] = Convert.ToInt64(str2);
                                }
                                else
                                {
                                    row[field.FieldName] = double.Parse(str2, _nhi);
                                }
                            }
                            catch { }
                        }
                        break;
                    case '+':
                    case 'I':
                        row[field.FieldName] = br.ReadInt32();
                        break;
                    case 'O':
                        row[field.FieldName] = br.ReadDouble();
                        break;
                    case 'L':
                        char c = br.ReadChar();
                        if (c == 'Y' || c == 'y' ||
                            c == 'T' || c == 't') row[field.FieldName] = true;
                        else if (c == 'N' || c == 'n' ||
                            c == 'F' || c == 'f') row[field.FieldName] = false;
                        else
                            row[field.FieldName] = null;
                        break;
                    case 'D':
                        string date = _encoder.GetString(br.ReadBytes(field.FieldLength)).TrimEnd(_trims);
                        if (date.Length == 8)
                        {
                            int y = int.Parse(date.Substring(0, 4));
                            int m = int.Parse(date.Substring(4, 2));
                            int d = int.Parse(date.Substring(6, 2));
                            DateTime td = new DateTime(y, m, d);
                            row[field.FieldName] = td;
                        }
                        break;
                }
            }
            if (tab.Columns[_idField] != null) row[_idField] = index;
            tab.Rows.Add(row);
        }

        internal void Records(DataTable tab, BinaryReader br)
        {
            uint rowCount = _header.recordCount;

            for (uint i = 0; i < rowCount; i++)
            {
                br.BaseStream.Position = _header.headerLength + _header.LegthOfEachRecord * (i);

                char deleted = br.ReadChar();
                if (deleted != ' ') continue;

                DataRow row = tab.NewRow();

                foreach (FieldDescriptor field in _fields)
                {
                    if (tab.Columns[field.FieldName] == null)
                    {
                        br.BaseStream.Position += field.FieldLength;
                        continue;
                    }

                    switch ((char)field.FieldType)
                    {
                        case 'C':
                            row[field.FieldName] = _encoder.GetString(br.ReadBytes(field.FieldLength)).TrimEnd(_trims);
                            break;
                        case 'F':
                        case 'N':
                            string str2 = _encoder.GetString(br.ReadBytes(field.FieldLength)).TrimEnd(_trims);
                            if (str2 != "")
                            {
                                try
                                {
                                    if (field.DecimalCount == 0)
                                    {
                                        row[field.FieldName] = long.Parse(str2);
                                    }
                                    else
                                    {
                                        row[field.FieldName] = double.Parse(str2, _nhi);
                                    }
                                }
                                catch { }
                            }
                            break;
                        case '+':
                        case 'I':
                            row[field.FieldName] = br.ReadInt32();
                            break;
                        case 'O':
                            row[field.FieldName] = br.ReadDouble();
                            break;
                        case 'L':
                            char c = br.ReadChar();
                            if (c == 'Y' || c == 'y' ||
                                c == 'T' || c == 't') row[field.FieldName] = true;
                            else if (c == 'N' || c == 'n' ||
                                c == 'F' || c == 'f') row[field.FieldName] = false;
                            else
                                row[field.FieldName] = null;
                            break;
                        case 'D':
                            string date = _encoder.GetString(br.ReadBytes(field.FieldLength)).TrimEnd(_trims);
                            if (date.Length == 8)
                            {
                                int y = int.Parse(date.Substring(0, 4));
                                int m = int.Parse(date.Substring(4, 2));
                                int d = int.Parse(date.Substring(6, 2));
                                DateTime td = new DateTime(y, m, d);
                                row[field.FieldName] = td;
                            }
                            break;
                    }
                }
                if (tab.Columns[_idField] != null) row[_idField] = i + 1;
                tab.Rows.Add(row);
            }
        }

        #region Writer

        public static bool Create(string filename, Fields fields)
        {
            try
            {
                FileInfo fi = new FileInfo(filename);
                if (fi.Exists) fi.Delete();

                StreamWriter sw = new StreamWriter(filename);
                BinaryWriter bw = new BinaryWriter(sw.BaseStream);

                bool ret = DBFFileHader.Write(bw, fields);

                bw.Flush();
                sw.Flush();
                sw.Close();

                return ret;
            }
            catch
            {
                return false;
            }
        }

        internal bool WriteRecord(uint index, IFeature feature)
        {
            if (feature == null) return false;

            FileStream fs = null;
            BinaryWriter bw = null;
            BinaryReader br = null;
            try
            {
                fs = new FileStream(_filename, FileMode.Open);
                bw = new BinaryWriter(fs);

                long pos0 = bw.BaseStream.Position = _header.headerLength + _header.LegthOfEachRecord * (index - 1);
                long posX = 1;

                bw.Write((byte)' ');  // deleted Flag

                string str;
                foreach (FieldDescriptor fd in _fields)
                {
                    object obj = feature[fd.FieldName];
                    if (obj == null || obj == DBNull.Value)
                    {
                        WriteNull(fd, bw);
                    }
                    else
                    {
                        try
                        {
                            switch (fd.FieldType)
                            {
                                case 'C':
                                    str = obj.ToString().PadRight(fd.FieldLength, ' ');
                                    WriteString(fd, bw, str);
                                    break;
                                case 'N':
                                case 'F':
                                    if (fd.DecimalCount == 0)
                                    {
                                        str = Convert.ToInt32(obj).ToString();
                                        str = str.PadLeft(fd.FieldLength, ' ');
                                        WriteString(fd, bw, str);
                                    }
                                    else
                                    {
                                        str = Convert.ToDouble(obj).ToString(_nhi);
                                        str = str.PadLeft(fd.FieldLength, ' ');
                                        WriteString(fd, bw, str);
                                    }
                                    break;
                                case '+':
                                case 'I':
                                    bw.Write(Convert.ToInt32(obj));
                                    break;
                                case 'O':
                                    bw.Write(Convert.ToDouble(obj));
                                    break;
                                case 'L':
                                    bool v = Convert.ToBoolean(obj);
                                    str = (v) ? "T" : "F";
                                    WriteString(fd, bw, str);
                                    break;
                                case 'D':
                                    DateTime td = Convert.ToDateTime(obj);
                                    str = td.Year.ToString().PadLeft(4, '0') +
                                          td.Month.ToString().PadLeft(2, '0') +
                                          td.Day.ToString().PadLeft(2, '0');
                                    WriteString(fd, bw, str);
                                    break;
                                default:
                                    WriteNull(fd, bw);
                                    break;
                            }
                        }
                        catch
                        {
                            WriteNull(fd, bw);
                        }
                    }
                    posX += fd.FieldLength;
                    bw.BaseStream.Position = pos0 + posX;
                }

                br = new BinaryReader(fs);
                br.BaseStream.Position = 4;
                uint recCount = (uint)br.ReadInt32();

                DateTime now = DateTime.Now;
                bw.BaseStream.Position = 1;
                bw.Write((byte)(now.Year - 1900));
                bw.Write((byte)now.Month);
                bw.Write((byte)now.Day);

                bw.Write((int)recCount + 1);

                fs.Flush();
                return true;
            }
            catch (Exception ex)
            {
                string err = ex.Message;
                return false;
            }
            finally
            {
                if (fs != null)
                    fs.Close();
            }
        }

        private void WriteNull(FieldDescriptor fd, BinaryWriter bw)
        {
            for (int i = 0; i < fd.FieldLength; i++)
            {
                bw.Write((byte)' ');
            }
        }
        private void WriteString(FieldDescriptor fd, BinaryWriter bw, string str)
        {
            byte[] bytes = _encoder.GetBytes(str);
            for (int i = 0; i < fd.FieldLength; i++)
            {
                if (i < bytes.Length)
                    bw.Write((byte)bytes[i]);
                else
                    bw.Write((byte)0);
            }
        }

        #endregion

        public IFields Fields
        {
            get
            {
                Fields fields = new Fields();

                // ID
                Field field = new Field();
                field.name = _idField;
                field.type = gView.Framework.Data.FieldType.ID;
                fields.Add(field);

                foreach (FieldDescriptor fd in _fields)
                {
                    field = new Field();
                    field.name = fd.FieldName;
                    field.size = fd.FieldLength;
                    field.precision = fd.DecimalCount;
                    field.type = FieldType(fd);

                    fields.Add(field);
                }

                return fields;
            }
        }


    }

    internal class DBFDataReader
    {
        private DBFFile _file;
        private StreamReader _sr = null;
        private BinaryReader _br = null;
        private DataTable _tab;

        public DBFDataReader(DBFFile file, string fieldnames)
        {
            if (file == null) return;
            _file = file;
            _sr = new StreamReader(_file.Filename);
            _br = new BinaryReader(_sr.BaseStream);

            string[] names = null;
            fieldnames = fieldnames.Replace(" ", "");
            if (fieldnames != "*") names = fieldnames.Split(',');

            _tab = _file.DataTable(names);
        }

        public DataTable AllRecords
        {
            get
            {
                if (_file == null) return null;

                _file.Records(_tab, _br);
                return _tab;
            }
        }

        public void AddRecord(uint index)
        {
            _file.Record(index, _tab, _br);
        }
        public void Clear()
        {
            _tab.Rows.Clear();
        }
        public DataTable Table
        {
            get { return _tab; }
        }
        public void Dispose()
        {
            if (_tab != null)
            {
                _tab.Rows.Clear();
                _tab.Dispose();
                _tab = null;
            }
            if (_sr != null)
            {
                _sr.Close();
                _sr = null;
            }
        }
    }

    internal class DBFDataWriter
    {
        public DBFDataWriter()
        {
        }
    }
}
