using System.Threading;

namespace HardwareCore
{
    public class CancellationTokenWrapper
    {
        public CancellationTokenSource Source {get; private set;}
        public CancellationToken Token {get; private set;}

        public CancellationTokenWrapper()
        {
            Reset();
        }

        public void Reset()
        {
            Source = new CancellationTokenSource();
            Token = Source.Token;
        }
    }
}