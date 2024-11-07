using System.Collections.Generic;
using System.Text;

namespace gView.Framework.Common
{
    public class UniqueList<T> : List<T>
    {
        new public void Add(T t)
        {
            if (Contains(t))
            {
                return;
            }

            base.Add(t);
        }

        public string ToString(char seperator)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Count; i++)
            {
                if (sb.Length > 0)
                {
                    sb.Append(seperator);
                }

                sb.Append(this[i].ToString());
            }
            return sb.ToString();
        }
    }
}
