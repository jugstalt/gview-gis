using gView.Framework.Core.Data;
using gView.Framework.Core.Extensions;

namespace gView.Framework.Core.Tests.Extensions;

public class DatasetNameCaseExtensionsTests
{
    [Theory]
    [InlineData(DatasetNameCase.ignore, "MyTable")]
    [InlineData(DatasetNameCase.upper, "MYTABLE")]
    [InlineData(DatasetNameCase.lower, "mytable")]
    [InlineData(DatasetNameCase.classNameUpper, "MYTABLE")]
    [InlineData(DatasetNameCase.classNameLower, "mytable")]
    [InlineData(DatasetNameCase.fieldNamesUpper, "MyTable")]
    [InlineData(DatasetNameCase.fieldNamesLower, "MyTable")]
    public void ToClassName_AppliesOnlyClassNameRules(DatasetNameCase nameCase, string expected)
    {
        var result = nameCase.ToClassName("MyTable");

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(DatasetNameCase.ignore, "A_Date")]
    [InlineData(DatasetNameCase.upper, "A_DATE")]
    [InlineData(DatasetNameCase.lower, "a_date")]
    [InlineData(DatasetNameCase.classNameUpper, "A_Date")]
    [InlineData(DatasetNameCase.classNameLower, "A_Date")]
    [InlineData(DatasetNameCase.fieldNamesUpper, "A_DATE")]
    [InlineData(DatasetNameCase.fieldNamesLower, "a_date")]
    public void ToFieldName_AppliesOnlyFieldNameRules(DatasetNameCase nameCase, string expected)
    {
        var result = nameCase.ToFieldName("A_Date");

        Assert.Equal(expected, result);
    }

    [UseDatasetNameCase(DatasetNameCase.fieldNamesUpper)]
    private class DatasetWithNameCase { }

    private class DerivedDataset : DatasetWithNameCase { }

    private class DatasetWithoutNameCase { }

    [Theory]
    [InlineData(typeof(DatasetWithNameCase), DatasetNameCase.fieldNamesUpper)]
    [InlineData(typeof(DerivedDataset), DatasetNameCase.fieldNamesUpper)]
    [InlineData(typeof(DatasetWithoutNameCase), DatasetNameCase.ignore)]
    public void GetDatasetNameCase_ReadsAttributeIncludingInherited(Type type, DatasetNameCase expected)
    {
        var result = type.GetDatasetNameCase();

        Assert.Equal(expected, result);
    }
}
