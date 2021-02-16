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
using SignalRConnection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace Repl
{
    class Program
    {
        public static ManualResetEventSlim CanStartEvent = new ManualResetEventSlim(false);

        // private static CancellationTokenWrapper _cancellationToken;
        // private static IServiceProvider _serviceProvider;

        static async Task Main(string[] args)
        {
            //_cancellationToken = new CancellationTokenWrapper();
            var host = CreateHostBuilder(args).Build();

//            var serviceCollection = new ServiceCollection();
//            ConfigureServices(serviceCollection);
//            _serviceProvider = serviceCollection.BuildServiceProvider();
            // ServiceProviderLocator.ServiceProvider = _serviceProvider;
            //ServiceProviderLocator.ServiceProvider = host.Services;
            var scope = host.Services.CreateScope();
            ServiceProviderLocator.ServiceProvider = scope.ServiceProvider;
            Run();

            await host.RunAsync();

            scope.Dispose();
            Debug.WriteLine("Here I am");
        }

        // private static void ConfigureServices(IServiceCollection services)
        // {
        //     services.AddLogging(
        //          configure => configure.AddDebuggerLogger())
        //          .AddSingleton<ILoaderLabelTable>(new LoaderLabelTable())
        //          .AddScoped<IEmulatorHost, ReplHost>()
        //          .AddScoped<IDebugger, ConsoleDebugger>()
        //          .AddScoped<ILogFormatter, DebugLogFormatter>()
        //          .AddScoped<IParser, Parser>()
        //          .AddScoped<ILabelMap, LabelMap>()
        //          .AddScoped<IDebuggableCpu, CPU6502>()
        //          .AddScoped<IAddressMap, AddressMap>()
        //          .AddScoped<IEmulatorConsole, ReplConsole>()
        //          .AddTransient<ISignalRHubConnection, SignalRHubConnection>()
        //          .AddScoped<IRemoteConnection, NoRemoteKeyboardConnection>()
        //          .AddScoped<IMemoryMappedDisplay, MemoryMappedDisplay>()
        //          .AddScoped<IRemoteDisplayConnection, RemoteDisplayConnection>()
        //          .AddTransient<ILoader, Loader>()
        //          .AddScoped<ILogSink, ReplSink>()
        //          .AddScoped<ICpuHoldEvent, CpuHoldEvent>()
        //          .AddScoped<ICpuStepEvent, CpuStepEvent>()
        //          .AddScoped<IRegisterTracker, DebugRegisterTracker>()
        //          .AddSingleton<CancellationTokenWrapper>(_cancellationToken)
        //          ;
        // }

        static void Run()
        {
            var hostThread = new Thread(new ThreadStart(RunHost));
            hostThread.Priority = ThreadPriority.BelowNormal;
            var debuggerThread = new Thread(new ThreadStart(RunDebugger));
            debuggerThread.Priority = ThreadPriority.AboveNormal;
            var consoleThread = new Thread(new ThreadStart(RunConsole));
            hostThread.Start();
            debuggerThread.Start();
            consoleThread.Start();
        }

        private static void RunConsole()
        {
            CanStartEvent.Wait();
            Thread.Sleep(3000);
            var console = ServiceProviderLocator.ServiceProvider.GetService<IEmulatorConsole>();
            console.Start();
        }

        private static void RunHost()
        {
            CanStartEvent.Wait();
            Thread.Sleep(3000);
            var host = ServiceProviderLocator.ServiceProvider.GetService<IEmulatorHost>();
            host.Start();
        }

        private static void RunDebugger()
        {
            CanStartEvent.Wait();
            Thread.Sleep(3000);
            var debugger = ServiceProviderLocator.ServiceProvider.GetService<IDebugger>();
            var parser = ServiceProviderLocator.ServiceProvider.GetService<IParser>() as Parser;
            debugger.Start();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureLogging((hostContext, configLogging) =>
                {
                    configLogging.ClearProviders();
                    configLogging.AddDebug();
                })
                .ConfigureHostConfiguration(configHost =>
                {
                    configHost.SetBasePath(Directory.GetCurrentDirectory());
                    configHost.AddJsonFile("Config/hostsettings.json", optional: true);
                    configHost.AddEnvironmentVariables(prefix: "BOT_");
                    configHost.AddCommandLine(args);
                })
                .ConfigureAppConfiguration((hostContext, configApp) =>
                {
                    configApp.AddJsonFile("Config/appsettings.json", optional: true);
                    configApp.AddJsonFile(
                        $"Config/appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json",
                        optional: true);
                    configApp.AddEnvironmentVariables(prefix: "BOT_");
                    configApp.AddCommandLine(args);
                })
                .UseConsoleLifetime();
    }
}
