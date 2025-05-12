using HotPotPlayer.Bilibili.Models.Dynamic;
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
    public partial class DynamicItemCollection : ObservableCollection<DynamicItem>, ISupportIncrementalLoading
    {
        int _pageNum;
        string _prevOffset;
        readonly BiliBiliService _service;
        public DynamicItemCollection(DynamicData data, BiliBiliService service) : base(data.DynamicItems)
        {
            _pageNum = 1;
            _prevOffset = data.OffSet;
            _service = service;
            _hasMore = data.HasMore;
        }

        private bool _hasMore;
        public bool HasMoreItems => _hasMore;

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            return AsyncInfo.Run(async (token) =>
            {
                _pageNum++;
                var dyn = await _service.API.GetDynamic(DynamicType.All, _prevOffset, _pageNum);
                foreach (var item in dyn.Data.DynamicItems)
                {
                    Add(item);
                }
                _prevOffset = dyn.Data.OffSet;
                _hasMore = dyn.Data.HasMore;
                return new LoadMoreItemsResult() { Count = (uint)dyn.Data.DynamicItems.Count };
            });
        }
    }
}
