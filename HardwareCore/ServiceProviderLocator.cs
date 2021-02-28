using System;
using Microsoft.Extensions.DependencyInjection;

namespace HardwareCore
{
    public static class ServiceProviderLocator
    {
        public static IServiceProvider ServiceProvider {get; set;}
    }
}