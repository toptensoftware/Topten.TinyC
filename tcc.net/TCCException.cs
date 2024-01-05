namespace tcc.net;

public class TCCException : Exception
{
    public TCCException(string message, int code) : base(message)
    {
        Code = code;
    }

    public int Code
    {
        get;
        private set;
    }
}
