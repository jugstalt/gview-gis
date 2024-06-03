using System;

namespace gView.Framework.Core.Data
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ImportFeatureClassNameWithSchemaAttribute : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ImportFeaturesBufferSizeAttribute : Attribute
    {
        public ImportFeaturesBufferSizeAttribute(int bufferSize)
        {
            BufferSize = bufferSize;
        }
        public int BufferSize { get; set; }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class MaximumFieldnameLength : Attribute
    {
        private int _max;
        public MaximumFieldnameLength(int maxLength)
        {
            _max = maxLength;
        }
        public int Value
        {
            get { return _max; }
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class UseDatasetNameCaseAttribute : Attribute
    {
        private DatasetNameCase _case;
        public UseDatasetNameCaseAttribute(DatasetNameCase nameCase)
        {
            _case = nameCase;
        }
        public DatasetNameCase Value
        {
            get { return _case; }
        }
    }
}