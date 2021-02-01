using System;
using System.IO;
using HardwareCore;

namespace FilePersistence
{
    public class MemoryFilePersistence : ILoaderPersistence
    {
        private string _workingDirectory;
        public string WorkingDirectory 
        {
            get 
            {
                return _workingDirectory;
            }
             
            set
            {
                if(!Path.EndsInDirectorySeparator(value))
                {
                    _workingDirectory = value + Path.DirectorySeparatorChar;
                }
                else
                {
                    _workingDirectory = value;
                }
            }
        }

        public void Load(string name, IAddressAssignment mem)
        {
            LoadInternal(name, mem);
        }

        public void LoadAt(string name, ushort startAddress, IAddressAssignment mem)
        {
            LoadInternal(name, mem, startAddress);
        }

        private void LoadInternal(string name, IAddressAssignment mem, int overrideStartAddress = -1)
        {
            var combined = CrossPlatformPathExtensions.Combine(WorkingDirectory, name);
            var filename = Path.GetFullPath(combined);

            using(var file = File.OpenRead(filename))
            {
                var address = file.ReadByte() + (256 * file.ReadByte());
                var length = file.ReadByte() + (256 * file.ReadByte());

                if(overrideStartAddress >= 0)
                {
                    address = overrideStartAddress;
                }

                var byteCount = 0;

                while(byteCount < length)
                {
                    mem.Write((ushort)(address + byteCount), (byte)file.ReadByte());
                    byteCount++;
                }
            }
        }
        public void Save(string name, ushort startAddress, ushort length, IAddressAssignment mem)
        {
            var combined = CrossPlatformPathExtensions.Combine(WorkingDirectory, name);
            var filename = Path.GetFullPath(combined);

            var buffer = new byte[length + 4];

            buffer[0] = (byte)(startAddress & 0xff);
            buffer[1] = (byte)(startAddress >> 8);
            buffer[2] = (byte)(length & 0xff);
            buffer[3] = (byte)(length >> 8);
            for(var ix = 0; ix < length; ix++)
            {
                buffer[ix + 4] = mem.Read((ushort)(ix + startAddress));
            }

            using(var file = File.Create(filename))
            {
                file.Write(buffer, 0, length + 4);
            }
        }
    }
}
