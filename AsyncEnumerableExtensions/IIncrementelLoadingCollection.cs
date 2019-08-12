using System.Windows.Input;

namespace AsyncEnumerableExtensions
{
    public interface IIncrementelLoadingCollection
    {
        ICommand LoadMoreCommand { get; }
        bool IsBusy { get; }
    }
}