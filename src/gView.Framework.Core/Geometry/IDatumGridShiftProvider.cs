namespace gView.Framework.Core.Geometry;

public interface IDatumGridShiftProvider
{
    (string shortName, string name)[] GridShiftNames();

    (string shortName, string name)[] EllipsoidNames();

    string GridParameter(string shiftShortName, string ellipsoidShortName);
}
