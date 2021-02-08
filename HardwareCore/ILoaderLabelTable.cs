using System.Collections.Generic;

namespace HardwareCore
{
    public interface ILoaderLabelTable
    {
        List<string> Names { get; }

        bool Add(string label, ushort address);
        void Clear();
        void Pop();
        void Push();
        ushort Resolve(string label);
        bool TryResolve(string label, out ushort address);
    }
}