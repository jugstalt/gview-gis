#nullable enable

using gView.Framework.Core.Data;
using gView.GeoJsonService.DTOs;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace gView.Server.EndPoints.GeoJsonService.Extensions;

internal static class FieldExtensions
{
    /// <summary>
    /// A field as layer property, as in the capabilities, plus length, nullable, editable and the domain
    /// </summary>
    static public async Task<LayerProperty> ToLayerPropertyAsync(this IField field, bool layerEditable)
    {
        var property = new LayerProperty()
        {
            Name = field.name,
            Aliasname = String.IsNullOrEmpty(field.aliasname) ? field.name : field.aliasname,
            Length = field.size > 0 ? field.size : null,
            Nullable = field.type != FieldType.ID && field.IsNullable && !field.IsRequired,
            Editable = layerEditable && field.type != FieldType.ID && field.IsEditable && !field.name.Contains('('),
            Domain = await field.Domain.ToPropertyDomainAsync()
        };

        property.Type = field.type.ToPropertyType();
        if (field.type == FieldType.ID)
        {
            property.IsPrimaryKey = true;
        }

        return property;
    }

    static private async Task<PropertyDomain?> ToPropertyDomainAsync(this IFieldDomain? domain)
        => domain switch
        {
            IRangeFieldDomain range => new PropertyDomain() { Type = "range", Min = range.MinValue, Max = range.MaxValue },
            IDictionaryFieldDomain dictionary when dictionary.Dictionary is not null
                => new PropertyDomain() { Type = "dictionary", Dictionary = dictionary.Dictionary.ToDictionary(e => e.Key, e => (object?)e.Value) },
            IValuesFieldDomain values => new PropertyDomain() { Type = "values", Values = (await values.ValuesAsync())?.ToArray() ?? [] },
            _ => null
        };
}
