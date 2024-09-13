namespace gView.Framework.Core.Common;

public interface IClone
{
    object Clone();
}

public interface IClone<T>
{
    T Clone();
}

public interface ICopy
{
    object Copy();
}
