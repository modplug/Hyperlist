using System;
using Hyperlist.ViewModels;

namespace Hyperlist.Behaviors
{
	public interface IInfiniteScrollDetector
	{
		bool ShouldLoadMore(object currentItem);
		ScrollDirection ScrollDirection { get; }
		int ShouldLoadMoreThreshold { get; }
	}
}
