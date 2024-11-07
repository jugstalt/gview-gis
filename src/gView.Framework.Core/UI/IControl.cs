namespace gView.Framework.Core.UI
{
    public interface IControl
    {
        void OnShowControl(object hook);
        void UnloadControl();
    }


}