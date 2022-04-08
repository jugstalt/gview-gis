using gView.Framework.Carto;
using gView.Framework.Editor.Core;
using gView.Framework.IO;
using gView.Framework.system;
using System;
using System.Collections.Generic;
using System.Linq;

namespace gView.Plugins.Modules
{
    [gView.Framework.system.RegisterPlugIn("45713F48-0D81-4a54-A422-D0E6F397BC95", PluginUsage.Server)]
    public class EditorModule : IMapApplicationModule, IPersistable
    {
        private List<EditLayer> _editLayers = new List<EditLayer>();

        internal void AddEditLayer(EditLayer editLayer)
        {
            _editLayers.Add(editLayer);
        }

        internal IEnumerable<EditLayer> EditLayers => _editLayers;

        public IEditLayer GetEditLayer(int id)
        {
            return _editLayers
                .Where(l => l.LayerId == id)
                .Select(l => new EditLayer(l))
                .FirstOrDefault();
        }

        public IMap Map { get; private set; }

        #region IMapApplicationModule

        public void OnCreate(object hook)
        {
            if (hook is IMap)
            {
                Map = (IMap)hook;
            }
        }

        #endregion

        #region IPersistable

        public void Load(IPersistStream stream)
        {
            _editLayers.Clear();
            if (stream == null)
            {
                return;
            }

            MapEditLayerPersist mapEditLayers;
            while ((mapEditLayers = (MapEditLayerPersist)stream.Load("MapEditLayers", null, new MapEditLayerPersist(this))) != null)
            {
            }
        }

        public void Save(IPersistStream stream)
        {
            stream.Save("MapEditLayers", new MapEditLayerPersist(this));
        }

        #endregion

        #region Classes

        internal class MapEditLayerPersist : IPersistable
        {
            private EditorModule _module;

            public MapEditLayerPersist(EditorModule module)
            {
                _module = module;
            }

            public EditorModule Module { get { return _module; } }

            #region IPersistable Member

            public void Load(IPersistStream stream)
            {
                if (stream == null ||
                    _module == null)
                {
                    return;
                }

                EditLayer eLayer;
                while ((eLayer = (EditLayer)stream.Load("EditLayer", null, new EditLayer())) != null)
                {
                    _module.AddEditLayer(eLayer);
                }
            }

            public void Save(IPersistStream stream)
            {
                if (stream == null ||
                    _module?.Map == null)
                {
                    return;
                }

                stream.Save("index", 0);
                stream.Save("name", _module.Map.Name);

                foreach (IEditLayer editLayer in _module.EditLayers)
                {
                    if (editLayer == null)
                    {
                        continue;
                    }

                    stream.Save("EditLayer", editLayer);
                }
            }

            #endregion
        }

        internal class EditLayer : IPersistable, IEditLayer
        {
            public EditLayer()
            {
                this.Statements = EditStatements.NONE;
            }

            public EditLayer(EditLayer editLayer)
            {
                this.LayerId = editLayer.LayerId;
                this.ClassName = editLayer.ClassName;
                this.Statements = editLayer.Statements;
            }

            public int LayerId { get; private set; }
            public string ClassName { get; private set; }
            public EditStatements Statements { get; private set; }

            public Framework.Data.IFeatureLayer FeatureLayer { get { return null; } }

            #region IPersistable

            public void Load(IPersistStream stream)
            {
                LayerId = (int)stream.Load("id", -1);
                ClassName = (string)stream.Load("class", String.Empty);
                Statements = (EditStatements)stream.Load("statement", (int)EditStatements.NONE);
            }

            public void Save(IPersistStream stream)
            {
                stream.Save("id", LayerId);
                stream.Save("class", ClassName);
                stream.Save("statement", (int)Statements);
            }

            #endregion
        }

        #endregion  
    }
}
