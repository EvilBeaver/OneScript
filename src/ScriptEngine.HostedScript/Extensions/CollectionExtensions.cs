using System.Collections.Generic;

namespace ScriptEngine.HostedScript.Extensions
{
    public static class CollectionExtensions
    {
        public static void AddRange<T>(this IList<T> destination, IEnumerable<T> source)
        {
            foreach (var item in source)
            {
                destination.Add(item);
            }
        }
    }
}