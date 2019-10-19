using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Xml;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using gView.Framework.Data;
using gView.Framework.Geometry;

namespace gView.Interoperability.ArcXML.UI.ImportAXL
{
    internal partial class FormSelectAXLLayer : Form
    {
        public FormSelectAXLLayer(XmlDocument doc, IFeatureLayer layer)
        {
            InitializeComponent();

            FillList(doc, layer);
        }

        private void FillList(XmlDocument doc, IFeatureLayer layer)
        {
            if (layer == null || layer.FeatureClass == null) return;

            foreach (XmlNode layerNode in doc.SelectNodes("ARCXML/CONFIG/MAP/LAYER[@type='featureclass']"))
            {
                if (layerNode.Attributes["name"] == null) continue;

                XmlNode datasetNode = layerNode.SelectSingleNode("DATASET[@type]");
                if (datasetNode == null) continue;

                switch (datasetNode.Attributes["type"].Value)
                {
                    case "point":
                        if (layer.FeatureClass.GeometryType != geometryType.Point) continue;
                        break;
                    case "line":
                        if (layer.FeatureClass.GeometryType != geometryType.Polyline) continue;
                        break;
                    case "polygon":
                        if (layer.FeatureClass.GeometryType != geometryType.Polygon) continue;
                        break;
                }

                AXLLayerItem item=new AXLLayerItem(layerNode);
                lstLayers.Items.Add(item);
                if ((item.DatasetName.ToLower() == layer.FeatureClass.Name.ToLower() ||
                     item.DatasetName.Replace(".", "_").ToLower() == layer.FeatureClass.Name.ToLower()) &&
                    (lstLayers.SelectedIndex == -1))
                {
                    lstLayers.SelectedItem = item;
                }
            }
        }

        public XmlNode SelectedNode
        {
            get
            {
                if (lstLayers.SelectedItem == null) return null;
                return ((AXLLayerItem)lstLayers.SelectedItem).Node;
            }
        }
    }

    internal class AXLLayerItem
    {
        private XmlNode _node;

        public AXLLayerItem(XmlNode layerNode)
        {
            _node = layerNode;
        }

        public override string ToString()
        {
            if (_node == null || _node.Attributes["name"] == null) return "???";
            return _node.Attributes["name"].Value;
        }

        public string DatasetName
        {
            get
            {
                if (_node == null) return "???";
                XmlNode datasetNode = _node.SelectSingleNode("DATASET[@name]");
                if (datasetNode == null) return "???";

                return datasetNode.Attributes["name"].Value;
            }
        }

        public XmlNode Node
        {
            get
            {
                return _node;
            }
        }
    }
}