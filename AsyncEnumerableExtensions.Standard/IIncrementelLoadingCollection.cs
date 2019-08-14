using System.Collections.Async;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Dynamensions.Input.Commands;

namespace AsyncEnumerableExtensions.Standard
{
    public interface IIncrementelLoadingCollection<T>:ICollection<T>
    {
        ICommand LoadMoreCommand { get; }
        bool IsBusy { get; }
    }

    internal class StandardIncrementelLoadingCollection<T> :ObservableCollection<T> , IIncrementelLoadingCollection<T>
    {
        public ICommand LoadMoreCommand { get; }
        public bool IsBusy { get; private set; }

        private readonly IAsyncEnumerable<T> _asyncEnumerable;
        private IAsyncEnumerator<T> enumerator;
        private int _count;
        internal StandardIncrementelLoadingCollection(IAsyncEnumerable<T> asyncEnumerable, int count)
        {
            _asyncEnumerable = asyncEnumerable;
            _count = count;
            LoadMoreCommand=new RelayCommand<object>((x)=>LoadMoreItemsAsync(),(x) => HasMoreItems);
            LoadMoreItemsAsync();
        }

       

        private async void LoadMoreItemsAsync()
        {
            IsBusy = true;
            if (enumerator == null)
                enumerator = await _asyncEnumerable.GetAsyncEnumeratorAsync();

            for (var i = 0; i < _count; i++)
            {
                if (await enumerator.MoveNextAsync())
                    this.Add(enumerator.Current);
                else
                {
                    HasMoreItems = false;
                    break;
                }

            }

            IsBusy = false;
        }

        public bool HasMoreItems { get; set; } = true;
    }
}