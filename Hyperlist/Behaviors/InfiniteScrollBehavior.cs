using System;
using System.Collections;
using Hyperlist.ViewModels;
using Xamarin.Forms;

namespace Hyperlist.Behaviors
{
	public class InfiniteScrollBehavior : Behavior<CollectionView>
	{
		public static readonly BindableProperty IsLoadingMoreProperty =
			BindableProperty.Create(
				nameof(IsLoadingMore),
				typeof(bool),
				typeof(InfiniteScrollBehavior),
				default(bool),
				BindingMode.OneWayToSource);

		private static readonly BindableProperty ItemsSourceProperty =
			BindableProperty.Create(
				nameof(ItemsSource),
				typeof(IEnumerable),
				typeof(InfiniteScrollBehavior),
				default(IEnumerable),
				BindingMode.OneWay,
				propertyChanged: OnItemsSourceChanged);

		private bool isLoadingMoreFromScroll;
		private bool isLoadingMoreFromLoader;
		private CollectionView associatedCollectionView;

		public bool IsLoadingMore
		{
			get => (bool)GetValue(IsLoadingMoreProperty);
			private set => SetValue(IsLoadingMoreProperty, value);
		}

		private IEnumerable ItemsSource => (IEnumerable)GetValue(ItemsSourceProperty);

		protected override void OnAttachedTo(CollectionView bindable)
		{
			base.OnAttachedTo(bindable);

			associatedCollectionView = bindable;

			SetBinding(ItemsSourceProperty, new Binding(ItemsSourceProperty.PropertyName, source: associatedCollectionView));

			bindable.BindingContextChanged += OnListViewBindingContextChanged;
			bindable.Scrolled += OnCollectionViewScrolled;
			BindingContext = associatedCollectionView.BindingContext;
		}

        private async void OnCollectionViewScrolled(object sender, ItemsViewScrolledEventArgs args)
        {
			if (IsLoadingMore)
            {
				return;
			}

			if (associatedCollectionView.ItemsSource is IInfiniteScrollLoader loader)
            {
				if (associatedCollectionView.ItemsSource is IList list)
				{
					if (list.Count <= 0)
                    {
						return;
                    }

                    var firstVisibleItem = list[args.FirstVisibleItemIndex];
                    var lastVisibleItem = list[args.LastVisibleItemIndex];

					var scrollDirection = ScrollDirection.End;
					int threshold = 0;
					if (associatedCollectionView.ItemsSource is IInfiniteScrollDetector detector)
                    {
						scrollDirection = detector.ScrollDirection;
						threshold = detector.ShouldLoadMoreThreshold;
                    }

					var lastItem = list[list.Count - 1];
                    object item = null;
					bool autoScroll = true;

					// Did we scroll to the very top?
					if (scrollDirection == ScrollDirection.Start && args.VerticalOffset <= 0)
                    {
                        item = firstVisibleItem;
                    }
					// Did we scroll to the very end?
                    else if (scrollDirection == ScrollDirection.End && args.VerticalDelta > 0 && lastVisibleItem == lastItem)
                    {
                        item = lastVisibleItem;
                    }
                    else
                    {
                        // Don't need to do anything if we are scrolling in the opposite direction
                        if (scrollDirection == ScrollDirection.Start && args.VerticalDelta > 0 ||
							scrollDirection == ScrollDirection.End && args.VerticalDelta < 0) 
                        {
                            return;
                        }

						// Don't need to do anything if we are outside the scope of the threshold
                        if (args.VerticalDelta > 0 && args.LastVisibleItemIndex < threshold ||
							args.VerticalDelta < 0 && args.FirstVisibleItemIndex > threshold)
						{
                            return;
                        }

                        item = scrollDirection == ScrollDirection.End ? lastVisibleItem : firstVisibleItem;
                        autoScroll = scrollDirection == ScrollDirection.Start;
                    }

                    if (loader.CanLoadMore && ShouldLoadMore(item))
					{
						UpdateIsLoadingMore(true, null);
						await loader.LoadMoreAsync();
						UpdateIsLoadingMore(false, null);

						// Autoscroll to the top item to keep the scroll offset after loading new items
						// This is a workaround for a Xamarin Forms bug where KeepItemsInView doesn't work.
						if (autoScroll)
                        {
							associatedCollectionView.ScrollTo(item, -1, scrollDirection == ScrollDirection.Start ? ScrollToPosition.Start : ScrollToPosition.End, animate: false);
						}
					}
				}
			}
        }

        protected override void OnDetachingFrom(CollectionView bindable)
		{
			RemoveBinding(ItemsSourceProperty);

			bindable.BindingContextChanged -= OnListViewBindingContextChanged;
			bindable.Scrolled -= OnCollectionViewScrolled;

			base.OnDetachingFrom(bindable);
		}

		private void OnListViewBindingContextChanged(object sender, EventArgs e)
		{
			BindingContext = associatedCollectionView.BindingContext;
		}

		private bool ShouldLoadMore(object item)
		{
			if (associatedCollectionView.ItemsSource is IInfiniteScrollDetector detector)
				return detector.ShouldLoadMore(item);
			if (associatedCollectionView.ItemsSource is IList list)
			{
				if (list.Count == 0)
					return true;
				var lastItem = list[list.Count - 1];
				if (associatedCollectionView.IsGrouped && lastItem is IList group)
					return group.Count == 0 || group[group.Count - 1] == item;
				else
					return lastItem == item;

			}
			return false;
		}

		private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (bindable is InfiniteScrollBehavior behavior)
			{
				if (oldValue is IInfiniteScrollLoading oldLoading)
				{
					oldLoading.LoadingMore -= behavior.OnLoadingMore;
					behavior.UpdateIsLoadingMore(null, false);
				}
				if (newValue is IInfiniteScrollLoading newLoading)
				{
					newLoading.LoadingMore += behavior.OnLoadingMore;
					behavior.UpdateIsLoadingMore(null, newLoading.IsLoadingMore);
				}
			}
		}

		private void OnLoadingMore(object sender, LoadingMoreEventArgs e)
		{
			UpdateIsLoadingMore(null, e.IsLoadingMore);
		}

		private void UpdateIsLoadingMore(bool? fromScroll, bool? fromLoader)
		{
			isLoadingMoreFromScroll = fromScroll ?? isLoadingMoreFromScroll;
			isLoadingMoreFromLoader = fromLoader ?? isLoadingMoreFromLoader;

			IsLoadingMore = isLoadingMoreFromScroll || isLoadingMoreFromLoader;
		}
	}
}

