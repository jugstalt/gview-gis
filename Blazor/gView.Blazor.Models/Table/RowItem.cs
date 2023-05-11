using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Blazor.Models.Table;
public class RowItem
{
    private readonly Dictionary<string, object?> _data;
    public RowItem()
    {
        _data = new Dictionary<string, object?>();
    }

    public string? Icon { get; set; }

    public IDictionary<string, object> Data => _data;

    public RowItem AddData(string key, object? value)
    {
        ((Dictionary<string, object?>)_data).Add(key, value);

        return this;
    }

    public bool HasColumn(string columnName) => _data.ContainsKey(columnName);

    public object? this[string column]
        => this.HasColumn(column) ? _data[column] : null;
}
