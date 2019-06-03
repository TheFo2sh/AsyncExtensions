using System;
using System.Collections.Async;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Data;

namespace AsyncEnumerableExtensions
{
    public class IncrementalLoadingCollection<T> :ObservableCollection<T> , ISupportIncrementalLoading
    {
        private readonly WeakReference<IAsyncEnumerable<T>> _asyncEnumerable;
        private IAsyncEnumerator<T> enumerator;

        internal IncrementalLoadingCollection(IAsyncEnumerable<T> asyncEnumerable)
        {
            _asyncEnumerable=new WeakReference<IAsyncEnumerable<T>>(asyncEnumerable,true);
        }

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            return AsyncInfo.Run((c) => LoadMoreItemsAsync(c, count));
        }

        private async Task<LoadMoreItemsResult> LoadMoreItemsAsync(CancellationToken c, uint count)
        {
           var result= _asyncEnumerable.TryGetTarget(out var enumerable);
            if(!result)
                throw new ObjectDisposedException("Source asyncEnumerable");
            if (enumerator == null)
                enumerator = await enumerable.GetAsyncEnumeratorAsync(c);

            for (var i = 0; i < count; i++)
            {
                if (await enumerator.MoveNextAsync(c))
                    this.Add(enumerator.Current);
                else
                {
                    HasMoreItems = false;
                    break;
                }

            }
            return new LoadMoreItemsResult(){Count = (uint)this.Count};
        }

        public bool HasMoreItems { get; set; } = true;
    }
}
