using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using gView.Framework.UI;

namespace gView.Explorer.UI.Framework.UI
{
    public interface IFormExplorer : IGUIAppWindow
    {
        void ValidateButtons();
        void AppendContextMenuItems(ContextMenuStrip strip, object context);
        void Close();
        void RefreshContents();
        void SetStatusbarText(string text);
        void SetStatusbarProgressVisibility(bool vis);
        void SetStatusbarProgressValue(int val);
        void RefreshStatusbar();

        gView.Framework.UI.Dialogs.FormCatalogTree CatalogTree { get; }
        string Text { get; set; }
        List<IExplorerObject> SelectedObjects { get; }

        bool MoveToTreeNode(string path);

        void SetCursor(object cursor);
    }
}
