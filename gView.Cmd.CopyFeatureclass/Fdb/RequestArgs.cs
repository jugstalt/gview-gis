namespace gView.Cmd.CopyFeatureclass.Fdb;

public class RequestArgs
{
    private string _request;
    private MessageBoxButtons _buttons;
    private DialogResult _result;

    public RequestArgs(string request, MessageBoxButtons buttons, DialogResult result)
    {
        _request = request;
        _buttons = buttons;
        _result = result;
    }

    public MessageBoxButtons Buttons
    {
        get
        {
            return _buttons;
        }
    }

    public string Request
    {
        get { return _request; }
    }

    public DialogResult Result
    {
        get { return _result; }
        set { _result = value; }
    }
}
