using HotPotPlayer.Services;
using Microsoft.UI.Xaml.Data;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using System.Collections.ObjectModel;
using HotPotPlayer.Bilibili.Models.User;

namespace HotPotPlayer.Models.BiliBili
{
    public class UserVideoInfoItemCollection : ObservableCollection<UserVideoInfoItem>, ISupportIncrementalLoading
    {
        int _pageNum;
        int _loadedCount;
        string _mid;
        readonly BiliBiliService _service;
        public UserVideoInfoItemCollection(UserVideoInfo data, string mid, BiliBiliService service) : base(data.List.VList)
        {
            _pageNum = 1;
            _service = service;
            _loadedCount = data.List.VList.Count;
            _mid = mid;
            _hasMore = data.Page.Count > _loadedCount;
        }

        private bool _hasMore;
        public bool HasMoreItems => _hasMore;

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            return AsyncInfo.Run(async (token) =>
            {
                _pageNum++;
                var dyn = (await _service.API.GetUserVideoInfo(_mid, _pageNum, 20)).Data;
                foreach (var item in dyn.List.VList)
                {
                    Add(item);
                }
                _loadedCount += dyn.List.VList.Count;
                _hasMore = dyn.Page.Count > _loadedCount;
                return new LoadMoreItemsResult() { Count = (uint)dyn.List.VList.Count };
            });
        }
    }
}
