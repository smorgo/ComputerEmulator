using System;
using System.Collections.Generic;

namespace HardwareCore
{

    public class Loader : IDisposable
    {
        public class ReferenceDescriptor
        {
            public string Label {get; private set;}
            public bool Relative {get; private set;}
            public bool ZeroPage {get; private set;}
            public int Offset {get; private set;}
            public ReferenceDescriptor(string label, bool relative = false, bool zeroPage = false, int offset = 0)
            {
                Label = label;
                Relative = relative;
                ZeroPage = zeroPage;
                Offset = offset;
            }
        }

        private bool _fixupRequired = false;
        private LabelTable _labels;
        private Dictionary<ushort, ReferenceDescriptor> _labelReferences;
        public ushort Cursor {get; private set;}
        private AddressMap _addressMap;

        public Loader(AddressMap addressMap, ushort cursor, LabelTable labels)
        {
            _addressMap = addressMap;
            Cursor = cursor;
            _labels = labels;
            _labelReferences = new Dictionary<ushort, ReferenceDescriptor>();
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

        public Loader Write(ushort address, OPCODE opcode, string label = null)
        {
            return Write(address, (byte)opcode, label);
        }
        
        public Loader Write(OPCODE opcode, string label = null)
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

        public Loader WriteString(ushort address, string content, string label = null)
        {
            AddLabel(label, address);
            foreach(var ch in content)
            {
                Write((byte)ch);
            }
            return this;
        }

        public Loader WriteString(string content, string label = null)
        {
            return WriteString(Cursor, content, label);
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

        public Loader Ref(string label, int offset = 0)
        {
            _fixupRequired = true;
            return Ref(Cursor, label, offset);
        }

        public Loader Ref(ushort address, string label, int offset = 0)
        {
            _fixupRequired = true;
            _labelReferences.Add(address, new ReferenceDescriptor(label, offset: offset));
            return WriteWord(address, 0xffff);
        }

        public Loader ZeroPageRef(string label, int offset = 0)
        {
            _fixupRequired = true;
            if(offset != 0)
            {
                Console.WriteLine("!");
            }
            return ZeroPageRef(Cursor, label, offset);
        }

        public Loader ZeroPageRef(ushort address, string label, int offset = 0)
        {
            _fixupRequired = true;
            _labelReferences.Add(address, new ReferenceDescriptor(label, false, true, offset));
            return Write(address, 0xff);
        }

        public Loader RelativeRef(string label)
        {
            _fixupRequired = true;
            return RelativeRef(Cursor, label);
        }

        public Loader RelativeRef(ushort address, string label)
        {
            _fixupRequired = true;
            _labelReferences.Add(address, new ReferenceDescriptor(label, true));
            return Write(address, 0xff);
        }

        private void AddLabel(string label, ushort address)
        {
            _fixupRequired = true;
            if(!string.IsNullOrWhiteSpace(label))
            {
                if(!_labels.Add(label, address))
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

        public Loader Fixup(out LabelTable exportLabels)
        {
            Fixup();
            exportLabels = _labels;
            return this;
        }

        public Loader Fixup()
        {
            ushort labelAddress;

            foreach(var reference in _labelReferences)
            {
                if(_labels.TryResolve(reference.Value.Label, out labelAddress))
                {
                    if(reference.Value.Relative)
                    {
                        short relAddress = (short)(labelAddress - reference.Key -1);
                        _addressMap.Write(reference.Key, (byte)relAddress);
                    }
                    else if(reference.Value.ZeroPage)
                    {
                        var addressToUse = labelAddress + reference.Value.Offset;

                        if(addressToUse >= 0 && addressToUse < 0x100)
                        {
                            _addressMap.Write(reference.Key, (byte)addressToUse);
                        }
                        else
                        {
                            Console.WriteLine($"FIXUP ERROR at ${reference.Key:X4} - label '{reference.Value.Label}' at ${addressToUse:X4} is not a ZeroPage address");
                        }
                    }
                    else
                    {
                        var offsetAddress = labelAddress + reference.Value.Offset;

                        if(offsetAddress >= 0 && offsetAddress < 0x10000)
                        {
                            _addressMap.WriteWord(reference.Key, (ushort)offsetAddress );
                        }
                        else
                        {
                            Console.WriteLine($"FIXUP ERROR at ${reference.Key:X4} - label '{reference.Value.Label}' at ${offsetAddress:X8} is not valid");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"Error: 0x{reference.Key:X4} Label '{reference.Value} is not defined");
                }
            }

            _fixupRequired = false;
            return this;
        }

        public void Dispose()
        {
            if(_fixupRequired)
            {
                Fixup();
            }
        }
    }
}