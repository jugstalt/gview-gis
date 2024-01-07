using gView.Framework.Core.Data;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace gView.Blazor.Core.Extensions;

static public class FieldExtensions
{
    static public string FieldValueFormatString(this IField field)
        => field.type switch
        {
            FieldType.Date => "'{0}'",
            FieldType.character => "'{0}'",
            FieldType.guid => "'{0}'",
            FieldType.NString => "'{0}'",
            FieldType.String => "'{0}'",
            _ => "{0}"
        };

    static private string[] KnownOperators
        = ["=", "<>", ">=","<=", "<", ">"];

    static public string FieldWhereClauseSegment(
            this IField field, 
            string value, 
            IDatasetCapabilities? datasetCapabilities = null
        )
    {
        var formatString = field.FieldValueFormatString();
        string? queryOperator = null;

        var knownOperator = KnownOperators
            .Where(o => value.StartsWith(o, StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault();

        if(knownOperator is not null)
        {
            queryOperator = knownOperator;
            value = value.Substring(queryOperator.Length).Trim();
        }

        queryOperator = queryOperator ?? field.type switch
        {
            FieldType.String /*when value.Contains("%")*/ => $" {datasetCapabilities.GetLikeOperator()} ",  // aways like... also solves case insensiv
            FieldType.NString /*when value.Contains("%")*/ => $" {datasetCapabilities.GetLikeOperator()} ",
            _ => "="
        };

        return $"{field.name}{queryOperator}{String.Format(formatString, value)}";
    }

    static public bool IsDataTableField(this IField field)
        => field.type switch 
        {
            FieldType.GEOMETRY => false,
            FieldType.GEOGRAPHY => false,
            FieldType.binary => false,
            FieldType.Shape => false,
            _ => true
        };

    static private string GetLikeOperator(this IDatasetCapabilities? datasetCapabilities)
        => datasetCapabilities switch
        {
            null => "like",
            _ => datasetCapabilities.CaseInsensitivLikeOperator
        };
}
