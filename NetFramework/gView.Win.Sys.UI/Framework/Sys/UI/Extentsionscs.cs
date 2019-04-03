using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gView.Framework.UI;
using System.Threading.Tasks;

namespace gView.system.UI.Framework.system.UI
{
    public static class AysncMethods
    {
        private static Action EmptyDelegate = delegate() { };

        public static List<IExplorerObject> ChildObjectsAsync(IExplorerParentObject parentObject)
        {
            ChildObjectsList col = new ChildObjectsList(parentObject);
            Task task = new Task(new Action(col.Get));
            task.RunSynchronously();
            return col.ChildObjects;
        }

        private class ChildObjectsList
        {
            public List<IExplorerObject> ChildObjects;
            public IExplorerParentObject ParentObject;

            public ChildObjectsList(IExplorerParentObject parentObject)
            {
                ParentObject = parentObject;
            }

            public void Get()
            {
                ChildObjects = ParentObject.ChildObjects;
            }
        }
    }
}
