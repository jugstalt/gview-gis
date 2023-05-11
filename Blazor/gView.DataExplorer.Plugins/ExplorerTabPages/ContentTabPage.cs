using gView.Blazor.Models.Content;
using gView.Blazor.Models.Table;
using gView.Framework.DataExplorer.Abstraction;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace gView.DataExplorer.Plugins.ExplorerTabPages;

[gView.Framework.system.RegisterPlugIn("47328671-87F7-4cf7-B59D-2BC111A29BA3")]
public class ContentTabPage : IExplorerTabPage
{
    private IExplorerObject? _exObject;

    #region IExplorerTabPage

    public Type RazorComponent => typeof(Razor.Components.Content.TabPageConent);

    public string Title => "Content";

    public IExplorerObject GetExplorerObject()
    {
        throw new NotImplementedException();
    }

    public void OnCreate(object hook)
    {
        throw new NotImplementedException();
    }

    public void OnHide()
    {
        throw new NotImplementedException();
    }

    public Task<bool> OnShow()
    {
        throw new NotImplementedException();
    }

    async public Task<ContentItemResult> RefreshContents()
    {
        var table = new TableItem(new[] { "Name", "Type" });

        if (_exObject is IExplorerParentObject)
        {
            List<IExplorerObject> childs = await((IExplorerParentObject)_exObject).ChildObjects();

            if (childs != null)
            {
                foreach(var child in childs)
                {
                    table.AddRow()
                        .AddData("Name", child.Name)
                        .AddData("Type", child.Type)
                        .Icon = child.Icon;
                }
            }
        }

        return new ContentItemResult() { Item = table };
    }

    public Task SetExplorerObjectAsync(IExplorerObject value)
    {
        _exObject = value;

        return Task.CompletedTask;
    }

    public Task<bool> ShowWith(IExplorerObject exObject)
    {
        return Task.FromResult(exObject is IExplorerParentObject);
    }

    #endregion

    #region IOrder
    public int SortOrder => 0;

    #endregion
}
