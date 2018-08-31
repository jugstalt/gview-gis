using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gView.Framework.system
{
    public class UniqueList<T> : List<T>
    {
        new public void Add(T t)
        {
            if (base.Contains(t))
                return;

            base.Add(t);
        }

        public string ToString(char seperator)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < this.Count; i++)
            {
                if (sb.Length > 0) sb.Append(seperator);
                sb.Append(this[i].ToString());
            }
            return sb.ToString();
        }
    }
}
