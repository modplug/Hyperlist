using System;
namespace Hyperlist.Behaviors
{
	public interface IInfiniteScrollLoading
	{
		bool IsLoadingMore { get; }

		event EventHandler<LoadingMoreEventArgs> LoadingMore;
	}
}
