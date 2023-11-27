namespace gView.Framework.Core.Network
{
    public interface ISwitch
    {
        int NodeId { get; }
        bool SwitchState { get; }
    }
}
