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
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Repl;

namespace Repl
{
    class Program
    {
        public static ManualResetEventSlim CanStartEvent = new ManualResetEventSlim(false);

        static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            var scope = host.Services.CreateScope();
            ServiceProviderLocator.ServiceProvider = scope.ServiceProvider;
            Run();

            await host.RunAsync();

            scope.Dispose();
        }

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
                    configLogging.AddDebuggerLogger();
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
