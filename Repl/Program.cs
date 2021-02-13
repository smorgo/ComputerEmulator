using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using _6502;
using Debugger;
using IntegratedDebugger;
using Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using HardwareCore;
using KeyboardConnector;
using RemoteDisplayConnector;

namespace Repl
{
    class Program
    {
        private static CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private static CancellationTokenWrapper _cancellationToken;
        private static ServiceProvider _serviceProvider;

        static void Main(string[] args)
        {
            _cancellationToken = new CancellationTokenWrapper(_cancellationTokenSource.Token);
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();
            ServiceProviderLocator.ServiceProvider = _serviceProvider;
            Run();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(
                 configure => configure.AddDebuggerLogger())
                 .AddSingleton<ILoaderLabelTable>(new LoaderLabelTable())
                 .AddScoped<IEmulatorHost, ReplHost>()
                 .AddScoped<IDebugger, ConsoleDebugger>()
                 .AddScoped<ILogFormatter, DebugLogFormatter>()
                 .AddScoped<IParser, Parser>()
                 .AddScoped<ILabelMap, LabelMap>()
                 .AddScoped<IDebuggableCpu, CPU6502>()
                 .AddScoped<IAddressMap, AddressMap>()
                 .AddScoped<IMemoryDebug, AddressMap>()
                 .AddScoped<IEmulatorConsole, ReplConsole>()
                 .AddScoped<IRemoteConnection, NoRemoteKeyboardConnection>()
                 .AddScoped<IMemoryMappedDisplay, MemoryMappedDisplay>()
                 .AddTransient<ILoader, Loader>()
                 .AddScoped<ILogSink, ReplSink>()
                 .AddScoped<ICpuHoldEvent, CpuHoldEvent>()
                 .AddScoped<ICpuStepEvent, CpuStepEvent>()
                 .AddScoped<IRegisterTracker, DebugRegisterTracker>()
                 .AddSingleton<CancellationTokenWrapper>(_cancellationToken)
                 ;
        }

        static void Run()
        {
            var hostThread = new Thread(new ThreadStart(RunHost));
            hostThread.Priority = ThreadPriority.BelowNormal;
            var debuggerThread = new Thread(new ThreadStart(RunDebugger));
            debuggerThread.Priority = ThreadPriority.AboveNormal;

            hostThread.Start();

            debuggerThread.Start();

            var console = _serviceProvider.GetService<IEmulatorConsole>();
            console.Start();
        }

        private static void RunHost()
        {
            var host = _serviceProvider.GetService<IEmulatorHost>();
            host.Start();
        }

        private static void RunDebugger()
        {
            var debugger = _serviceProvider.GetService<IDebugger>();
            var parser = _serviceProvider.GetService<IParser>() as Parser;
            debugger.Start();
        }
    }
}
