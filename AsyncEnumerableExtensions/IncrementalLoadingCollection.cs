using Dasync.Collections;
using System;
    using System.Collections.ObjectModel;
    using System.Runtime.InteropServices.WindowsRuntime;
    using System.Threading;
    using System.Threading.Tasks;
    using Windows.Foundation;
    using Windows.UI.Xaml.Data;

    namespace AsyncEnumerableExtensions
    {
        public class IncrementalLoadingCollection<T> : ObservableCollection<T>, ISupportIncrementalLoading
        {
            private readonly IAsyncEnumerable<T> _asyncEnumerable;
            private IAsyncEnumerator<T> enumerator;

            internal IncrementalLoadingCollection(IAsyncEnumerable<T> asyncEnumerable)
            {
                _asyncEnumerable = asyncEnumerable;
            }

            public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
            {
                return AsyncInfo.Run((c) => LoadMoreItemsAsync(c, count));
            }

            private async Task<LoadMoreItemsResult> LoadMoreItemsAsync(CancellationToken c, uint count)
            {

                if (enumerator == null)
                    enumerator =  _asyncEnumerable.GetAsyncEnumerator(c);

            for (var i = 0; i < count; i++)
            {
                if (c.IsCancellationRequested)
                {
                    HasMoreItems = false;
                    break;
                }
                else if (await enumerator.MoveNextAsync())
                    this.Add(enumerator.Current);
                else
                {
                    HasMoreItems = false;
                    break;
                }

            }
                return new LoadMoreItemsResult() { Count = (uint)this.Count };
            }

            public bool HasMoreItems { get; set; } = true;
        }
    }
