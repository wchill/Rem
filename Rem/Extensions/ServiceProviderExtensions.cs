using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rem.Attributes;

namespace Rem.Extensions
{
    public static class ServiceProviderExtensions
    {
        public static IServiceProvider RunInitMethods(this IServiceProvider services) =>
            RunInitMethods(services, Assembly.GetEntryAssembly());

        public static IServiceProvider RunInitMethods(this IServiceProvider services, Assembly assembly)
            => RunInitMethods(services, assembly.FindTypesWithAttribute<ServiceAttribute>());

        public static IServiceProvider RunInitMethods(this IServiceProvider services, IEnumerable<Type> types)
        {
            foreach (var type in types)
            {
                var serviceAtt = type.GetCustomAttribute<ServiceAttribute>();
                var target = serviceAtt.Generic == null ? serviceAtt.Target : serviceAtt.Target.MakeGenericType(serviceAtt.Generic);
                var service = services.GetService(target);

                foreach (var method in service.GetType().GetMethods())
                {
                    if (!(method.GetCustomAttribute<InitAttribute>() is InitAttribute initAtt))
                        continue;

                    var argTypes = initAtt.Arguments;
                    var args = argTypes.Select(services.GetService).ToArray();
                    method.Invoke(service, args);
                }
            }

            return services;
        }
    }
}