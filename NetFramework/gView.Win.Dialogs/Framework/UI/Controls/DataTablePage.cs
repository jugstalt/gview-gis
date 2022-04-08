using gView.Framework.Data;
using gView.Framework.system;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gView.Framework.UI.Controls
{
    [gView.Framework.system.RegisterPlugIn("62661EF5-2B98-4189-BFCD-7629476FA91C")]
    public class DataTablePage : IExplorerTabPage
    {
        gView.Framework.UI.Dialogs.FormDataTable _table = null;
        IExplorerObject _exObject = null;

        #region IExplorerTabPage Members

        public Control Control
        {
            get
            {
                if (_table == null)
                {
                    _table = new gView.Framework.UI.Dialogs.FormDataTable(null);
                }

                _table.StartWorkerOnShown = false;
                _table.ShowExtraButtons = false;

                return _table;
            }
        }

        public void OnCreate(object hook)
        {
        }

        async public Task<bool> OnShow()
        {
            OnHide();
            if (_exObject == null)
            {
                return false;
            }

            var instance = await _exObject.GetInstanceAsync();
            if (instance is ITableClass && _table != null)
            {
                _table.TableClass = (ITableClass)instance;
                _table.StartWorkerThread();
            }

            return true;
        }
        public void OnHide()
        {
            //MessageBox.Show("get hidden...");
            if (_table != null)
            {
                _table.TableClass = null;
            }
        }

        public IExplorerObject GetExplorerObject()
        {
            return _exObject;
        }

        public Task SetExplorerObjectAsync(IExplorerObject value)
        {
            _exObject = value;
            //await OnShow();

            return Task.CompletedTask;
        }


        public Task<bool> ShowWith(IExplorerObject exObject)
        {
            if (exObject == null)
            {
                return Task.FromResult(false);
            }

            if (TypeHelper.Match(exObject.ObjectType, typeof(ITableClass)))
            {
                //if (TypeHelper.Match(exObject.ObjectType, typeof(INetworkClass)) &&
                //   ((IFeatureClass)exObject.Object).GeometryType == gView.Framework.Geometry.geometryType.Network)
                //    return false;

                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public string Title
        {
            get { return "Data Table"; }
        }

        public Task<bool> RefreshContents()
        {
            return Task.FromResult(true);
        }
        #endregion

        #region IOrder Members

        public int SortOrder
        {
            get { return 20; }
        }

        #endregion
    }
}
