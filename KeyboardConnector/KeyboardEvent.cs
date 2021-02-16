namespace KeyboardConnector
{
    public class KeyboardEvent
    {
        public byte Status {get; private set;}
        public byte Data {get; private set;}
        public byte ScanCode {get; private set;}
        public KeyboardEvent()
        {
        }
        
        public KeyboardEvent(byte status, byte data, byte scanCode)
        {
            Status = status;
            Data = data;
            ScanCode = scanCode;
        }
    }
}
