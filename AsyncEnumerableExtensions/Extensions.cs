
using Dasync.Collections;

namespace AsyncEnumerableExtensions
{
    public static class Extensions
    {
        public static IncrementalLoadingCollection<T> ToIncrementalLoadingCollection<T>(
            this IAsyncEnumerable<T> asyncEnumerable)
        {
            return new IncrementalLoadingCollection<T>(asyncEnumerable);
        }
    }
}