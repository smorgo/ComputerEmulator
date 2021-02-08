using System.Collections.Generic;

namespace Debugger
{
    public interface ILabelMap
    {
        void Clear();
        Dictionary<string, ushort> LabelAddresses { get; }
        Dictionary<ushort, string> AddressLabels { get; }

        void Add(Label label);
        bool TryLookup(string name, out ushort address);
        bool TryLookup(ushort address, out string name);
    }
}