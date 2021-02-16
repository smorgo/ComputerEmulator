using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _6502;
using Debugger;
using HardwareCore;
using IntegratedDebugger;
using KeyboardConnector;
using Memory;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RemoteDisplayConnector;
using Repl.Hubs;
using SignalRConnection;

namespace Repl
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapHub<DisplayHub>("/display");
            });

            
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddSignalR();

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
                 .AddScoped<IEmulatorConsole, ReplConsole>()
                 .AddTransient<ISignalRHubConnection, SignalRHubConnection>()
                 .AddScoped<IRemoteConnection, NoRemoteKeyboardConnection>()
                 .AddScoped<IMemoryMappedDisplay, MemoryMappedDisplay>()
                 .AddScoped<IRemoteDisplayConnection, RemoteDisplayConnection>()
                 .AddTransient<ILoader, Loader>()
                 .AddScoped<ILogSink, ReplSink>()
                 .AddScoped<ICpuHoldEvent, CpuHoldEvent>()
                 .AddScoped<ICpuStepEvent, CpuStepEvent>()
                 .AddScoped<IRegisterTracker, DebugRegisterTracker>()
                 .AddSingleton<CancellationTokenWrapper>(new CancellationTokenWrapper())
                 ;

            Program.CanStartEvent.Set();
        }
    }
}
