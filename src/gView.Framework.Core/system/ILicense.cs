/// <summary>
/// The <c>gView.Framework</c> provides all interfaces to develope
/// with and for gView
/// </summary>
namespace gView.Framework.Core.system
{
    public interface ILicense
    {
        LicenseTypes ComponentLicenseType(string componentName);

        string ProductID
        {
            get;
        }
        LicenseTypes LicenseType
        {
            get;
        }
        string ProductName
        {
            get;
        }

        string LicenseFile { get; }
        bool LicenseFileExists { get; }
        string[] LicenseComponents { get; }
    }
}
