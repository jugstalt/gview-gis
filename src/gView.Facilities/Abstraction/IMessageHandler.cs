namespace gView.Facilities.Abstraction;
public interface IMessageHandler
{
    Task InvokeAsync(string message);
}
