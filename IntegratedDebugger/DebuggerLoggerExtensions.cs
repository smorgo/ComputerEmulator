using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IntegratedDebugger
{
    static public class DebuggerLoggerExtensions
{ 
    static public ILoggingBuilder AddDebuggerLogger(this ILoggingBuilder builder)
    {
        builder.AddConfiguration();
 
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, 
                                          DebuggerLoggerProvider>());
        // builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton
        //    <IConfigureOptions<FileLoggerOptions>, FileLoggerOptionsSetup>());
        // builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton
        //    <IOptionsChangeTokenSource<FileLoggerOptions>, 
        //    LoggerProviderOptionsChangeTokenSource<FileLoggerOptions, FileLoggerProvider>>());
        return builder;
    }
 
    static public ILoggingBuilder AddDebuggerLogger
           (this ILoggingBuilder builder, Action<DebuggerLoggerOptions> configure)
    {
        if (configure == null)
        {
            throw new ArgumentNullException(nameof(configure));
        }
 
        builder.AddDebuggerLogger();
        builder.Services.Configure(configure);
 
        return builder;
    }
}
}