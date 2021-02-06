namespace Debugger
{
    public class CpuLog
    {
        public ushort Address {get; private set;}
        public string Message {get; private set;}
        public CpuLog(ushort address, string message)
        {
            Address = address;
            Message = message;
        }
    }
}
