using HotPotPlayer.Bilibili.Models.HomeVideo;
using HotPotPlayer.Services;
using Microsoft.UI.Xaml.Data;
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
    public class RecVideoCollection : ObservableCollection<RecommendVideoItem>, ISupportIncrementalLoading
    {
        int _pageNum;
        readonly BiliBiliService _service;
        public RecVideoCollection(RecommendVideoData data, BiliBiliService service) : base(data == null ? Enumerable.Empty<RecommendVideoItem>() : data.Items)
        {
            _pageNum = 1;
            _service = service;
            _hasMore = data == null ? false: true;
        }

        private bool _hasMore;
        public bool HasMoreItems => _hasMore;

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            return AsyncInfo.Run(async (token) =>
            {
                _pageNum++;
                var dyn = await _service.API.GetRecVideo(_pageNum);
                if (dyn.Data == null)
                {
                    _hasMore = false;
                    return new LoadMoreItemsResult() { Count = 0 };
                }
                else
                {
                    _hasMore = true;
                    foreach (var item in dyn.Data.Items)
                    {
                        Add(item);
                    }
                    return new LoadMoreItemsResult() { Count = (uint)dyn.Data.Items.Count };
                }
            });
        }
    }
}
