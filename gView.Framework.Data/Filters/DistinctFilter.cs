using gView.Framework.Core.Data.Filters;

namespace gView.Framework.Data.Filters
{
    public class DistinctFilter : QueryFilter, IDistinctFilter
    {
        private DistinctFilter()
            : base()
        {
        }
        public DistinctFilter(string field)
            : base()
        {
            m_fields.Add(field);
        }
        public DistinctFilter(string field, string alias)
            : base()
        {
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
                        subfields += "DISTINCT(";
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
        public override void AddField(string fieldname, bool caseSensitive = true)
        {
            // Nicht möglich
        }
        public override void AddField(string fieldname, string alias)
        {
            // Nicht möglich
        }

        public override object Clone()
        {
            DistinctFilter clone = new DistinctFilter();
            base.CopyTo(clone);

            return clone;
        }
    }
}
