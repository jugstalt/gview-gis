namespace gView.Framework.Network
{
    public interface ISwitch
    {
        int NodeId { get; }
        bool SwitchState { get; }
    }
}
