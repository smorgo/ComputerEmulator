using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using HardwareCore;
using Memory;
using RemoteDisplayConnector;
using Microsoft.AspNetCore.SignalR.Client;

namespace Tests
{
    public class MockMemoryMappedDisplay : IAddressAssignment, IMemoryMappedDisplay
    {

        private DisplayControlBlock _controlBlock;
        public const ushort DISPLAY_CONTROL_BLOCK_ADDR = 0x80;
        private Ram _videoRam;
        private DisplayMode _mode = DisplayMode.Mode7;

        public DisplayMode Mode
        {
            get
            {
                return _mode;
            }
        }

        public List<IAddressableBlock> Blocks {get; private set;}

        public MockMemoryMappedDisplay()
        {
        }
        public void Locate(ushort address, UInt32 size)
        {
            _videoRam = new Ram(address, size);
            _controlBlock = new DisplayControlBlock(this, 1, DISPLAY_CONTROL_BLOCK_ADDR);
            _controlBlock.OnControlChanged += OnControlChanged;
            _controlBlock.OnModeChanged += OnModeChanged;
            _controlBlock.OnCursorMoved += OnCursorMoved;
            _controlBlock.OnClearScreen += async (s,e) => {await OnClearScreen();};
            
            Blocks = new List<IAddressableBlock>
            {
                _videoRam,
                _controlBlock
            };
        }

        private async Task OnClearScreen()
        {
            await _videoRam.Initialise();
            await Task.Delay(0);
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
        }

        public async Task SetMode(DisplayMode mode)
        {
            _mode = mode;
            await Task.Delay(0);
        }

        public void Write(int blockId, ushort address, byte value)
        {
            // address is now relative to start address
            var block = Blocks[blockId];
            Debug.Assert(address < block.Size);
            block.Write(address, value);
        }

        public byte Read(int blockId, ushort address)
        {
            // address is now relative to start address
            var block = Blocks[blockId];
            return block.Read(address);
        }

        public void Clear()
        {
            AsyncUtil.RunSync(() => _videoRam.Initialise());
        }
    }
}