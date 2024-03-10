using gView.Blazor.Models.Content;
using System.Collections.Generic;
using System.Linq;

namespace gView.Blazor.Models.Table;
public class TableItem : ContentItem
{
    private readonly List<string> _columns;
    private List<RowItem> _rows;

    public TableItem(IEnumerable<string> columns)
    {
        _columns = new List<string>(columns);
        _rows = new List<RowItem>();
    }

    public string[] Columns => _columns.ToArray();
    public IEnumerable<RowItem> Rows => _rows;

    public bool TryAddColumn(string column)
    {
        if(!_columns.Contains(column))
        {
            _columns.Add(column);
            
            return true;
        }

        return false;
    }

    public RowItem AddRow()
    {
        var row = new RowItem();
        _rows.Add(row);

        return row;
    }

    public void OrderBy(string column)
    {
        _rows = new List<RowItem>(_rows.OrderBy(r => r[column]));
    }
}
