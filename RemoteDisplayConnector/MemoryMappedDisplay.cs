using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using HardwareCore;
using Microsoft.AspNetCore.SignalR.Client;

namespace RemoteDisplayConnector
{

    public class MemoryMappedDisplay : IAddressAssignment, IMemoryMappedDisplay
    {
        public const ushort DISPLAY_CONTROL_BLOCK_ADDR = 0x80;
        private VideoRam _videoRam;
        private DisplayControlBlock _controlBlock;

        HubConnection _connection;
        private DisplayMode _mode = DisplayMode.Mode7;

        public DisplayMode Mode
        {
            get
            {
                return _mode;
            }
        }

        public List<IAddressableBlock> Blocks { get; private set; }
        public MemoryMappedDisplay()
        {
        }

        public void Locate(ushort address, UInt32 size)
        {
            _videoRam = new VideoRam(this, 0, address, size);
            _videoRam.OnRender += OnRender;
            _controlBlock = new DisplayControlBlock(this, 1, DISPLAY_CONTROL_BLOCK_ADDR);
            _controlBlock.OnControlChanged += OnControlChanged;
            _controlBlock.OnModeChanged += OnModeChanged;
            _controlBlock.OnCursorMoved += OnCursorMoved;
            _controlBlock.OnClearScreen += async (s, e) => { await OnClearScreen(); };

            Blocks = new List<IAddressableBlock>
            {
                _videoRam,
                _controlBlock
            };
        }

        private async Task OnClearScreen()
        {
            Debug.WriteLine("Clear screen");
            try
            {
                _videoRam.Clear();
                await _connection.InvokeAsync("Clear");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void OnCursorMoved(object sender, CursorPosition e)
        {
        }

        private void OnModeChanged(object sender, byte e)
        {
        }

        private void OnControlChanged(object sender, byte e)
        {
        }

        public async Task Initialise()
        {
            await _videoRam.Initialise();
            await _controlBlock.Initialise();

            _connection = new HubConnectionBuilder()
                .WithUrl("https://localhost:5001/display")
                .Build();

            _connection.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0, 5) * 1000);
                try
                {
                    await _connection.StartAsync();
                    Debug.Assert(_connection.State == HubConnectionState.Connected);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    Console.WriteLine("Unable to reach remote display");
                }
            };

            try
            {
                await _connection.StartAsync();
                Debug.Assert(_connection.State == HubConnectionState.Connected);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Console.WriteLine("Unable to reach remote display");
            }

            await SendDisplayMode();
        }

        private async Task SendDisplayMode()
        {
            if(_connection.State == HubConnectionState.Connected)
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
        }

        public async Task SetMode(DisplayMode mode)
        {
            _mode = mode;
            await SendDisplayMode();
        }

        public void Write(int blockId, ushort address, byte value)
        {
            // address is now relative to start address
            var block = Blocks[blockId];
            Debug.Assert(address < block.Size);
            block.Write(address, value);
        }

        private void OnRender(object sender, AddressedByte data)
        {
            Task.Run(() => RenderCharacter(data.Address, data.Value));
        }

        private async Task RenderCharacter(ushort address, byte value)
        {
            if(_connection.State == HubConnectionState.Connected)
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
        }
        public byte Read(int blockId, ushort address)
        {
        // address is now relative to start address
            var block = Blocks[blockId];
            return block.Read(address);
        }

        public void Clear()
        {
            _videoRam.Clear();
        }
    }
}