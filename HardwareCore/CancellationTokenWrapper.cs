using System.Threading;

namespace HardwareCore
{
    public class CancellationTokenWrapper
    {
        public CancellationToken Token {get; private set;}

        public CancellationTokenWrapper(CancellationToken token)
        {
            Token = token;
        }
    }
}