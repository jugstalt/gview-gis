using gView.Framework.Core.IO;

/// <summary>
/// The <c>gView.Framework</c> provides all interfaces to develope
/// with and for gView
/// </summary>
namespace gView.Framework.Core.Common
{
    public interface ISimpleNumberCalculation : IPersistable
    {
        string Name { get; }
        string Description { get; }

        double Calculate(double val);
    }
}
