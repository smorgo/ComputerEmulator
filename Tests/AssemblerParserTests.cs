using NUnit.Framework;
using _6502;
using HardwareCore;
using System;
using RemoteDisplayConnector;
using System.Threading.Tasks;
using Memory;
using System.Threading;
using Microsoft.Extensions.Logging;

using Microsoft.Extensions.DependencyInjection;
using Debugger;
using System.Collections;
using Assembler6502;

namespace Tests
{
    public class AssemblerParserTests
    {
        private IAddressMap mem;
        private IServiceProvider _serviceProvider;
        private AssemblyParser _parser;
        public AssemblerParserTests()
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();
            ServiceProviderLocator.ServiceProvider = _serviceProvider;
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services
                .AddScoped<IAddressMap, AddressMap>()
                .AddScoped<ILoaderLabelTable, LoaderLabelTable>()
                .AddScoped<LexicalAnalyser, LexicalAnalyser>()
                .AddScoped<AssemblyParser, AssemblyParser>();

        }
        [SetUp]
        public void Setup()
        {
            mem = _serviceProvider.GetService<IAddressMap>();
            mem.Install(new Ram(0x0000, 0x10000));

            _parser = _serviceProvider.GetService<AssemblyParser>();
        }

        [Test]
        public void CanParseSmallProgram()
        {
            var code = @"
CLRMEM  LDA #$00        ;Set up zero value
        TAY             ;Initialize index pointer
CLRM1   STA (TOPNT),Y   ;Clear memory location
        INY             ;Advance index pointer
        DEX             ;Decrement counter
        BNE CLRM1       ;Not zero, continue checking
        RTS             ;Return
";

            _parser.Parse(code);
        }
    }
}