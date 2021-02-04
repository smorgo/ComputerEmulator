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
            public bool OneByte {get; private set;}
            public ByteSelector ByteSelector {get; private set;}
            public int Offset {get; private set;}
            public ReferenceDescriptor(string label, bool relative = false, bool oneByte = false, int offset = 0)
            {
                Offset = offset; // This may be modified by ParseLabel
                Label = ParseLabel(label);
                Relative = relative;
                OneByte = oneByte;
            }

            private string ParseLabel(string label)
            {
                var ix = label.IndexOf(":");

                if(ix < 0)
                {
                    ByteSelector = ByteSelector.Both;
                    return ParseLabelOffset(label);
                }

                var byteSpec = label.Substring(ix+1).ToUpper();

                switch(byteSpec)
                {
                    case "LO":
                        ByteSelector = ByteSelector.Low;
                        break;
                    case "HI":
                        ByteSelector = ByteSelector.High;
                        break;
                    default:
                        throw new InvalidOperationException($"Invalid label specifier: {label}");
                }

                return ParseLabelOffset(label.Substring(0, ix));
            }

            private string ParseLabelOffset(string label)
            {
                var ix = label.IndexOf("+");

                if(ix < 0)
                {
                    return ParseLabelNegativeOffset(label);
                }

                Offset += int.Parse(label.Substring(ix+1));

                return label.Substring(0, ix);
            }

            private string ParseLabelNegativeOffset(string label)
            {
                var ix = label.IndexOf("-");

                if(ix < 0)
                {
                    return label;
                }

                Offset += int.Parse(label.Substring(ix));

                return label.Substring(0, ix);
            }

            public int Select(int address)
            {
                switch(ByteSelector)
                {
                    case ByteSelector.Low:
                        return (ushort)(address & 0xff);
                    case ByteSelector.High:
                        return (ushort)(address >> 8);
                    default:
                        return address;
                }
            }
        }

        private bool _fixupRequired = false;
        private LabelTable _labels;
        private Dictionary<ushort, ReferenceDescriptor> _labelReferences;
        public ushort Cursor {get; private set;}
        private IAddressMap _addressMap;

        public bool HasErrors {get; private set;}

        public Loader(IAddressMap addressMap, ushort cursor, LabelTable labels)
        {
            _addressMap = addressMap;
            Cursor = cursor;
            _labels = labels;
            _labelReferences = new Dictionary<ushort, ReferenceDescriptor>();
            HasErrors = false;
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
            return Ref(Cursor, label, offset);
        }

        public Loader Ref(ushort address, string label, int offset = 0)
        {
            _fixupRequired = true;
            _labelReferences.Add(address, new ReferenceDescriptor(label, offset: offset));
            return WriteWord(address, 0xffff);
        }

        public Loader RefByte(string label, int offset = 0)
        {
            return RefByte(Cursor, label, offset);
        }

        public Loader RefByte(ushort address, string label, int offset = 0)
        {
            _fixupRequired = true;
            _labelReferences.Add(address, new ReferenceDescriptor(label, false, true, offset));
            return Write(address, 0xff);
        }
        public Loader ZeroPageRef(string label, int offset = 0)
        {
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
            return RelativeRef(Cursor, label);
        }

        public Loader RelativeRef(ushort address, string label)
        {
            _fixupRequired = true;
            _labelReferences.Add(address, new ReferenceDescriptor(label, true));
            return Write(address, 0xff);
        }

        public Loader Label(ushort address, string label)
        {
            AddLabel(label, address);
            return this;
        }

        public Loader Label(string label)
        {
            return Label(Cursor, label);
        }
        
        private void AddLabel(string label, ushort address)
        {
            _fixupRequired = true;
            if(!string.IsNullOrWhiteSpace(label))
            {
                if(!_labels.Add(label, address))
                {
                    Console.WriteLine($"Label '{label}' already defined");
                    HasErrors = true;
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
                    var descriptor = reference.Value;

                    if(descriptor.Relative)
                    {
                        short relAddress = (short)(descriptor.Select(labelAddress - reference.Key -1));
                        _addressMap.Write(reference.Key, (byte)relAddress);
                    }
                    else if(descriptor.OneByte)
                    {
                        var addressToUse = descriptor.Select(labelAddress + reference.Value.Offset);

                        if(addressToUse >= 0 && addressToUse < 0x100)
                        {
                            _addressMap.Write(reference.Key, (byte)addressToUse);
                        }
                        else
                        {
                            Console.WriteLine($"FIXUP ERROR at ${reference.Key:X4} - label '{reference.Value.Label}' at ${addressToUse:X4} is not a single byte");
                            HasErrors = true;
                        }
                    }
                    else
                    {
                        var offsetAddress = descriptor.Select(labelAddress + reference.Value.Offset);

                        if(offsetAddress >= 0 && offsetAddress < 0x10000)
                        {
                            _addressMap.WriteWord(reference.Key, (ushort)offsetAddress );
                        }
                        else
                        {
                            Console.WriteLine($"FIXUP ERROR at ${reference.Key:X4} - label '{reference.Value.Label}' at ${offsetAddress:X8} is not valid");
                            HasErrors = true;
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"Error: 0x{reference.Key:X4} Label '{reference.Value} is not defined");
                    HasErrors = true;
                }
            }

            _fixupRequired = false;

            if(HasErrors)
            {
                throw new InvalidProgramException("There were errors in the loader");
            }

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