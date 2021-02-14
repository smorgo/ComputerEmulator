using System;
using System.Collections;
using System.Collections.Generic;

namespace Debugger
{
    public class MemoryBreakpoints : IList<MemoryBreakpoint>
    {
        private int _nextId = 1;
        private List<MemoryBreakpoint> _items = new List<MemoryBreakpoint>();
        public MemoryBreakpoint this[int index] 
        { 
            get
            {
                return _items[index];
            }
            
            set
            {
                throw new InvalidOperationException("Use Add to modify MemoryBreakpoints");
            }
        }

        public int Count => _items.Count;

        public bool IsReadOnly => true;

        public void Add(MemoryBreakpoint item)
        {
            item.Id = _nextId++;
            _items.Add(item);
        }

        public void Clear()
        {
            _items.Clear();
            _nextId = 1;
        }

        public bool Contains(MemoryBreakpoint item)
        {
            return _items.Contains(item);
        }

        public void CopyTo(MemoryBreakpoint[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        public IEnumerator<MemoryBreakpoint> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        public int IndexOf(MemoryBreakpoint item)
        {
            return _items.IndexOf(item);
        }

        public void Insert(int index, MemoryBreakpoint item)
        {
            throw new NotImplementedException();
        }

        public bool Remove(MemoryBreakpoint item)
        {
            return _items.Remove(item);
        }

        public void RemoveAt(int index)
        {
            _items.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }
    }
}