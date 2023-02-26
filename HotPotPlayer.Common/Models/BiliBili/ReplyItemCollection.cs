using HotPotPlayer.Bilibili.Models.Reply;
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
    public class ReplyItemCollection : ObservableCollection<Reply>, ISupportIncrementalLoading
    {
        int _pageNum;
        readonly BiliBiliService _service;
        readonly string _type;
        readonly string _oid;
        public ReplyItemCollection(Replies data, string type, string oid, BiliBiliService service) : base(data.TheReplies)
        {
            _pageNum = 0;
            _service = service;
            _hasMore = !data.Cursor.IsEnd;
            _type = type;
            _oid = oid;
        }

        private bool _hasMore;
        public bool HasMoreItems => _hasMore;

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            return AsyncInfo.Run(async (token) =>
            {
                _pageNum++;
                var dyn = await _service.API.GetReplyAsync(_type, _oid, _pageNum);
                foreach (var item in dyn.Data.TheReplies)
                {
                    Add(item);
                }
                _hasMore = !dyn.Data.Cursor.IsEnd;
                return new LoadMoreItemsResult() { Count = (uint)dyn.Data.TheReplies.Count };
            });
        }
    }
}
