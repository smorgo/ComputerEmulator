namespace Debugger
{
    public interface IBreakpoint
    {
        string Type {get;}
        string Description {get;}
        bool Disabled {get;set;}
    }

}