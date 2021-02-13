using NUnit.Framework;
using _6502;
using HardwareCore;
using System;
using RemoteDisplayConnector;
using System.Threading.Tasks;
using Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Tests
{
    public class RomTests
    {
        private IAddressMap mem;
        private Rom _rom;
        const ushort RAM_BANK_1_START = 0x0000;
        const ushort RAM_BANK_1_SIZE = 0x1000;
        const ushort RAM_BANK_2_START = 0x2000;
        const ushort RAM_BANK_2_SIZE = 0x1000;
        const ushort ROM_START = 0x1000;
        const ushort ROM_SIZE = 0x1000;

        private ServiceProvider _serviceProvider;

        public RomTests()
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();
            ServiceProviderLocator.ServiceProvider = _serviceProvider;
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services
                 .AddLogging()
                 .AddTransient<IAddressMap, AddressMap>()
                 .AddTransient<ILoaderLabelTable, LoaderLabelTable>();

        }
        [SetUp]
        public async Task Setup()
        {
            mem = _serviceProvider.GetService<IAddressMap>();
            _rom = new Rom(ROM_START, ROM_SIZE);

            mem.Install(new Ram(RAM_BANK_1_START, RAM_BANK_1_SIZE));
            mem.Install(_rom);
            mem.Install(new Ram(RAM_BANK_2_START, RAM_BANK_2_SIZE));
            await mem.Initialise();
        }

        [Test]
        public void ErasedRomIsFilled()
        {
            Assert.IsTrue(IsMemoryFilledWith(mem, ROM_START, ROM_SIZE, 0xFF));
        }

        [Test]
        public void CanWriteToRamButNoRom()
        {
            var end = (ushort)(RAM_BANK_2_START + RAM_BANK_2_SIZE - 1);

            for(var ix = RAM_BANK_1_START; ix <= end; ix++)
            {
                mem.Write(ix, 0x55);
            }

            Assert.IsTrue(IsMemoryFilledWith(mem, RAM_BANK_1_START, RAM_BANK_1_SIZE, 0x55));
            Assert.IsTrue(IsMemoryFilledWith(mem, ROM_START, ROM_SIZE, 0xFF));
            Assert.IsTrue(IsMemoryFilledWith(mem, RAM_BANK_2_START, RAM_BANK_2_SIZE, 0x55));
        }

        [Test]
        public void CanWriteToRamButBurningRom()
        {
            _rom.Burn = true;

            var end = (ushort)(RAM_BANK_2_START + RAM_BANK_2_SIZE - 1);

            for(var ix = RAM_BANK_1_START; ix <= end; ix++)
            {
                mem.Write(ix, 0x55);
            }

            Assert.IsTrue(IsMemoryFilledWith(mem, RAM_BANK_1_START, RAM_BANK_1_SIZE, 0x55),"RAM(1) unexpected content");
            Assert.IsTrue(IsMemoryFilledWith(mem, ROM_START, ROM_SIZE, 0x55),"ROM unexpected content");
            Assert.IsTrue(IsMemoryFilledWith(mem, RAM_BANK_2_START, RAM_BANK_2_SIZE, 0x55),"RAM(2) unexpected content");
        }

        [Test]
        public void CannotBurnRomTwice()
        {
            _rom.Erase();
            _rom.Burn = true;

            var end = (ushort)(ROM_START + ROM_SIZE - 1);

            for(var ix = ROM_START; ix <= end; ix++)
            {
                mem.Write(ix, 0x55);
            }

            _rom.Burn = false; // End burning

            Assert.IsTrue(IsMemoryFilledWith(mem, ROM_START, ROM_SIZE, 0x55),"ROM unexpected content");

            for(var ix = ROM_START; ix <= end; ix++)
            {
                mem.Write(ix, 0xAA);
            }

            // Content should not have changed
            Assert.IsTrue(IsMemoryFilledWith(mem, ROM_START, ROM_SIZE, 0x55),"ROM unexpected content");

            try
            {
                _rom.Burn = true; // Should fail
            }
            catch(InvalidOperationException)
            {
                Assert.Pass();
            }

            Assert.Fail("Exception wasn't thrown");
        }

        [Test]
        public void CannotBurnRomAfterErase()
        {
            _rom.Erase();
            _rom.Burn = true;

            var end = (ushort)(ROM_START + ROM_SIZE - 1);

            for(var ix = ROM_START; ix <= end; ix++)
            {
                mem.Write(ix, 0x55);
            }

            _rom.Burn = false; // End burning

            Assert.IsTrue(IsMemoryFilledWith(mem, ROM_START, ROM_SIZE, 0x55),"ROM unexpected content (1)");

            _rom.Erase();

            Assert.IsTrue(IsMemoryFilledWith(mem, ROM_START, ROM_SIZE, 0xFF),"ROM unexpected content (2)");

            _rom.Burn = true;

            for(var ix = ROM_START; ix <= end; ix++)
            {
                mem.Write(ix, 0xAA);
            }

            // Content should not have changed
            Assert.IsTrue(IsMemoryFilledWith(mem, ROM_START, ROM_SIZE, 0xAA),"ROM unexpected content (3)");

            try
            {
                _rom.Burn = true; // Should fail
            }
            catch(InvalidOperationException)
            {
                Assert.Fail("Exception wasn't expected");
            }

            Assert.Pass();
        }

        [Test]
        public void CanBurnContent()
        {
            _rom.Erase();

            Assert.IsFalse(_rom.Burned);

            ushort blockSize = 0x100;
            var block = new byte[blockSize];
            ushort blockStart = 0x200;
            Array.Fill<byte>(block, 0x77);

            _rom.BurnContent(block, blockStart);

            Assert.IsTrue(_rom.Burned);

            Assert.IsTrue(IsMemoryFilledWith(mem, ROM_START, blockStart, 0xFF),"ROM unexpected content (1)");
            Assert.IsTrue(IsMemoryFilledWith(mem, (ushort)(ROM_START + blockStart), blockSize, 0x77),"ROM unexpected content (2)");
            Assert.IsTrue(IsMemoryFilledWith(mem, (ushort)(ROM_START + blockStart + blockSize), 
                (ushort)(ROM_SIZE - blockStart - blockSize), 0xFF),"ROM unexpected content (3)");
        }

        [Test]
        public void CannotBurnContentTwice()
        {
            _rom.Erase();

            Assert.IsFalse(_rom.Burned);

            ushort blockSize = 0x100;
            var block = new byte[blockSize];
            ushort blockStart = 0x200;
            Array.Fill<byte>(block, 0x77);

            _rom.BurnContent(block, blockStart);

            Assert.IsTrue(_rom.Burned);

            try
            {
                _rom.BurnContent(block, blockStart);
            }
            catch(InvalidOperationException)
            {
                Assert.Pass();
            }

            Assert.Fail("Second BurnContent should have failed");
        }

        private bool IsMemoryFilledWith(IAddressMap mem, ushort start, ushort size, byte value)
        {
            var end = (ushort)(start + size - 1);

            for(var ix = start; ix <= end; ix++)
            {
                if(mem.Read(ix) != value)
                {
                    return false;
                }
            }

            return true;
        }
    }
}