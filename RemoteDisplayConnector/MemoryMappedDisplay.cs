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
        private IRemoteDisplayConnection _connection;
        private DisplayMode _mode = DisplayMode.Mode7;

        public DisplayMode Mode
        {
            get
            {
                return _mode;
            }
        }

        public List<IAddressableBlock> Blocks { get; private set; }
        public MemoryMappedDisplay(IRemoteDisplayConnection connection)
        {
            _connection = connection;
        }

        public void Locate(ushort address, UInt32 size)
        {
            _videoRam = new VideoRam(this, 0, address, size);
            _videoRam.OnRender += OnRender;
            _controlBlock = new DisplayControlBlock(this, 1, DISPLAY_CONTROL_BLOCK_ADDR, _mode.Mode);
            _controlBlock.OnControlChanged += async (s,e) => { await OnControlChanged(s,e); };
            _controlBlock.OnModeChanged += async (s,e) => { await OnModeChanged(s,e); };
            _controlBlock.OnCursorMoved += async (s,e) => { await OnCursorMoved(s,e); };
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
            _videoRam.Clear();
            await _connection.Clear();
        }

        private async Task OnCursorMoved(object sender, CursorPosition e)
        {
            await _connection.SendCursorPosition(e);
        }

        private async Task OnModeChanged(object sender, byte e)
        {
            var mode = DisplayMode.GetMode(e);
            if(mode != null)
            {
                _mode = mode;
                await _connection.SendDisplayMode(mode);
            }
        }
        private async Task OnControlChanged(object sender, byte e)
        {
            await _connection.SendControl(e);
        }

        public async Task Initialise()
        {
            await _videoRam.Initialise();
            await _controlBlock.Initialise();

            await _connection.Initialise();
            
            await _connection.SendDisplayMode(_mode);
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
            AsyncUtil.RunSync(() => RenderCharacter(data.Address, data.Value));
        }

        private async Task RenderCharacter(ushort address, byte value)
        {
            await _connection.RenderCharacter(address, value);
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

        public void SetMode(DisplayMode mode)
        {
            _controlBlock.Write(DisplayControlBlock.MODE_ADDR, mode.Mode);
        }
    }
}