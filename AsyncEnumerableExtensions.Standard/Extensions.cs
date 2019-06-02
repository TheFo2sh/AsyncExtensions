using System;
using System.Collections.Async;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AsyncEnumerableExtensions.Standard
{

    public static class AsyncEnumerableBuilder
    {
        /// <summary>
        /// Create an AsyncEnumerable from Paged request
        /// </summary>
        /// <typeparam name="T">the Enumerable Type</typeparam>
        /// <typeparam name="P">the Paged result type</typeparam>
        /// <param name="pagedRequest">the method that return data of a specific page</param>
        /// <param name="itemsSelector">the items selector of the reutrned data</param>
        /// <param name="hasNext">check if there is more pages</param>
        /// <param name="updatecounter">updeted the loaded items counter</param>
        /// <param name="nextpageNumber">return the page number</param>
        /// <param name="nextpageSize">return the page size</param>
        /// <returns></returns>
        public static IAsyncEnumerable<T> FromPaged<T, P>(
            Func<int, int, Task<P>> pagedRequest,
            Func<P, IEnumerable<T>> itemsSelector,
            Func<int, P, bool> hasNext,
            Func<P, int, int> updatecounter,
            Func<P, int> nextpageNumber,
            Func<P, int> nextpageSize)
        {
            return new AsyncEnumerable<T>(async yield =>
            {
                int count = 0;
                P results = default(P);
                do
                {
                    var invoke = nextpageNumber.Invoke(results);
                    results = await pagedRequest.Invoke(invoke, nextpageSize.Invoke(results));
                    count = updatecounter.Invoke(results, count);
                    foreach (var result in itemsSelector.Invoke(results))
                    {
                        await yield.ReturnAsync(result);
                    }
                } while (hasNext.Invoke(count, results));

            });

        }

        public static IAsyncEnumerable<T> FromTasks<T>(params Task<T>[] tasks)
        {
            var list = tasks.ToList();
            return new AsyncEnumerable<T>(async yield =>
            {
                do
                {
                    var completedTask = await Task.WhenAny(list);
                    list.Remove(completedTask);
                    await yield.ReturnAsync(await completedTask);
                } while (list.Any());
            });
        }

        public static IAsyncEnumerable<T> FromTasks<T>(this IEnumerable<Task<T>> tasks)
        {
            return FromTasks(tasks.ToArray());
        }

    }
}
