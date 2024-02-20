using gView.DataExplorer.Plugins.ExplorerObjects.Base;
using gView.DataExplorer.Plugins.Extensions;
using gView.DataExplorer.Razor.Components.Dialogs.Models;
using gView.DataExplorer.Razor.Components.Dialogs.Models.Extensions;
using gView.Framework.Blazor.Services.Abstraction;
using gView.Framework.Core.Common;
using gView.Framework.DataExplorer.Abstraction;
using gView.Framework.DataExplorer.Events;
using gView.Framework.IO;
using System.Threading.Tasks;
using gView.Framework.Common.Extensions;
using gView.Framework.DataExplorer.Services.Abstraction;
using gView.DataExplorer.Core.Extensions;

namespace gView.DataExplorer.Plugins.ExplorerObjects.GeoJson
{
    [RegisterPlugIn("DAF698C1-54F2-4199-A145-A7A911B882DF")]
    public class GeoJsonServiceNewConnectionObject : ExplorerObjectCls<GeoJsonServiceGroupObject>,
                                                     IExplorerSimpleObject,
                                                     IExplorerObjectDoubleClick,
                                                     IExplorerObjectCreatable
    {

        public GeoJsonServiceNewConnectionObject()
            : base()
        {
        }

        public GeoJsonServiceNewConnectionObject(GeoJsonServiceGroupObject parent)
            : base(parent, 1)
        {
        }

        #region IExplorerSimpleObject Members

        public string Icon => "basic:round-plus";

        #endregion

        #region IExplorerObject Members

        public string Name => "New Connection...";

        public string FullName => "";

        public string Type => "New GeoJson Service Connection";

        public void Dispose()
        {

        }

        public Task<object?> GetInstanceAsync() => Task.FromResult<object?>(null);

        #endregion

        #region IExplorerObjectDoubleClick Members

        async public Task ExplorerObjectDoubleClick(IExplorerApplicationScopeService appScope, ExplorerObjectEventArgs e)
        {
            var model = await appScope.ShowModalDialog(typeof(gView.DataExplorer.Razor.Components.Dialogs.GeoJsonConnectionDialog),
                                                                    "GeoJson Connection",
                                                                    new GeoJsonConnectionModel());

            if (model != null)
            {
                string connectionString = model.ToConnectionString();

                string name = connectionString.ExtractConnectionStringParameter("target");

                ConfigConnections config = ConfigConnections.Create(
                        this.ConfigStorage(),
                        GeoJsonServiceGroupObject.ConfigName, 
                        GeoJsonServiceGroupObject.EncKey
                    );
                config.Add(name, connectionString);
                e.NewExplorerObject = new GeoJsonServiceExplorerObject(this.TypedParent, name, connectionString);
            }
        }

        #endregion

        #region ISerializableExplorerObject Member

        public Task<IExplorerObject?> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache? cache)
        {
            if (cache?.Contains(FullName) == true)
            {
                return Task.FromResult<IExplorerObject?>(cache[FullName]);
            }

            return Task.FromResult<IExplorerObject?>(null);
        }

        #endregion

        #region IExplorerObjectCreatable Member

        public bool CanCreate(IExplorerObject parentExObject)
        {
            return (parentExObject is GeoJsonServiceGroupObject);
        }

        async public Task<IExplorerObject?> CreateExplorerObjectAsync(IExplorerApplicationScopeService appScope, IExplorerObject parentExObject)
        {
            ExplorerObjectEventArgs e = new ExplorerObjectEventArgs();
            await ExplorerObjectDoubleClick(appScope, e);
            return e.NewExplorerObject;
        }

        #endregion
    }
}
