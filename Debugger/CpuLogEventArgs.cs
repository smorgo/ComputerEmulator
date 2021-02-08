namespace Debugger
{
    public class CpuLogEventArgs
    {
        public ushort Address {get; private set;}
        public string Message {get; private set;}
        public CpuLogEventArgs(ushort address, string message)
        {
            Address = address;
            Message = message;
        }
    }
}
