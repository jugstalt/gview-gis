using gView.Framework.system;
using System.Collections;
using System.Collections.Generic;

namespace gView.Framework.Data
{

    public class DisplayFilterCollection : List<DisplayFilter>
    {
        public static DisplayFilterCollection FromJSON(string json)
        {
            try
            {
                if (json.StartsWith("["))
                {
                    json = "{ \"filters\":" + json + "}";
                }

                json = json.Replace("\r", "").Replace("\n", "").Replace("\t", "");
                Hashtable f = (Hashtable)JSON.JsonDecode(json);

                DisplayFilterCollection filterCollection = new DisplayFilterCollection();

                ArrayList filters = (ArrayList)f["filters"];
                for (int i = 0; i < filters.Count; i++)
                {
                    Hashtable filter = (Hashtable)filters[i];
                    DisplayFilter displayFilter = new DisplayFilter();
                    if (filter["sql"] != null)
                    {
                        displayFilter.Filter = (string)filter["sql"];
                    }

                    if (filter["color"] != null)
                    {
                        displayFilter.Color = ColorConverter2.ConvertFrom((string)filter["color"]);
                    }

                    if (filter["penwidth"] is double)
                    {
                        displayFilter.PenWidth = (float)((double)filter["penwidth"]);
                    }

                    filterCollection.Add(displayFilter);
                }
                return filterCollection;
            }
            catch
            {
            }
            return null;
        }
    }
}
