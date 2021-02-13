namespace Debugger
{
    public interface IBreakpoint
    {
        string Type {get;}
        string Describe(ILabelMap labels);
        bool Disabled {get;set;}
    }

}