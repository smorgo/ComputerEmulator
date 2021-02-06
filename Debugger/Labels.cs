using System.Collections.Generic;

namespace Debugger
{
    public class Labels 
    {
        public Dictionary<string, ushort> LabelAddresses {get; set;}
        public Dictionary<ushort, string> AddressLabels {get; set;}

        public Labels(IEnumerable<Label> labels = null)
        {
            LabelAddresses = new Dictionary<string, ushort>();
            AddressLabels = new Dictionary<ushort, string>();

            if(labels != null)
            {
                foreach(var label in labels)
                {
                    Add(label);
                }
            }
        }
        public void Add(Label label)
        {
            LabelAddresses.Add(label.Name.ToLower(), label.Address);

            if(AddressLabels.ContainsKey(label.Address))
            {
                AddressLabels[label.Address] = AddressLabels[label.Address] + "|" + label.Name;
            }
            else
            {
                AddressLabels.Add(label.Address, label.Name);
            }
        }

        public bool TryLookup(string name, out ushort address)
        {
            name = name.ToLower();

            if(LabelAddresses.ContainsKey(name))
            {
                address = LabelAddresses[name];
                return true;
            }

            address = 0x0000;
            return false;
        }

        public bool TryLookup(ushort address, out string name)
        {
            if(AddressLabels.ContainsKey(address))
            {
                name = AddressLabels[address];
                return true;
            }

            name = string.Empty;
            return false;
        }
    }
}