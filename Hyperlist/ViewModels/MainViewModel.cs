using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using DynamicData;
using DynamicData.Binding;
using Hyperlist.Models;
using Hyperlist.Services;
using ReactiveUI;

namespace Hyperlist.ViewModels
{
    public class MainViewModel : ReactiveObject
    {
        private readonly ObservableAsPropertyHelper<bool> _isLoadingItems;
        private readonly SourceList<ShowDto> _showsList = new SourceList<ShowDto>();
        private int _pageNr = 2;

        public MainViewModel()
        {
            _movieService = new MovieService();
            LoadNextPageCommand = ReactiveCommand.CreateFromTask<int, List<ShowDto>>(async _ =>
            {
                var items = await _movieService.LoadPage(_pageNr);
                await Task.Delay(1000);
                return items;
            });

            LoadNextPageCommand.IsExecuting.BindTo(this, vm => vm.IsBusy);

            LoadNextPageCommand.Subscribe(shows =>
            {
                _showsList.AddRange(shows);
            });

            LoadNextPageCommand.Execute(_pageNr).Subscribe();

            LoadNextPageCommand.IsExecuting
                .ToProperty(this, vm => vm.IsLoadingItems, out _isLoadingItems);

            _showsList
                .Connect()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Sort(SortExpressionComparer<ShowDto>.Ascending(a => a.Id))
                .Bind(Shows)
                .Subscribe();

            Shows.ShouldLoadMoreThreshold = 100;
            Shows.OnLoadMore += OnLoadMore;
            Shows.OnCanLoadMore += OnCanLoadMore;
        }

        private bool OnCanLoadMore()
        {
            return _pageNr >= 0;
        }

        private async Task<IEnumerable<ShowDto>> OnLoadMore()
        {
            _pageNr--;
            await LoadNextPageCommand.Execute(_pageNr).ToTask();
            return new List<ShowDto>();
        }

        private MovieService _movieService;
        private bool isBusy;

        public ReactiveCommand<int, List<ShowDto>> LoadNextPageCommand { get; }
        public InfiniteScrollCollection<ShowDto> Shows { get; } = new InfiniteScrollCollection<ShowDto>(ScrollDirection.Start);
        public bool IsLoadingItems => _isLoadingItems.Value;

        public bool IsBusy
        {
            get => isBusy;
            private set
            {
                this.RaiseAndSetIfChanged(ref isBusy, value);
            }
        }
    }
}