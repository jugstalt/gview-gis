using System;
using System.Collections.Generic;
using System.Text;
using System.Resources;
using System.Reflection;
using System.Windows.Forms;

namespace gView.Framework.Globalisation
{
    public class LocalizedResources
    {
        static private ResourceManager _resMan = null;

        static public string GetResString(string name, string defString)
        {
            return GetResString(name, defString, String.Empty);
        }

        static public string GetResString(string name, string defString, string arg)
        {
            try
            {
                if (_resMan == null)
                {
                    _resMan = new ResourceManager(
                        "gView.Globalisation.ResNames",
                        typeof(LocalizedResources).Assembly);
                }

                string ret = _resMan.GetString(name);
                if (ret == null)
                    return defString;

                if (!String.IsNullOrEmpty(arg))
                {
                    ret = String.Format(ret, arg);
                }

                return ret;
            }
            catch
            {
                return defString;
            }
        }

        #region Forms
        static public void GlobalizeMenuItem(ToolStripMenuItem item)
        {
            if (item == null) return;

            if (item.DropDownItems.Count > 0)
            {
                item.Text = LocalizedResources.GetResString("MenuHeader." + item.Text.Replace(" ", ""), item.Text);
            }
            else
            {
                item.Text = LocalizedResources.GetResString("Menu." + item.Text.Replace(" ", ""), item.Text);
            }

            for (int i = 0; i < item.DropDownItems.Count; i++)
                GlobalizeMenuItem(item.DropDownItems[i] as ToolStripMenuItem);
        }
        static public void GlobalizeMenuItem(ToolStripDropDownButton item)
        {
            for (int i = 0; i < item.DropDownItems.Count; i++)
                GlobalizeMenuItem(item.DropDownItems[i] as ToolStripMenuItem);
        }
        static public void GlobalizeMenuItem(ContextMenuStrip item)
        {
            if (item == null) return;

            for (int i = 0; i < item.Items.Count; i++)
                GlobalizeMenuItem(item.Items[i] as ToolStripMenuItem);
        }
        #endregion

        #region Wpf
        static public void GlobalizeWpfMenuItem(System.Windows.Controls.MenuItem item)
        {
            if (item == null) return;

            if (item.Header is string)
            {
                if (item.Items.Count > 0)
                {
                    item.Header = LocalizedResources.GetResString("MenuHeader." + item.Header.ToString().Replace(" ", ""), item.Header.ToString());
                }
                else
                {
                    item.Header = LocalizedResources.GetResString("Menu." + item.Header.ToString().Replace(" ", ""), item.Header.ToString());
                }
            }

            for (int i = 0; i < item.Items.Count; i++)
                GlobalizeWpfMenuItem(item.Items[i] as System.Windows.Controls.MenuItem);
        }
        #endregion

    }
}
