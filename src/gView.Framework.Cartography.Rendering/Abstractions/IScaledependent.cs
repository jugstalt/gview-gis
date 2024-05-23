namespace gView.Framework.Cartography.Rendering.Abstractions
{
    public interface IScaledependent
    {
        double MinimumScale { get; set; }
        double MaximumScale { get; set; }
    }
}
