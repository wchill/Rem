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
    }
}