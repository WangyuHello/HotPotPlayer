using HotPotPlayer.Services;
using Microsoft.UI.Xaml.Data;
using Richasy.BiliKernel.Models.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace HotPotPlayer.Models.BiliBili
{
    public partial class SearchVideoCollection(BiliBiliService service) : ObservableCollection<VideoInformation>, ISupportIncrementalLoading
    {
        readonly BiliBiliService _service = service;

        public string Keyword { get; set; }

        public void Reset()
        {
            _pageNum = null;
            _hasMore = true;
            Clear();
        }

        int? _pageNum;
        private bool _hasMore = true;
        public bool HasMoreItems => _hasMore;

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            return AsyncInfo.Run(async (token) =>
            {
                var (items, nextPage) = await _service.GetComprehensiveSearchResultAsync(Keyword, _pageNum, Richasy.BiliKernel.Models.ComprehensiveSearchSortType.Default, token);

                if (nextPage == null)
                {
                    _hasMore = false;
                    return new LoadMoreItemsResult() { Count = 0 };
                }
                _pageNum = nextPage;
                _hasMore = true;

                if (items == null || items.Count == 0)
                {
                    return new LoadMoreItemsResult() { Count = 0 };
                }
                else
                {
                    foreach (var item in items)
                    {
                        Add(item);
                    }
                    return new LoadMoreItemsResult() { Count = (uint)items.Count };
                }
            });
        }
    }
}
