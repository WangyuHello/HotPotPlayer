using HotPotPlayer.Bilibili.Models.History;
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
    public class HistoryCollection : ObservableCollection<HistoryItem>, ISupportIncrementalLoading
    {
        HistoryCursor _cursor;
        readonly BiliBiliService _service;

        public HistoryCollection(HistoryData data, BiliBiliService service) : base(data == null ? Enumerable.Empty<HistoryItem>() : data.List)
        {
            if (data == null)
            {
                _hasMore = false;
            }
            else
            {
                _hasMore = true;
                _cursor = data.Cursor;
            }
            _service = service;
        }

        private bool _hasMore;
        public bool HasMoreItems => _hasMore;

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            return AsyncInfo.Run(async (token) =>
            {
                var his = await _service.API.History(_cursor.Max, _cursor.Business, _cursor.ViewAt);
                if (his.Data == null)
                {
                    _hasMore = false;
                    foreach (var item in his.Data.List)
                    {
                        Add(item);
                    }
                    return new LoadMoreItemsResult() { Count = 0 };
                }
                else
                {
                    _cursor = his.Data.Cursor;
                    foreach (var item in his.Data.List)
                    {
                        Add(item);
                    }
                    return new LoadMoreItemsResult() { Count = (uint)his.Data.List.Count };
                }

            });
        }
    }
}
