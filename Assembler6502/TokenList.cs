using System.Collections;
using System.Collections.Generic;

namespace Assembler6502
{
    public class TokenList : IList<Token>
    {
        private Token _eofToken = new EofToken();
        private int _cursor = 0;
        private int _peekCursor = 0;
        private List<Token> _items = new List<Token>();
        
        public TokenList()
        {
        }

        public TokenList(IList<Token> sourceTokens)
        {
            _items.AddRange(sourceTokens);
        }

        public Token this[int index] 
        { 
            get
            {
                if(index >= _items.Count)
                {
                    return _eofToken;
                }

                return _items[index];
            }

            set => throw new System.NotImplementedException(); 
        }

        public int Count => _items.Count;

        public bool IsReadOnly => false;

        public Token Take()
        {
            var result = this[_cursor];
            _cursor++;
            _peekCursor = _cursor;
            return result;
        }

        public Token Peek()
        {
            var result = this[_peekCursor];
            _peekCursor++;
            return result;
        }
        public void Add(Token item)
        {
            _items.Add(item);
        }

        public void Clear()
        {
            _items.Clear();
        }

        public bool Contains(Token item)
        {
            return _items.Contains(item);
        }

        public void CopyTo(Token[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        public IEnumerator<Token> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        public int IndexOf(Token item)
        {
            return _items.IndexOf(item);
        }

        public void Insert(int index, Token item)
        {
            throw new System.NotImplementedException();
        }

        public bool Remove(Token item)
        {
            throw new System.NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new System.NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_items).GetEnumerator();
        }
    }

}