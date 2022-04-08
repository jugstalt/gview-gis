using gView.Explorer.UI.Framework.UI;
using gView.Framework.Sys.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gView.Framework.UI
{
    public class ExplorerApplication : License, IExplorerApplication
    {
        private gView.Explorer.UI.Framework.UI.IFormExplorer _appWindow = null;
        private IStatusBar _statusBar = null;

        public event EventHandler OnApplicationStart;

        public ExplorerApplication(gView.Explorer.UI.Framework.UI.IFormExplorer appWindow)
            : base()
        {
            _appWindow = appWindow;
            _statusBar = new ExplorerStatusBar(_appWindow);
        }

        #region IGUIApplication Members

        public event DockWindowAddedEvent DockWindowAdded;
        public event OnShowDockableWindowEvent OnShowDockableWindow;

        ArrayList _dockWindows = new ArrayList();

        public bool InvokeRequired
        {
            get
            {
                return _appWindow.InvokeRequired;
            }
        }
        public object Invoke(Delegate method)
        {
            return _appWindow.Invoke(method);
        }

        public List<IDockableWindow> DockableWindows
        {
            get
            {
                List<IDockableWindow> e = new List<IDockableWindow>();

                foreach (IDockableWindow window in _dockWindows)
                {
                    e.Add(window);
                }
                return e;
            }
        }

        virtual public void AddDockableWindow(IDockableWindow window, DockWindowState state)
        {
            if (window != null)
            {
                foreach (IDockableWindow win in _dockWindows)
                {
                    if (win == window)
                    {
                        return;
                    }
                }

                window.DockableWindowState = state;
                _dockWindows.Add(window);
            }
            if (DockWindowAdded != null)
            {
                DockWindowAdded(window, "");
            }
        }

        virtual public void AddDockableWindow(IDockableWindow window, string parentDockableWindowName)
        {
            if (window != null)
            {
                foreach (IDockableWindow win in _dockWindows)
                {
                    if (win == window)
                    {
                        return;
                    }
                }
                if (parentDockableWindowName != "")
                {
                    window.DockableWindowState = DockWindowState.child;
                }
                else if (window is IDockableToolWindow)
                {
                    window.DockableWindowState = DockWindowState.right;
                    foreach (IDockableWindow win in _dockWindows)
                    {
                        if (win is IDockableToolWindow)
                        {
                            parentDockableWindowName = win.Name;
                            window.DockableWindowState = DockWindowState.child;
                            break;
                        }
                    }
                }
                _dockWindows.Add(window);
            }
            if (DockWindowAdded != null)
            {
                DockWindowAdded(window, parentDockableWindowName);
            }
        }

        virtual public void ShowDockableWindow(IDockableWindow window)
        {
            if (window is Form)
            {
                ((Form)window).Visible = true;
            }
            if (OnShowDockableWindow != null)
            {
                OnShowDockableWindow(window);
            }
        }

        public List<object> ToolStrips
        {
            get { return null; }
        }

        public ITool Tool(Guid guid)
        {
            return null;
        }
        public List<ITool> Tools
        {
            get { return new List<ITool>(); }
        }
        public IToolbar Toolbar(Guid guid)
        {
            return null;
        }
        public List<IToolbar> Toolbars
        {
            get
            {
                return new List<IToolbar>();
            }
        }
        public ITool ActiveTool
        {
            get { return null; }
            set { }
        }
        public IGUIAppWindow ApplicationWindow
        {
            get { return _appWindow; }
        }

        public void ValidateUI()
        {
            if (_appWindow != null)
            {
                _appWindow.ValidateButtons();
            }
        }

        public IStatusBar StatusBar
        {
            get
            {
                return _statusBar;
            }
        }

        public void AppendContextMenuItems(ContextMenuStrip strip, object context)
        {
            if (_appWindow != null)
            {
                _appWindow.AppendContextMenuItems(strip, context);
            }
        }

        public void SetCursor(object cursor)
        {
            if (_appWindow != null)
            {
                _appWindow.SetCursor(cursor);
            }
        }

        #endregion

        #region IApplication Members

        public string Title
        {
            get
            {
                return (_appWindow != null) ? _appWindow.Text : "";
            }
            set
            {
                if (_appWindow != null)
                {
                    _appWindow.Text = value;
                }
            }
        }

        public void Exit()
        {
            if (_appWindow != null)
            {
                _appWindow.Close();
            }
        }

        #endregion

        #region IExplorerApplication Members

        async public Task ExecuteBatch(string batch, ExplorerExecuteBatchCallback callback)
        {
            //Thread thread = new Thread(new ParameterizedThreadStart(CommandInterpreter.RunProcess));
            //thread.Start(new CommandInterpregerArgs(batch, callback));

            await CommandInterpreter.RunProcess(batch, callback);
        }

        public List<IExplorerObject> SelectedObjects
        {
            get
            {
                if (_appWindow == null)
                {
                    return new List<IExplorerObject>();
                }

                return _appWindow.SelectedObjects;
            }
        }

        public void RefreshContents()
        {
            if (_appWindow != null)
            {
                _appWindow.RefreshContents();
            }
        }
        #endregion

        public void ShowBackstageMenu()
        {
        }

        public void HideBackstageMenu()
        {
        }

        public bool MoveToTreeNode(string path)
        {
            return _appWindow.MoveToTreeNode(path);
        }
    }

    internal class ExplorerStatusBar : IStatusBar
    {
        private IFormExplorer _appWin = null;
        private bool _progressVisible = false;
        private int _progressValue = 0;

        public ExplorerStatusBar(IFormExplorer app)
        {
            _appWin = app;
        }

        #region IStatusBar Member

        public string Text
        {
            set
            {
                if (_appWin != null)
                {
                    _appWin.SetStatusbarText(value);
                }
            }
        }

        public System.Drawing.Image Image
        {
            set { }
        }
        public bool ProgressVisible
        {
            get
            {

                return _progressVisible;
            }
            set
            {
                if (_appWin != null && _progressVisible != value)
                {
                    _progressVisible = value;
                    _appWin.SetStatusbarProgressVisibility(value);
                }
            }
        }

        public int ProgressValue
        {
            get
            {
                return _progressValue;
            }
            set
            {
                if (value < 0 || value > 100)
                {
                    return;
                }

                if (_appWin != null && _progressValue != value)
                {
                    _progressValue = value;
                    _appWin.SetStatusbarProgressValue(value);
                }
            }
        }

        public void Refresh()
        {
            if (_appWin != null)
            {
                _appWin.RefreshStatusbar();
            }
        }
        #endregion
    }
}
