using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Vasi
{
    [PublicAPI]
    public static class EnumerableUtil
    {
        public static IEnumerable<T> RemoveFirst<T>(this IEnumerable<T> source, Func<T, bool> f)
        {
            using IEnumerator<T> iter = source.GetEnumerator();

            while (iter.MoveNext())
            {
                if (f(iter.Current))
                {
                    // Skip this.

                    break;
                }

                yield return iter.Current;
            }


            while (iter.MoveNext())
                yield return iter.Current;
        }

        public static IEnumerable<T> Append<T>(this IEnumerable<T> source, T elem)
        {
            using IEnumerator<T> iter = source.GetEnumerator();

            while (iter.MoveNext())
                yield return iter.Current;

            yield return elem;
        }

        public static IEnumerable<T> Insert<T>(this IEnumerable<T> source, int index, T elem)
        {
            using IEnumerator<T> iter = source.GetEnumerator();

            int i = 0;

            for (; iter.MoveNext(); i++)
            {
                if (i == index)
                    yield return elem;

                yield return iter.Current;
            }

            if (i == index)
                yield return elem;
        }
    }
}