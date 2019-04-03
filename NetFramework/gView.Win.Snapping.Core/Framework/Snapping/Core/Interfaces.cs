using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Carto;
using gView.Framework.Data;
using gView.Framework.UI;

namespace gView.Framework.Snapping.Core
{
    public class Globals
    {
        public const string ModuleGuidString = "2E50D579-766B-4ed9-A6F9-A4F3472F77AC";
        static public Guid ModuleGuid = new Guid(ModuleGuidString);
    }

    public enum SnapMethode { None=0, Vertex = 1, Edge = 2, EndPoint = 4 }

    public interface ISnapLayer
    {
        SnapMethode Methode { get; }
        IFeatureLayer FeatureLayer { get; }
    }
    
    public interface ISnapSchema : IEnumerable<ISnapLayer>
    {
        string Name { get; }
        double MaxScale { get; }

        void Clear();
        void Add(ISnapLayer layer);
        void Remove(ISnapLayer layer);
        void Remove(IFeatureLayer layer);
    }

    public interface ISnapModule
    {
        List<ISnapSchema> this[IMap map] { get; }

        ISnapSchema ActiveSnapSchema { get; set; }
        void RefreshGUI();

        void Snap(ref double X, ref double Y);
        int SnapTolerance { get; set; }
    }

    public interface ISnapTool
    {
        void Snap(ref double X, ref double Y);
        bool ShowSnapMarker { get; }
    }

    public class SnapTool : ISnapTool
    {
        private ISnapModule _module = null;

        virtual public void OnCreate(object hook)
        {
            if (hook is IMapDocument && ((IMapDocument)hook).Application is IMapApplication)
            {
                _module = ((IMapApplication)((IMapDocument)hook).Application).IMapApplicationModule(Globals.ModuleGuid) as ISnapModule;
            }
        }

        #region ISnapTool Member

        virtual public void Snap(ref double X, ref double Y)
        {
            if (_module != null)
            {
                _module.Snap(ref X, ref Y);
            }
        }
        virtual public bool ShowSnapMarker
        {
            get { return true; }
        }
        #endregion
    }
}
