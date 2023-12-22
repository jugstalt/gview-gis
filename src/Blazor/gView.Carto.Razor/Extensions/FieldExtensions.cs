using gView.Framework.Core.Data;

namespace gView.Carto.Razor.Extensions;

static internal class FieldExtensions
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

    static public string FieldWhereClauseSegment(this IField field, string value)
    {
        var formatString = field.FieldValueFormatString();

        var queryOperator = field.type switch
        {
            FieldType.String /*when value.Contains("%")*/ => " like ",  // aways like... also solves case insensiv
            FieldType.NString /*when value.Contains("%")*/ => " like ",
            _ => "="
        };

        return $"{field.name}{queryOperator}{String.Format(formatString, value)}";
    }
}
