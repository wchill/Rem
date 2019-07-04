using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Rem.Extensions
{
    public static class AssemblyExtensions
    {
        public static IEnumerable<Type> FindTypesWithAttribute<T>(this Assembly assembly)
        {
            return assembly.GetTypes().Where(x => x.GetCustomAttributes(typeof(T), true).Length > 0);
        }

        public static IEnumerable<Type> FindTypesAssignableTo<T>(this Assembly assembly, string underNamespace = null)
        {
            var types = assembly.GetTypes().Where(p => typeof(T).IsAssignableFrom(p));
            if (underNamespace != null)
            {
                types = types.Where(t => t.Namespace == underNamespace);
            }

            return types;
        }
    }
}