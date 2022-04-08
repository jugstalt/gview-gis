namespace gView.Framework.UI
{
    public interface IContextMenuItem : IOrder
    {
        bool ShowWith(object context);
    }

    
}