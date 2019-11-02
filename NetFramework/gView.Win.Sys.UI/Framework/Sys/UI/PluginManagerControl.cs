using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace gView.Framework.system.UI
{
    public partial class PluginManagerControl : UserControl
    {
        public PluginManagerControl()
        {
            InitializeComponent();

            PlugInManager pm = new PlugInManager();
            foreach (Type type in pm.GetPluginTypes.OrderBy(t=>t.ToString()))
            {
                TypeNode node = new TypeNode(type);
 
                tvPlugins.Nodes.Add(node);
                node.Nodes.Add(new TreeNode());
            }
        }

        private void tvPlugins_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (tvPlugins.SelectedNode is TypeNode)
            {
                e.Node.Nodes.Clear();

                PlugInManager pm = new PlugInManager();

                var plugins = pm.GetPluginInstances(((TypeNode)e.Node).InterfaceType);
                if (plugins == null)
                    return;

                foreach (object plugin in plugins)
                {
                    e.Node.Nodes.Add(new PluginNode(plugin));
                }
            }
        }

        private void tvPlugins_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (tvPlugins.SelectedNode is PluginNode)
            {
                txtGuid.Text = PlugInManager.PlugInID(((PluginNode)tvPlugins.SelectedNode).Plugin).ToString();
                Assembly assembly = Assembly.GetAssembly(((PluginNode)tvPlugins.SelectedNode).Plugin.GetType());
                txtAssembly.Text = assembly.FullName;
                txtPath.Text = assembly.Location;
            }
        }

        #region Item Classes

        public class TypeNode : TreeNode
        {
            public TypeNode(Type type)
            {
                base.Text = type.ToString();
                this.InterfaceType = type;
            }

            public Type InterfaceType { get; set; }

            public override string ToString()
            {
                return this.InterfaceType.ToString();
            }
        }

        private class PluginNode : TreeNode
        {
            public PluginNode(object plugin)
            {
                Plugin = plugin;
                base.Text = plugin.GetType().ToString();
            }

            public object Plugin { get; set; }

            public override string ToString()
            {
                return Plugin.ToString();
            }

        }

        #endregion
    }
}
