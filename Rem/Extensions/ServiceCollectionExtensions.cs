using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Rem.Attributes;

namespace Rem.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection collection, Assembly assembly)
            => AddServices(collection, assembly.FindTypesWithAttribute<ServiceAttribute>());

        public static IServiceCollection AddServices(this IServiceCollection collection, IEnumerable<Type> types)
        {
            foreach (var type in types)
            {
                var attribute = type.GetCustomAttribute<ServiceAttribute>();
                var target = attribute.Generic == null ? attribute.Target : attribute.Target.MakeGenericType(attribute.Generic);
                collection.AddSingleton(target, type);
            }

            return collection;
        }
    }
}
