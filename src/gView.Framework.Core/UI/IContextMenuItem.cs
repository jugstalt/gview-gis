namespace gView.Framework.Core.UI
{
    public interface IContextMenuItem : IOrder
    {
        bool ShowWith(object context);
    }


}