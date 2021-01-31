using System;
using System.Diagnostics;
using System.Threading.Tasks;
using HardwareCore;
using Microsoft.AspNetCore.SignalR.Client;

namespace RemoteDisplayConnector
{
    public class MemoryMappedDisplay : IAddressAssignment
    {
        public bool CanRead => true;
        public bool CanWrite => true;
        public ushort StartAddress {get; private set;}
        public UInt32 Size {get; private set;}
        private Byte[] Memory;
        HubConnection _connection;
        private DisplayMode _mode = DisplayMode.Mode7;

        public DisplayMode Mode
        {
            get
            {
                return _mode;
            }
        }

        public MemoryMappedDisplay(ushort absoluteAddress, UInt32 size) 
        {
            Debug.Assert(absoluteAddress + size <= 0x10000);
            StartAddress = absoluteAddress;
            Size = size;
            Memory = new byte[size];
        }

        public async Task Initialise()
        {
            _connection = new HubConnectionBuilder()
                .WithUrl("https://localhost:5001/display")
                .Build();
        
            _connection.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0,5) * 1000);
                await _connection.StartAsync();
                Debug.Assert(_connection.State == HubConnectionState.Connected);
            };

            await _connection.StartAsync();
            Debug.Assert(_connection.State == HubConnectionState.Connected);
            await SendDisplayMode();
        }

        private async Task SendDisplayMode()
        {
            var message = $"Display Mode: {_mode.Mode}";

            Debug.WriteLine(message);
            try
            {
                await _connection.InvokeAsync("SetMode", _mode);
            }
            catch (Exception ex)
            {                
                Debug.WriteLine(ex.Message);
            }
        }

        public async Task SetMode(DisplayMode mode)
        {
            _mode = mode;
            await SendDisplayMode();
        }

        public void Write(ushort address, byte value)
        {
            // address is now relative to start address
            Debug.Assert(address < Size);
            Memory[address] = value;
            Task.Run(() => RenderCharacter(address, value));
        }

        private async Task RenderCharacter(ushort address, byte value)
        {
            try
            {
                await _connection.InvokeAsync("Write", 
                    address, value);
            }
            catch (Exception ex)
            {                
                Debug.WriteLine(ex.Message);
            }
        }
        public byte Read(ushort address)
        {
            // address is now relative to start address
            Debug.Assert(address < Size);
            return Memory[address];
        }

        private async Task Render()
        {
            var message = $"Display: {System.Text.Encoding.ASCII.GetString(Memory, 0, (int)Size)}";

            Debug.WriteLine(message);
            try
            {
                await _connection.InvokeAsync("SendMessage", 
                    "CPU", message);
            }
            catch (Exception ex)
            {                
                Debug.WriteLine(ex.Message);
            }
        }

        public void Clear()
        {
            for(var ix = 0; ix < Size; ix++)
            {
                Memory[ix] = (byte)' ';
            }
        }
    }
}