using System;
using System.Threading.Tasks;

namespace Hyperlist.Behaviors
{
	public interface IInfiniteScrollLoader
	{
		bool CanLoadMore { get; }
		Task LoadMoreAsync();
	}
}
