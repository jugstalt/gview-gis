using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gView.Framework.UI;

namespace gView.Framework.system.UI
{
    public class MapToolCls : ITool
    {
        public MapToolCls(string name)
        {
            this.Name = name;

            this.Enabled = true;
            this.ToolTip = String.Empty;
            this.toolType = ToolType.command;
        }
        public MapToolCls(string name, object image)
            : this(name)
        {
            this.Image = image;
        }

        #region ITool Member

        public string Name
        {
            get;
            private set;
        }

        public bool Enabled
        {
            get;
            protected set;
        }

        public string ToolTip
        {
            get;
            protected set;
        }

        public ToolType toolType
        {
            get;
            protected set;
        }

        public object Image
        {
            get;
            private set;
        }

        virtual public void OnCreate(object hook)
        {
            if (hook is IMapDocument)
                this.MapDocument = (IMapDocument)hook;
        }

        virtual public void OnEvent(object MapEvent)
        {
            
        }

        #endregion

        public IMapDocument MapDocument
        {
            get;
            protected set;
        }
    }
}
