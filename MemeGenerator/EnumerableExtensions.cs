using System;
using System.Collections.Generic;

namespace MemeGenerator
{
    public static class EnumerableExtensions
    {
        public static void Dispose(this IEnumerable<IDisposable> collection)
        {
            if (collection == null)
            {
                return;
            }
            foreach (var item in collection)
            {
                item?.Dispose();
            }
        }
    }
}