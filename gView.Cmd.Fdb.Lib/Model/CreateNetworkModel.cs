using gView.Framework.Data;
using gView.Framework.IO;
using gView.Framework.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gView.Cmd.Fdb.Lib.Model;

public class CreateNetworkModel : IPersistable
{
    public string Name { get; set; }=string.Empty;
    public string ConnectionString { get; set; } = string.Empty;    
    public Guid DatasetGuid { get; set; }
    public string DatasetName { get; set; } = string.Empty;

    public IEnumerable<Edge>? Edges { get; set; }
    public IEnumerable<Node>? Nodes { get; set; }

    public bool UseSnapTolerance { get; set; }
    public double SnapTolerance { get; set; } = 0D;

    public GraphWeights Weights { get; } = new GraphWeights();

    #region IPersistable

    public void Load(IPersistStream stream)
    {
        this.Name = (string)stream.Load("name", string.Empty);
        this.ConnectionString = (string)stream.Load("connection_string", string.Empty);
        this.DatasetGuid = new Guid((string)stream.Load("dataset_guid", Guid.Empty.ToString()));
        this.DatasetName = (string)stream.Load("dataset_name", string.Empty);

        var edges = new List<Edge>();
        var nodes = new List<Node>();
        while(true)
        {
            string edgeName = (string)stream.Load($"edge{edges.Count}_name", string.Empty);
            if (string.IsNullOrEmpty(edgeName))
            {
                break;
            }

            edges.Add(new Edge()
            {
                Name = edgeName,
                IsComplexEdge = (bool)stream.Load($"edge{edges.Count}_complex", false)
            });
        }
        while (true)
        {
            string nodeName = (string)stream.Load($"node{nodes.Count}_name", string.Empty);
            if (string.IsNullOrEmpty(nodeName))
            {
                break;
            }

            nodes.Add(new Node()
            {
                Name = nodeName,
                IsSwitch = (bool)stream.Load($"node{nodes.Count}_is_switch", false),
                Fieldname = (string)stream.Load($"node{nodes.Count}_fieldname", string.Empty),
                NodeType = (NetworkNodeType)(int)stream.Load($"node{nodes.Count}_node_type", (int)NetworkNodeType.Unknown)
            });

        }

        if (edges.Count > 0)
        {
            this.Edges = edges;
        }
        if (nodes.Count > 0)
        {
            this.Nodes = nodes;
        }
    }

    public void Save(IPersistStream stream)
    {
        stream.Save("name", this.Name);
        stream.Save("connection_string", this.ConnectionString);
        stream.Save("dataset_guid", this.DatasetGuid.ToString());
        stream.Save("dataset_name", this.DatasetName);

        if (Edges != null)
        {
            var edges = Edges.ToArray();
            for (int i = 0; i < edges.Length; i++)
            {
                stream.Save($"edge{i}_name", edges[i].Name);
                stream.Save($"edge{i}_complex", edges[i].IsComplexEdge);
            }
        }

        if (Nodes != null)
        {
            var nodes = Nodes.ToArray();
            for (int i = 0; i < nodes.Length; i++)
            {
                stream.Save($"node{i}_name", nodes[i].Name);
                stream.Save($"node{i}_is_switch", nodes[i].IsSwitch);
                stream.Save($"node{i}_fieldname", nodes[i].Fieldname);
                stream.Save($"node{i}_node_type", (int)nodes[i].NodeType);
            }
        }

        stream.Save("use_snap_tolerance", this.UseSnapTolerance);
        stream.Save("snap_tolerance", this.SnapTolerance);
    }

    #endregion

    #region Classes

    public class Edge 
    {
        public string Name { get; set; } = String.Empty;
        public bool IsComplexEdge { get; set; }
    }

    public class Node
    {
        public string Name { get; set; } = String.Empty;
        public bool IsSwitch { get; set; }
        public string Fieldname { get; set; } = String.Empty;
        public NetworkNodeType NodeType { get; set; } = NetworkNodeType.Unknown;
    }

    #endregion
}
