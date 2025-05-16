using HotPotPlayer.Services;
using Microsoft.UI.Xaml.Data;
using Richasy.BiliKernel.Models.Media;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;

namespace HotPotPlayer.Models.BiliBili
{
    public partial class HistoryCollection(BiliBiliService service) : ObservableCollection<VideoInformation>, ISupportIncrementalLoading
    {
        public long Offset { get; set; }
        readonly BiliBiliService _service = service;
        private bool _hasMore = true;
        public bool HasMoreItems => _hasMore;

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            return AsyncInfo.Run(async (token) =>
            {
                var hist = await _service.GetVideoHistoryAsync(Offset, token);
                Offset = hist.Offset ?? 0;

                if (hist.Videos == null || hist.Videos.Count == 0)
                {
                    _hasMore = false;
                    return new LoadMoreItemsResult() { Count = 0 };
                }
                else
                {
                    _hasMore = true;
                    foreach (var item in hist.Videos)
                    {
                        Add(item);
                    }
                    return new LoadMoreItemsResult() { Count = (uint)hist.Videos.Count };
                }

            });
        }
    }
}
