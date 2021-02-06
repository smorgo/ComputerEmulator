namespace Debugger
{
    public interface ILogFormatter
    {
        void LogBytes(ushort startAddress, byte[] bytes);
        void LogWord(ushort address, ushort value);
        void LogRegister(string register, ushort value, string hexValue);
    }
}