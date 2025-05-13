using HotPotPlayer.Bilibili.Models.HomeVideo;
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
    public partial class RecVideoCollection(BiliBiliService service) : ObservableCollection<VideoInformation>, ISupportIncrementalLoading
    {
        readonly BiliBiliService _service = service;

        long _offset;
        private bool _hasMore = true;
        public bool HasMoreItems => _hasMore;

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            return AsyncInfo.Run(async (token) =>
            {
                var (items, offset) = await _service.GetRecommendVideoListAsync(_offset, token);
                _offset = offset;

                if (items == null || items.Count == 0)
                {
                    _hasMore = false;
                    return new LoadMoreItemsResult() { Count = 0 };
                }
                else
                {
                    _hasMore = true;
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
