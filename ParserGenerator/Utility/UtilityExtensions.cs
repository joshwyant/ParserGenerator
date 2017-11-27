using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserGenerator.Utility
{
    public static class UtilityExtensions
    {
        /// <summary>
        /// Unions a hash set with an enumerable, and returns whether anything was added.
        /// </summary>
        /// <typeparam name="T">The type of elements in the hashset.</typeparam>
        /// <param name="hashSet">The hash set to add elements to, to create a union.</param>
        /// <param name="other">The enumerable to add elements from.</param>
        /// <returns></returns>
        public static bool TryUnionWith<T>(this HashSet<T> hashSet, IEnumerable<T> other)
        {
            var c = hashSet.Count;

            hashSet.UnionWith(other);

            return hashSet.Count > c;
        }

        public static void CopyToArray<T>(this IEnumerable<T> enumerable, T[] array)
        {
            var enumerator = enumerable.GetEnumerator();
            for (var i = 0; i < array.Length; i++)
            {
                if (!enumerator.MoveNext())
                    break;

                array[i] = enumerator.Current;
            }
        }

        public static IEnumerable<T> AsSingletonEnumerable<T>(this T item)
        {
            yield return item;
        }

        public static void Each<T>(this IEnumerable<T> e, Action<T> a)
        {
            foreach (var i in e) a(i);
        }
    }
}
