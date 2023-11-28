using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Data.Filters;
using System;
using System.Threading.Tasks;

namespace gView.Framework.Data.Filters
{
    public class FunctionFilter : QueryFilter, IFunctionFilter
    {
        private string _function = String.Empty;

        private FunctionFilter() : base() { }
        public FunctionFilter(string function, string field)
            : base()
        {
            _function = function;
            m_fields.Add(field);
        }
        public FunctionFilter(string function, string field, string alias)
            : base()
        {
            _function = function;
            m_fields.Add(field);
            m_alias.Add(field, alias);
        }

        public override string SubFieldsAndAlias
        {
            get
            {
                string subfields = "";
                foreach (string field in m_fields)
                {
                    if (subfields != "")
                    {
                        subfields += ",";
                    }

                    if (field == "*")
                    {
                        subfields += field;
                    }
                    else
                    {
                        subfields += _function + "(";
                        if (field.IndexOf(m_fieldPrefix) != 0 && field.IndexOf("(") == -1 && field.IndexOf(")") == -1)
                        {
                            subfields += m_fieldPrefix;
                        }

                        subfields += field;
                        if (field.IndexOf(m_fieldPostfix) != field.Length - m_fieldPostfix.Length && field.IndexOf("(") == -1 && field.IndexOf(")") == -1)
                        {
                            subfields += m_fieldPostfix;
                        }

                        subfields += ")";

                        string alias = Alias(field);
                        if (alias != "" && alias != null)
                        {
                            subfields += " as " + alias;
                        }
                    }
                }
                return subfields;
            }
        }

        public string Function
        {
            get { return _function; }
        }

        public string FunctionAlias
        {
            get
            {
                if (m_fields.Count == 1 && m_alias.ContainsKey(m_fields[0]))
                {
                    return m_alias[m_fields[0]];
                }

                return String.Empty;
            }
        }

        public override void AddField(string fieldname, bool caseSensitive = true)
        {
            // Nicht möglich
        }
        public override void AddField(string fieldname, string alias)
        {
            // Nicht möglich
        }

        async public static Task<object> QueryScalar(IFeatureClass fc, FunctionFilter filter, string fieldName)
        {
            if (fc == null || filter == null)
            {
                return null;
            }

            using (IFeatureCursor cursor = await fc.Search(filter) as IFeatureCursor)
            {
                if (cursor is IFeatureCursorSkills && ((IFeatureCursorSkills)cursor).KnowsFunctions == false)
                {
                    double ret = 0D;
                    #region Init 

                    switch (filter._function.ToLower())
                    {
                        case "min":
                            ret = double.MaxValue;
                            break;
                        case "max":
                            ret = double.MinValue;
                            break;
                        default:
                            ret = 0D;
                            break;
                    }

                    #endregion

                    IFeature feature = null;
                    string subField = filter.SubFields.Split(' ')[0];
                    bool hasFeature = false;
                    while ((feature = await cursor.NextFeature()) != null)
                    {
                        hasFeature = true;
                        double val = Convert.ToDouble(feature[subField]);

                        switch (filter._function.ToLower())
                        {
                            case "min":
                                ret = Math.Min(ret, val);
                                break;
                            case "max":
                                ret = Math.Max(ret, val);
                                break;
                            case "sum":
                                ret += val;
                                break;
                        }
                    }

                    if (hasFeature == false)
                    {
                        return null;
                    }

                    return ret;
                }
                else
                {
                    if (cursor == null)
                    {
                        return null;
                    }

                    IFeature feature = await cursor.NextFeature();
                    if (feature == null)
                    {
                        return null;
                    }

                    return feature[fieldName];
                }
            }
        }

        public override object Clone()
        {
            FunctionFilter clone = new FunctionFilter();
            clone._function = _function;
            base.CopyTo(clone);
            return clone;
        }
    }
}
