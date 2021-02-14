namespace Debugger
{
    public interface IParser
    {
        void Parse(string command);
        RunMode RunMode {get;}
    }
}