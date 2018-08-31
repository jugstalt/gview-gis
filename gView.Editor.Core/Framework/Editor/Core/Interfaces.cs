using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Data;
using gView.Framework.Geometry;

namespace gView.Framework.Editor.Core
{
    public class Globals
    {
        public const string ModuleGuidString = "45713F48-0D81-4a54-A422-D0E6F397BC95";
        static public Guid ModuleGuid = new Guid(ModuleGuidString);
    }

    public class EditorEventArgument
    {
        private IFeatureClass _fc = null;
        private IFeature _feature = null;
        private bool _cancel = false;
        private string _msg = String.Empty;

        public EditorEventArgument(IFeatureClass fc, IFeature feature)
        {
            _fc = fc;
            _feature = feature;
        }

        public IFeatureClass FeatureClass
        {
            get { return _fc; }
        }
        public IFeature Feature
        {
            get { return _feature; }
        }
        public bool Cancel
        {
            get { return _cancel; }
            set { _cancel = value; }
        }
        public string Message
        {
            get { return _msg; }
            set { _msg = value; }
        }
    }

    public delegate void OnBeginEditFeatureEventHandler(object sender, EditorEventArgument args);
    public delegate void OnCreateFeatureEventHandler(object sender, EditorEventArgument args);
    public delegate void OnInsertFeatureEventHandler(object sender, EditorEventArgument args);
    public delegate void OnUpdateFeatureEventHandler(object sender, EditorEventArgument args);
    public delegate void OnDeleteFeatureEventHandler(object sender, EditorEventArgument args);
    public delegate void OnEditLayerCollectionChangedEventHandler(object sender);

    public interface IModule : IModuleEvents
    {
    }

    public interface IModuleEvents
    {
        event OnBeginEditFeatureEventHandler OnBeginEditFeature;
        event OnCreateFeatureEventHandler OnCreateFeature;
        event OnInsertFeatureEventHandler OnInsertFeature;
        event OnUpdateFeatureEventHandler OnUpdateFeature;
        event OnDeleteFeatureEventHandler OnDeleteFeature;

        event OnEditLayerCollectionChangedEventHandler OnEditLayerCollectionChanged;
    }

    public enum EditStatements { NONE = 0, INSERT = 1, UPDATE = 2, DELETE = 4 }

    public interface IEditLayer
    {
        IFeatureLayer FeatureLayer { get; }
        EditStatements AllowedStatements { get; }
    }

    public interface IEditableDatabase
    {
    }

    public interface IPenTool
    {
        string Name { get; }
        object Image { get; }

        void OnCreate(IModule module);

        IPoint CalcPoint(int mouseX, int mouseY, IPoint world);
        bool MouseClick();
        bool Activated(IPoint world, IPoint worldMouse, IPoint vertex);

        bool DrawMover { get; }
        bool UseCalcToolResultType(CalcToolResultType type);
        void EvaluateCalcToolResult(CalcToolResultType type, object result);
    }

    public enum CalcToolResultType { None = 0, AbsolutPos = 1, Direction = 2, Distance = 3, SnapTo = 4 }
    public interface ICalcTool
    {
        string Name { get; }
        object Image { get; }

        void OnCreate(IModule module);
        object Calc(IPoint world, IPoint worldMouse, IPoint vertex);

        CalcToolResultType ResultType { get; }
        bool Enabled { get; }
    }
}
