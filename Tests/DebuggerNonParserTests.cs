using NUnit.Framework;
using System.Threading.Tasks;
using System;
using Debugger;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using HardwareCore;
using Microsoft.Extensions.Logging;

namespace Tests
{
    public class DebuggerNonParserTests
    {
        private IDebuggableCpu _cpuDebug;
        private IAddressMap _addressMap;
        private ILogFormatter _logFormatter;
        private ILabelMap _labels;
        private IParser _parser;
        private ServiceProvider _serviceProvider;
        private UnitTestLogger<Parser> _logger;
        private MockCpuHoldEvent _cpuHoldEvent;
        private MockCpuStepEvent _cpuStepEvent;


        public DebuggerNonParserTests()
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();
            ServiceProviderLocator.ServiceProvider = _serviceProvider;
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services
                 .AddScoped(typeof(ILogger<Parser>), typeof(UnitTestLogger<Parser>))
                 .AddScoped<IDebuggableCpu, MockCpuDebug>()
                 .AddScoped<IAddressMap, MockMemoryDebug>()
                 .AddScoped<ILabelMap, LabelMap>()
                 .AddScoped<ILogFormatter, DebugLogFormatter>()
                 .AddScoped<IParser, Parser>()
                 .AddScoped<ICpuHoldEvent,MockCpuHoldEvent>()
                 .AddScoped<ICpuStepEvent,MockCpuStepEvent>()
                 .AddScoped<IRegisterTracker, NoRegisterTracker>()
                 .AddSingleton<CancellationTokenWrapper>(new CancellationTokenWrapper());
        }
        [SetUp]
        public void Setup()
        {
            _cpuDebug = _serviceProvider.GetService<IDebuggableCpu>();
            _addressMap = _serviceProvider.GetService<IAddressMap>();
            AsyncUtil.RunSync(_addressMap.Initialise);

            _labels = _serviceProvider.GetService<ILabelMap>();
            _labels.Clear();
            _logFormatter = _serviceProvider.GetService<ILogFormatter>();
            _parser = _serviceProvider.GetService<IParser>();
            _logger = (UnitTestLogger<Parser>)_serviceProvider.GetService<ILogger<Parser>>();
            _cpuDebug.Breakpoints.Clear();
            _cpuHoldEvent = (MockCpuHoldEvent)_serviceProvider.GetService<ICpuHoldEvent>();
            _cpuStepEvent = (MockCpuStepEvent)_serviceProvider.GetService<ICpuStepEvent>();
            _cpuHoldEvent.Init();
            _cpuStepEvent.Init();
            _logger.GetOutput(); // Flush any old content
        }


        [Test]
        public void CanTrackRegisterChanges()
        {
            var tracker = new DebugRegisterTracker();
            string register = "";
            ushort value = 0x0000;
            
            tracker.RegisterUpdated += (s,e) => {
                register = e.Register;
                value = e.Value;
                };

            tracker.PostRegisterUpdated("A", 0x1234);

            Assert.AreEqual("A", register);
            Assert.AreEqual(0x1234, value);
        }

        [Test]
        public void CanInitialiseLabelMapWithLabels()
        {
            var labels = new LabelMap(new Label[] {
                    new Label("label1", 0x0001),
                    new Label("label2", 0x0002)
                });

            Assert.AreEqual(2, labels.AddressLabels.Count);
            Assert.AreEqual(2, labels.LabelAddresses.Count);
            ushort ignore;
            Assert.IsTrue(labels.TryLookup("label2", out ignore));
        }
   }
}