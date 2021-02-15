namespace HardwareCore
{
    public class FifoBuffer<T> where T : new()
    {
        public int Size {get; private set;}
        public int WritePointer {get; private set;}
        public int ReadPointer {get; private set;}
        private bool _isFull = false;
        private T[] _buffer;
        public FifoBuffer(int size)
        {
            Size = size;
            _buffer = new T[Size];
            WritePointer = 0;
            ReadPointer = 0;
        }

        public bool Peek(out T result)
        {
            lock(this)
            {
                if(IsEmpty())
                {
                    result = default(T);
                    return false;
                }

                result = _buffer[ReadPointer];
                return true;
            }
        }

        public bool Read(out T result)
        {
            lock(this)
            {
                if(IsEmpty())
                {
                    result = default(T);
                    return false;
                }

                result = _buffer[ReadPointer];
                ReadPointer = (ReadPointer + 1) % Size;
                _isFull = false;
                return true;
            }
        }

        public bool Write(T value)
        {
            lock(this)
            {
                if(IsFull())
                {
                    return false;
                }
                
                _buffer[WritePointer] = value;
                WritePointer = (WritePointer + 1) % Size;
                _isFull = (WritePointer == ReadPointer);
                return true;
            }
        }

        public bool IsEmpty()
        {
            lock(this)
            {
                return ReadPointer == WritePointer;
            }
        }

        public bool IsFull()
        {
            lock(this)
            {
                return _isFull;
            }
        }

        public void Clear()
        {
            lock(this)
            {
                WritePointer = 0;
                ReadPointer = 0;
                _isFull = false;
            }
        }
    }
}