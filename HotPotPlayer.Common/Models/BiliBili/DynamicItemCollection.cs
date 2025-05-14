using HotPotPlayer.Bilibili.Models.Dynamic;
using HotPotPlayer.Services;
using Microsoft.UI.Xaml.Data;
using Richasy.BiliKernel.Models.Moment;
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
    public partial class DynamicItemCollection(BiliBiliService service) : ObservableCollection<MomentInformation>, ISupportIncrementalLoading
    {
        readonly BiliBiliService _service = service;
        string _prevOffset;
        string _prevBaseline;
        private bool _hasMore = true;
        public bool HasMoreItems => _hasMore;

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            return AsyncInfo.Run(async (token) =>
            {
                var dyn = await _service.GetComprehensiveMomentsAsync(_prevOffset, _prevBaseline, token);
                _prevOffset = dyn.Offset;
                _hasMore = dyn.HasMoreMoments ?? true;
                _prevBaseline = dyn.UpdateBaseline ?? null;
                foreach (var item in dyn.Moments)
                {
                    Add(item);
                }
                return new LoadMoreItemsResult() { Count = (uint)dyn.Moments.Count };
            });
        }
    }
}
