using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace HardwareCore
{
    public class LoaderLabelTable : ILoaderLabelTable
    {
        private List<Dictionary<string, ushort>> _scopes;
        private int _currentScope;
        public int CurrentScope => _currentScope;
        public LoaderLabelTable()
        {
            Clear();
        }

        public void Clear()
        {
            _scopes = new List<Dictionary<string, ushort>>();
            _scopes.Add(new Dictionary<string, ushort>());
            _currentScope = 0;
        }

        public void Push()
        {
            _scopes.Add(new Dictionary<string, ushort>());
            _currentScope++;
        }

        public void Pop()
        {
            if (_currentScope == 0)
            {
                // Can't remove the last scope, as Pops will exceed Pushes
                // Do a Clear() if you need to wipe everything out
                return;
            }

            _scopes.RemoveAt(_currentScope);
            _currentScope--;
        }

        public bool Add(string label, ushort address)
        {
            if (!_scopes[_currentScope].ContainsKey(label))
            {
                _scopes[_currentScope].Add(label, address);
                return true;
            }
            else
            {
                Debug.WriteLine($"Duplicate label: {label}");
                return false;
            }
        }

        public bool TryResolve(string label, out ushort address)
        {
            for (var ix = _currentScope; ix >= 0; ix--)
            {
                if (_scopes[ix].ContainsKey(label))
                {
                    address = _scopes[ix][label];
                    return true;
                }
            }

            address = 0x0000;
            return false;
        }

        public ushort Resolve(string label)
        {
            ushort result;

            if (!TryResolve(label, out result))
            {
                throw new KeyNotFoundException($"Label {label} not resolved by LabelTable");
            }

            return result;
        }

        public List<string> Names
        {
            get
            {
                var result = _scopes.SelectMany(x => x.Keys).Distinct().ToList();

                return result;
            }
        }
    }
}
