using System;
using System.Collections.Generic;

namespace _6502
{
    public class Loader
    {
        private Dictionary<string, ushort> _labels;
        private Dictionary<ushort, string> _labelReferences;
        public ushort Cursor {get; private set;}
        private AddressMap _addressMap;

        public Loader(AddressMap addressMap, ushort cursor)
        {
            _addressMap = addressMap;
            Cursor = cursor;
            _labels = new Dictionary<string, ushort>();
            _labelReferences = new Dictionary<ushort, string>();
        }

        public Loader Write(ushort address, byte value, string label = null)
        {
            AddLabel(label, address);
            _addressMap.Write(address, value);
            Cursor = (ushort)(address + 1);
            return this;
        }

        public Loader Write(byte value, string label = null)
        {
            return Write(Cursor, value, label);
        }

        public Loader Write(ushort address, CPU6502.OPCODE opcode, string label = null)
        {
            return Write(address, (byte)opcode, label);
        }
        
        public Loader Write(CPU6502.OPCODE opcode, string label = null)
        {
            return Write((byte)opcode, label);
        }
        public Loader Write(ushort address, int value, string label = null)
        {
            return Write(address, (byte)value, label);
        }
        
        public Loader Write(int value, string label = null)
        {
            return Write((byte)value, label);
        }

        public Loader WriteWord(ushort address, ushort value, string label = null)
        {
            AddLabel(label, address);
            _addressMap.WriteWord(address, value);
            Cursor = (ushort)(address + 2);
            return this;
        }

        public Loader WriteWord(ushort value, string label = null)
        {
            return WriteWord(Cursor, value, label);
        }

        public Loader RelativeAddress(int offset, string label = null)
        {
            AddLabel(label, Cursor);
            _addressMap.WriteWord(Cursor, (ushort)(Cursor + offset));
            Cursor += 2;
            return this;
        }

        public Loader From(ushort address)
        {
            Cursor = address;
            return this;
        }

        public Loader Ref(string label)
        {
            _labelReferences.Add(Cursor, label);
            return WriteWord(0xffff);
        }

        public Loader Ref(ushort address, string label)
        {
            _labelReferences.Add(address, label);
            return WriteWord(address, 0xffff);
        }

        private void AddLabel(string label, ushort address)
        {
            if(!string.IsNullOrWhiteSpace(label))
            {
                if(_labels.ContainsKey(label))
                {
                    Console.WriteLine($"Label '{label}' already defined");
                }
                else
                {
                    _labels.Add(label, address);
                }
            }
        }

        private void AddLabel(string label)
        {
            AddLabel(label, Cursor);
        }

        public Loader Fixup()
        {
            foreach(var reference in _labelReferences)
            {
                if(_labels.ContainsKey(reference.Value))
                {
                    _addressMap.WriteWord(reference.Key, _labels[reference.Value]);
                }
                else
                {
                    Console.WriteLine($"Error: 0x{reference.Key:X4} Label '{reference.Value} is not defined");
                }
            }

            return this;
        }
    }
}