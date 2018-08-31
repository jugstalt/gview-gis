using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Data;
using System.IO;
using System.Xml;
using gView.Framework.system;

namespace gView.Framework.Data
{
    internal class Xml
    {
        #region Static Members
        static public string ToXml(ITableClass fc, IQueryFilter filter)
        {
            if (filter is IBufferQueryFilter)
            {
                filter = BufferQueryFilter.ConvertToSpatialFilter((IBufferQueryFilter)filter);
            }
            MemoryStream ms = new MemoryStream();
            StreamWriter sw = new StreamWriter(ms, Encoding.UTF8);

            sw.WriteLine("<Filter>");
            if (filter is IRowIDFilter)
            {
                if (fc.IDFieldName == String.Empty)
                {
                    CreateRowIDFilter(fc, filter as IRowIDFilter, sw);
                }
                else
                {
                    ParseInWhereClause(((IRowIDFilter)filter).RowIDWhereClause, sw);
                }
            }
            else if (filter is IQueryFilter)
            {
                if (filter.WhereClause.ToLower().Contains(" in ("))
                    ParseInWhereClause(filter.WhereClause, sw);
                else
                {
                    ParseWhereClause(filter.WhereClause, sw);
                }
            }
            sw.WriteLine("</Filter>");

            sw.Flush();

            ms.Position = 0;
            byte[] bytes = new byte[ms.Length];
            ms.Read(bytes, 0, (int)ms.Length);
            sw.Close();

            string ret = Encoding.UTF8.GetString(bytes).Trim();

            //            ret = @"<ogc:Filter>
            //   <ogc:PropertyIsLike wildCard=""%"" singleChar=""?"" escape=""ESC"">
            //      <ogc:PropertyName>
            //         item_plz
            //      </ogc:PropertyName>
            //      <ogc:Literal>
            //         53111
            //      </ogc:Literal>
            //   </ogc:PropertyIsLike>
            //</ogc:Filter>";

            return ret;
        }

        static public IQueryFilter FromXml(XmlNode filter)
        {
            return FromXml(null, filter);
        }
        static public IQueryFilter FromXml(ITableClass fc, XmlNode filter/*, string nsName*/)
        {
            XmlNamespaceManager ns = null;
            if (filter.NamespaceURI != String.Empty)
            {
                ns = new XmlNamespaceManager(filter.OwnerDocument.NameTable);
            }
            StringBuilder whereClause = new StringBuilder();
            IQueryFilter newFilter = null;
            ParseWFSFilter(fc, filter, "", ns, whereClause, ref newFilter);

            if (newFilter == null) newFilter = new QueryFilter();
            newFilter.WhereClause = RemoveOuterBrackets(whereClause.ToString());

            return newFilter;
        }

        #region Helper

        #region ToWFS
        private static void CreateRowIDFilter(ITableClass fc, IRowIDFilter filter, StreamWriter sw)
        {
            if (fc == null || filter == null || sw == null) return;

            string fcName = fc.Name;
            foreach (int id in filter.IDs)
            {
                //sw.WriteLine("<ogc:GmlObjectId gml:id=\"" + fcName + "." + id.ToString() + "\" />");
                sw.WriteLine("<FeatureId fid=\"" + fcName + "." + id.ToString() + "\" />");
            }
        }

        private static void ParseWhereClause(string whereClause, StreamWriter sw)
        {
            Tokenizer tokenizer = new Tokenizer(whereClause);
            tokenizer.OnTransformToken += new Tokenizer.TransformTokenEventHandler(tokenizer_OnTransformToken);

            sw.Write(tokenizer.TransformXML());
        }

        static void tokenizer_OnTransformToken(ref string text)
        {
            if (text == String.Empty) return;
            if (text.LastIndexOf("[") == 0 &&
                text.IndexOf("]") == text.Length - 1) return;

            text = ParseFlatWhereClause(text);
        }

        private static string ParseFlatWhereClause(string whereClause)
        {
            StringBuilder sb = new StringBuilder();
            if (whereClause == String.Empty) return String.Empty;

            if (whereClause.Contains("(") || whereClause.Contains(")"))
            {
                throw (new Exception("Whereclause to complex: don't use brackets"));
            }
            //Tokenizer tokenizer = new Tokenizer(whereClause);
            //Token rootToken = tokenizer.Token;

            bool useAND = false, useOR = false, useNOT = false;
            useAND = containsLogicalOperator(whereClause, "and");
            useOR = containsLogicalOperator(whereClause, "or");
            useNOT = containsLogicalOperator(whereClause, "not");

            if (useAND == true && useOR == true ||
                useAND == true && useNOT == true ||
                useOR == true && useNOT == true)
            {
                throw (new Exception("Whereclause to complex: Only use one kind of logical operations (and, or, not)"));
            }

            string op = String.Empty;
            if (useAND) op = "And";
            if (useOR) op = "Or";
            if (useNOT) op = "Not";

            string[] properties = SplitByString(whereClause, " " + op + " ");

            if (op != String.Empty)
                sb.Append("<" + op + ">");

            foreach (string property in properties)
            {
                if (property == String.Empty) continue;
                if (property[0] == '[' && property[property.Length - 1] == ']')
                {
                    sb.Append(property);
                    continue;
                }
                string[] parts = SplitPropertyClause(property);
                string operatorName = "";
                switch (parts[1].ToLower())
                {
                    case "like":
                        operatorName = "PropertyIsLike";
                        sb.Append(@"<PropertyIsLike wildCard=""%"" singleChar=""?"" escape=""ESC"">");
                        break;
                    case "=":
                        operatorName = "PropertyIsEqualTo";
                        sb.Append(@"<PropertyIsEqualTo>");
                        //operatorName = "ogc:PropertyIsLike";
                        //sb.Append(@"<ogc:PropertyIsLike wildCard=""%"" singleChar=""?"" escape=""ESC"">");
                        break;
                    case ">":
                        operatorName = "PropertyIsGreaterThan";
                        sb.Append(@"<PropertyIsGreaterThan>");
                        break;
                    case ">=":
                        operatorName = "PropertyIsGreaterThanOrEqualTo";
                        sb.Append(@"<PropertyIsGreaterThanOrEqualTo>");
                        break;
                    case "<":
                        operatorName = "PropertyIsLessThan";
                        sb.Append(@"<PropertyIsLessThan>");
                        break;
                    case "<=":
                        operatorName = "PropertyIsLessThanOrEqualTo";
                        sb.Append(@"<PropertyIsLessThanOrEqualTo>");
                        break;
                    case "<>":
                        operatorName = "PropertyIsNotEqualTo";
                        sb.Append(@"<PropertyIsNotEqualTo>");
                        break;
                }
                sb.Append("<PropertyName>" + parts[0] + "</PropertyName>");
                sb.Append("<Literal>" + parts[2].Replace("\"", "").Replace("'", "") + "</Literal>");
                sb.Append(@"</" + operatorName + ">");
            }
            if (op != String.Empty)
                sb.Append("</" + op + ">");

            return sb.ToString();
        }

        private static void ParseInWhereClause(string whereClause, StreamWriter sw)
        {
            while (whereClause.IndexOf(" ,") != -1)
                whereClause = whereClause.Replace(" ,", ",");
            while (whereClause.IndexOf(", ") != -1)
                whereClause = whereClause.Replace(", ", ",");

            string[] parts = whereClause.Trim().Split(' ');
            if (parts.Length != 3)
                throw (new Exception("Whereclause to complex..."));

            string field = parts[0];
            string[] ids = parts[2].Substring(1, parts[2].Length - 2).Split(',');

            StringBuilder sb = new StringBuilder();
            foreach (string id in ids)
            {
                if (sb.Length > 0) sb.Append(" OR ");
                sb.Append(field + "=" + id);
            }

            ParseWhereClause(sb.ToString(), sw);
        }
        private static bool containsLogicalOperator(string clause, string op)
        {
            clause = TrimClause(clause).ToLower();
            op = op.ToLower();

            string[] parts = clause.Split(' ');

            foreach (string part in parts)
            {
                if (part == op) return true;
            }
            return false;
        }

        private static string TrimClause(string clause)
        {
            clause = clause.Trim();
            while (clause.IndexOf("  ") != -1)
                clause = clause.Replace("  ", " ");

            return clause;
        }

        private static string[] SplitByString(string testString, string split)
        {
            if (split == string.Empty)
            {
                string[] ret = new string[1];
                ret[0] = testString;
                return ret;
            }
            split = split.ToLower();
            string testStringL = testString.ToLower();

            int offset = 0;
            int index = 0;
            int[] offsets = new int[testString.Length + 1];

            while (index < testStringL.Length)
            {
                int indexOf = testStringL.IndexOf(split, index);
                if (indexOf != -1)
                {
                    offsets[offset++] = indexOf;
                    index = (indexOf + split.Length);
                }
                else
                {
                    index = testStringL.Length;
                }
            }

            string[] final = new string[offset + 1];
            if (offset == 0)
            {
                final[0] = testString;
            }
            else
            {
                offset--;
                final[0] = testString.Substring(0, offsets[0]);
                for (int i = 0; i < offset; i++)
                {
                    final[i + 1] = testString.Substring(offsets[i] + split.Length, offsets[i + 1] - offsets[i] - split.Length);
                }
                final[offset + 1] = testString.Substring(offsets[offset] + split.Length);
            }
            return final;
        }

        private static string[] SplitPropertyClause(string clause)
        {
            clause = clause.Trim();

            string clause2 = clause.ToLower();

            if (clause2.Contains(">="))
            {
                return SplitPropertyClause(clause, ">=");
            }
            else if (clause2.Contains("<="))
            {
                return SplitPropertyClause(clause, "<=");
            }
            else if (clause2.Contains("<>"))
            {
                return SplitPropertyClause(clause, "<>");
            }
            else if (clause2.Contains(">"))
            {
                return SplitPropertyClause(clause, ">");
            }
            else if (clause2.Contains("<"))
            {
                return SplitPropertyClause(clause, "<");
            }
            else if (clause2.Contains("="))
            {
                return SplitPropertyClause(clause, "=");
            }
            else if (clause2.ToLower().Contains(" like "))
            {
                return SplitPropertyClause(clause, " like ");
            }
            else
            {
                throw new Exception("Parser Error: Unkonwn operator in clause " + clause);
            }
        }

        private static string[] SplitPropertyClause(string clause, string op)
        {
            int pos = clause.ToLower().IndexOf(op.ToLower());
            if (pos == 0)
            {
                throw new Exception("Parse Error: " + clause + " do not contain " + op);
            }

            string[] parts = new string[3];
            parts[0] = (clause.Substring(0, pos)).Trim();
            parts[1] = op.Trim();
            parts[2] = (clause.Substring(pos + op.Length, clause.Length - pos - op.Length)).Trim();

            return parts;
        }
        #endregion

        #region FromWFS
        private static void ParseWFSFilter(ITableClass fc, XmlNode parentNode, string LogicalOperator, XmlNamespaceManager ns, StringBuilder sb, ref IQueryFilter newFilter)
        {
            if (parentNode == null) return;

            foreach (XmlNode child in parentNode.ChildNodes)
            {
                XmlNode gmlNode = null;

                switch (child.Name)
                {
                    case "And":
                        StringBuilder sb1 = new StringBuilder();
                        ParseWFSFilter(fc, child, "AND", ns, sb1, ref newFilter);
                        AppendClause(LogicalOperator, "(" + sb1.ToString() + ")", sb);
                        //sb.Append("(" + sb1.ToString() + ")");
                        break;
                    case "Or":
                        StringBuilder sb2 = new StringBuilder();
                        ParseWFSFilter(fc, child, "OR", ns, sb2, ref newFilter);
                        AppendClause(LogicalOperator, "(" + sb2.ToString() + ")", sb);
                        //sb.Append("(" + sb2.ToString() + ")");
                        break;
                    case "Not":
                        StringBuilder sb3 = new StringBuilder();
                        ParseWFSFilter(fc, child, "NOT", ns, sb3, ref newFilter);
                        AppendClause(LogicalOperator, "(" + sb3.ToString() + ")", sb);
                        //sb.Append("(" + sb3.ToString() + ")");
                        break;
                    case "BBOX":
                        if (newFilter != null)
                            throw new Exception("Invalid or corrupt Filter!!!");
                        newFilter = new SpatialFilter();
                        ((ISpatialFilter)newFilter).SpatialRelation = spatialRelation.SpatialRelationEnvelopeIntersects;
                        gmlNode = (ns != null) ? child.SelectSingleNode("GML:*", ns) : child.ChildNodes[0];
                        break;
                    case "Intersects":
                        if (newFilter != null)
                            throw new Exception("Invalid or corrupt Filter!!!");
                        newFilter = new SpatialFilter();
                        ((ISpatialFilter)newFilter).SpatialRelation = spatialRelation.SpatialRelationIntersects;
                        gmlNode = (ns != null) ? child.SelectSingleNode("GML:*", ns) : child.ChildNodes[0];
                        break;
                    case "Within":
                        if (newFilter != null)
                            throw new Exception("Invalid or corrupt Filter!!!");
                        newFilter = new SpatialFilter();
                        ((ISpatialFilter)newFilter).SpatialRelation = spatialRelation.SpatialRelationContains;
                        gmlNode = (ns != null) ? child.SelectSingleNode("GML:*", ns) : child.ChildNodes[0];
                        break;
                    case "Contains":
                        if (newFilter != null)
                            throw new Exception("Invalid or corrupt Filter!!!");
                        newFilter = new SpatialFilter();
                        ((ISpatialFilter)newFilter).SpatialRelation = spatialRelation.SpatialRelationContains;
                        gmlNode = (ns != null) ? child.SelectSingleNode("GML:*", ns) : child.ChildNodes[0];
                        break;
                    default:
                        string clause = ParseWFSFilterNode(fc, child, ns);
                        AppendClause(LogicalOperator, clause, sb);
                        break;
                }

                if (gmlNode != null && newFilter is ISpatialFilter)
                {
                    //((ISpatialFilter)newFilter).Geometry = GeometryTranslator.GML2Geometry(gmlNode.OuterXml);
                    //if (((ISpatialFilter)newFilter).Geometry == null)
                    //    throw new Exception("Unknown GML Geometry...");

                    //if (gmlNode.Attributes["srsName"] != null)
                    //    ((ISpatialFilter)newFilter).SpatialReference = SpatialReference.FromID(gmlNode.Attributes["srsName"].Value);
                }
            }
        }

        private static string ParseWFSFilterNode(ITableClass fc, XmlNode node, XmlNamespaceManager ns)
        {
            //if(fc==null) 
            //    throw new ArgumentException("ParseWFSFilterNode: FeatureClass is null...");
            if (node == null) return String.Empty;

            string op = String.Empty;
            switch (node.Name)
            {
                case "PropertyIsEqualTo":
                    op = "=";
                    break;
                case "PropertyIsLike":
                    op = " like ";
                    break;
                case "PropertyIsGreaterThan":
                    op = ">";
                    break;
                case "PropertyIsGreaterThanOrEqualTo":
                    op = ">=";
                    break;
                case "PropertyIsLessThan":
                    op = "<";
                    break;
                case "PropertyIsLessThanOrEqualTo":
                    op = "<=";
                    break;
                case "PropertyIsNotEqualTo":
                    op = "<>";
                    break;
                default:
                    throw new Exception("Unknown operation: " + node.Name + ". Xml is case sensitive.");
            }

            XmlNodeList propertyName = (ns != null) ? node.SelectNodes("OGC:PropertyName", ns) : node.SelectNodes("PropertyName");
            if (propertyName.Count > 1)
            {
                throw new Exception("More than one PropertyName in Property: " + node.OuterXml);
            }
            if (propertyName.Count == 0)
            {
                throw new Exception("No PropertyName in Property: " + node.OuterXml);
            }
            XmlNodeList Literal = (ns != null) ? node.SelectNodes("OGC:Literal", ns) : node.SelectNodes("Literal");
            if (Literal.Count > 1)
            {
                throw new Exception("More than one Literal in Property: " + node.OuterXml);
            }
            if (Literal.Count == 0)
            {
                throw new Exception("No Literal in Property: " + node.OuterXml);
            }

            string fieldname = propertyName[0].InnerText;
            string fieldvalue = Literal[0].InnerText;

            if (fc != null)
            {
                IField field = fc.FindField(fieldname);
                if (field == null)
                {
                    throw new Exception("Type " + fc.Name + " do not has field " + fieldname);
                }

                switch (field.type)
                {
                    case FieldType.String:
                        fieldvalue = "'" + fieldvalue + "'";
                        break;
                    case FieldType.Date:
                        fieldvalue = "'" + fieldvalue + "'";
                        break;
                }
            }

            return fieldname + op + fieldvalue;
        }

        private static void AppendClause(string op, string clause, StringBuilder sb)
        {
            if (clause == String.Empty) return;

            if (sb.Length > 0)
                sb.Append(" " + op + " ");
            sb.Append(clause);
        }

        private static string RemoveOuterBrackets(string sql)
        {
            if (sql == null || sql == String.Empty) return String.Empty;

            byte[] c = new byte[sql.Length];
            int pos = 0;
            for (int i = 0; i < sql.Length; i++)
            {
                if (sql[i] == '(')
                    c[i] = (byte)++pos;
                if (sql[i] == ')')
                    c[i] = (byte)pos--;
            }

            if (c[0] != 1 || c[c.Length - 1] != 1) return sql;
            if (CountValues(c, 1) == 2)
            {
                return RemoveOuterBrackets(sql.Substring(1, sql.Length - 2));
            }
            else
            {
                return sql;
            }
        }
        private static int CountValues(byte[] array, byte val)
        {
            int counter = 0;
            for (int i = 0; i < array.Length; i++)
                if (array[i] == val)
                    counter++;

            return counter;
        }
        #endregion

        #endregion

        #endregion
    }
}
