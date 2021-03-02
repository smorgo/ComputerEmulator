using System;

namespace HardwareCore
{
    public interface ILoader : IDisposable
    {
        ushort Cursor { get; set; }
        bool HasErrors { get; }
        Loader Fixup(out ILoaderLabelTable exportLabels);
        Loader Fixup();
        Loader From(ushort address);
        Loader Label(ushort address, string label);
        Loader Label(string label);
        Loader Macro(ushort address, Action<ushort, Loader> macro, string label = null);
        Loader Macro(Action<ushort, Loader> macro, string label = null);
        Loader Ref(string label, int offset = 0);
        Loader Ref(ushort address, string label, int offset = 0);
        Loader RefByte(string label, int offset = 0);
        Loader RefByte(ushort address, string label, int offset = 0);
        Loader RelativeAddress(int offset, string label = null);
        Loader RelativeRef(string label);
        Loader RelativeRef(ushort address, string label);
        Loader Write(ushort address, byte value, string label = null);
        Loader Write(byte value, string label = null);
        Loader Write(ushort address, OPCODE opcode, string label = null);
        Loader Write(OPCODE opcode, string label = null);
        Loader Write(ushort address, int value, string label = null);
        Loader Write(int value, string label = null);
        Loader WriteString(ushort address, string content, string label = null);
        Loader WriteString(string content, string label = null);
        Loader WriteWord(ushort address, ushort value, string label = null);
        Loader WriteWord(ushort value, string label = null);
        Loader ZeroPageRef(string label, int offset = 0);
        Loader ZeroPageRef(ushort address, string label, int offset = 0);
        bool TryResolveLabel(string label, out ushort address);
    }
}