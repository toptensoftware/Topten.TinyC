namespace Topten.TinyC;

public class TinyCException : Exception
{
    public TinyCException(string message, int code) : base(message)
    {
        Code = code;
    }

    public int Code
    {
        get;
        private set;
    }
}
