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
    public class RecVideoCollection : ObservableCollection<HomeDataItem>, ISupportIncrementalLoading
    {
        int _pageNum;
        readonly BiliBiliService _service;
        public RecVideoCollection(HomeData data, BiliBiliService service) : base(data.Items)
        {
            _pageNum = 1;
            _service = service;
            _hasMore = true;
        }

        private bool _hasMore;
        public bool HasMoreItems => _hasMore;

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            return AsyncInfo.Run(async (token) =>
            {
                _pageNum++;
                var dyn = await _service.API.GetRecVideo(_pageNum);
                foreach (var item in dyn.Data.Items)
                {
                    Add(item);
                }
                _hasMore = true;
                return new LoadMoreItemsResult() { Count = (uint)dyn.Data.Items.Count };
            });
        }
    }
}
