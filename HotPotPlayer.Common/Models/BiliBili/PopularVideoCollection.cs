using HotPotPlayer.Bilibili.Models.Video;
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
    public class PopularVideoCollection : ObservableCollection<VideoContent>, ISupportIncrementalLoading
    {
        int _pageNum;
        readonly BiliBiliService _service;
        public PopularVideoCollection(PopularVideos data, BiliBiliService service) : base(data.List)
        {
            _pageNum = 1;
            _service = service;
            _hasMore = !data.NoMore;
        }

        private bool _hasMore;
        public bool HasMoreItems => _hasMore;

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            return AsyncInfo.Run(async (token) =>
            {
                _pageNum++;
                var dyn = await _service.API.GetPopularVideo(_pageNum);
                foreach (var item in dyn.Data.List)
                {
                    Add(item);
                }
                _hasMore = !dyn.Data.NoMore;
                return new LoadMoreItemsResult() { Count = (uint)dyn.Data.List.Count };
            });
        }
    }
}
