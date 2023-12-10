/// <summary>
/// The <c>gView.Framework</c> provides all interfaces to develope
/// with and for gView
/// </summary>
namespace gView.Framework.Core.Common
{
    public interface IUserData
    {
        void SetUserData(string type, object val);
        void RemoveUserData(string type);
        object GetUserData(string type);
        string[] UserDataTypes { get; }
        void CopyUserDataTo(IUserData userData);
    }
}
