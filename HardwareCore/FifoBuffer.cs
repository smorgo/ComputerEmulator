namespace HardwareCore
{
    public class FifoBuffer<T> where T : new()
    {
        public int Size {get; private set;}
        public int WritePointer {get; private set;}
        public int ReadPointer {get; private set;}
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
                if(WritePointer == ReadPointer)
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
                if(WritePointer == ReadPointer)
                {
                    result = default(T);
                    return false;
                }

                result = _buffer[ReadPointer];
                ReadPointer = (ReadPointer + 1) % Size;
                return true;
            }
        }

        public bool Write(T value)
        {
            lock(this)
            {
                if(((WritePointer + Size + 1) % Size) == ReadPointer)
                {
                    return false;
                }
                
                _buffer[WritePointer] = value;
                WritePointer = (WritePointer + 1) % Size;
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
                // Is WritePointer up against ReadPointer?
                return ((WritePointer + Size + 1) % Size) == ReadPointer;
            }
        }

        public void Clear()
        {
            lock(this)
            {
                WritePointer = 0;
                ReadPointer = 0;
            }
        }
    }
}