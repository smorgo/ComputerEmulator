namespace HardwareCore
{
    public abstract class MockEventBase
    {
        private bool _initialState;
        public int SetCount {get; protected set;}
        public int ResetCount {get; protected set;}
        public bool IsSet {get; protected set;} = true;
        public MockEventBase(bool initialStateSet)
        {
            IsSet = initialStateSet;
            _initialState = initialStateSet;
        }
        public void Init()
        {
            IsSet = _initialState;
            ResetCount = 0;
            SetCount = 0;
        }
        public void Reset()
        {
            IsSet = false;
            ResetCount++;
        }
        public void Set()
        {
            IsSet = true;
            SetCount++;
        }
        public void WaitOne()
        {
        }
    }
}