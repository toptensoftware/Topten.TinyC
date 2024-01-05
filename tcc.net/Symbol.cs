namespace tcc.net;

public class Symbol
{
    internal Symbol(string name, IntPtr value)
    {
        Name = name;
        Value = value;
    }
    public string Name
    {
        get;
        private set;
    }

    public IntPtr Value
    {
        get;
        private set;
    }
}
