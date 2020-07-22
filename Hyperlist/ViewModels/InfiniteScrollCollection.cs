using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using DynamicData;
using DynamicData.Binding;
using Hyperlist.Behaviors;

namespace Hyperlist.ViewModels
{
    public class InfiniteScrollCollection<T> : ObservableCollectionExtended<T>, IInfiniteScrollLoader, IInfiniteScrollLoading, IInfiniteScrollDetector
    {
        private bool isLoadingMore;
        private readonly ScrollDirection scrollDirection;

        public InfiniteScrollCollection(ScrollDirection scrollDirection = ScrollDirection.End)
        {
            this.scrollDirection = scrollDirection;
        }

        public InfiniteScrollCollection(IEnumerable<T> collection, ScrollDirection scrollDirection = ScrollDirection.End)
            : base(collection)
        {
            this.scrollDirection = scrollDirection;
        }

        public int ShouldLoadMoreThreshold { get; set; }

        public Action OnBeforeLoadMore { get; set; }

        public Action OnAfterLoadMore { get; set; }

        public Action<Exception> OnError { get; set; }

        public Func<bool> OnCanLoadMore { get; set; }

        public Func<Task<IEnumerable<T>>> OnLoadMore { get; set; }

        public virtual bool CanLoadMore => OnCanLoadMore?.Invoke() ?? true;

        public async Task LoadMoreAsync()
        {
            try
            {
                IsLoadingMore = true;
                OnBeforeLoadMore?.Invoke();

                var result = await OnLoadMore();

                if (result != null)
                    AddRange(result);
            }
            catch (Exception ex) when (OnError != null)
            {
                OnError.Invoke(ex);
            }
            finally
            {
                IsLoadingMore = false;
                OnAfterLoadMore?.Invoke();
            }
        }

        public bool ShouldLoadMore(object currentItem)
        {
            if (scrollDirection == ScrollDirection.Start)
            {
                return Items.IndexOf((T)currentItem) < ShouldLoadMoreThreshold;
            }
            else
            {
                var index = Items.IndexOf((T)currentItem);
                var shouldLoadMore = Items.Count - index < ShouldLoadMoreThreshold;
                return shouldLoadMore;
            }
        }

        public bool IsLoadingMore
        {
            get => isLoadingMore;
            private set
            {
                if (isLoadingMore != value)
                {
                    isLoadingMore = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsLoadingMore)));

                    LoadingMore?.Invoke(this, new LoadingMoreEventArgs(IsLoadingMore));
                }
            }
        }

        public ScrollDirection ScrollDirection => this.scrollDirection;

        public event EventHandler<LoadingMoreEventArgs> LoadingMore;
    }
}
