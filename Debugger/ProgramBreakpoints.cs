using System;
using System.Collections;
using System.Collections.Generic;

namespace Debugger
{
    public class ProgramBreakpoints : IList<ProgramBreakpoint>
    {
        private int _nextId = 1;
        private List<ProgramBreakpoint> _items = new List<ProgramBreakpoint>();
        public ProgramBreakpoint this[int index] 
        { 
            get
            {
                return _items[index];
            }
            
            set
            {
                throw new InvalidOperationException("Use Add to modify ProgramBreakpoints");
            }
        }

        public int Count => _items.Count;

        public bool IsReadOnly => true;

        public void Add(ProgramBreakpoint item)
        {
            item.Id = _nextId++;
            _items.Add(item);
        }

        public void Clear()
        {
            _items.Clear();
            _nextId = 1;
        }

        public bool Contains(ProgramBreakpoint item)
        {
            return _items.Contains(item);
        }

        public void CopyTo(ProgramBreakpoint[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        public IEnumerator<ProgramBreakpoint> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        public int IndexOf(ProgramBreakpoint item)
        {
            return _items.IndexOf(item);
        }

        public void Insert(int index, ProgramBreakpoint item)
        {
            throw new NotImplementedException();
        }

        public bool Remove(ProgramBreakpoint item)
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