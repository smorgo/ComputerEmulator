namespace RemoteDisplayConnector
{
    public class CursorPosition
    {
        public byte X {get; private set;}
        public byte Y {get; private set;}

        public CursorPosition(byte x, byte y)
        {
            X = x;
            Y = y;
        }
    }
}
