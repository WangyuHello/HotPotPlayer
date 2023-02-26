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
        int _max;
        readonly BiliBiliService _service;

        public HistoryCollection(HistoryData data, BiliBiliService service) : base(data.List)
        {
            _max = data.Cursor.Max;
            _service = service;
        }

        public bool HasMoreItems => true;

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            return AsyncInfo.Run(async (token) =>
            {
                var his = await _service.API.History(_max);
                _max = his.Data.Cursor.Max;
                foreach (var item in his.Data.List)
                {
                    Add(item);
                }
                return new LoadMoreItemsResult() { Count = (uint)his.Data.List.Count };
            });
        }
    }
}
