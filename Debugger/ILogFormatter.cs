namespace Debugger
{
    public interface ILogFormatter
    {
        void LogByte(byte value);
        void LogBytes(ushort startAddress, byte[] bytes);
        void LogWord(ushort address, ushort value);
        void LogRegister(string register, ushort value, string hexValue);
        void LogError(string message);
        void Log(string message);
    }
}