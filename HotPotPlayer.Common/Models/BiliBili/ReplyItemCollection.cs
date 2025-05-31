using DirectN;
using HotPotPlayer.Bilibili.Models.Reply;
using HotPotPlayer.Services;
using Microsoft.UI.Xaml.Data;
using Richasy.BiliKernel.Models;
using Richasy.BiliKernel.Models.Comment;
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
    public partial class ReplyItemCollection(BiliBiliService service) : ObservableCollection<CommentInformation>, ISupportIncrementalLoading
    {
        readonly BiliBiliService _service = service;

        public long Offset { get; set; }
        public CommentTargetType Type { get; set; }
        public string Oid { get; set; }
        public string RootId { get; set; }
        public CommentSortType Sort { get; set; }
        public bool IsDetail { get; set; }

        private bool _hasMore = true;
        public bool HasMoreItems => _hasMore;

        public void Reset()
        {
            Offset = 0;
            _hasMore = true;
        }

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            return AsyncInfo.Run(async (token) =>
            {
                CommentView dyn;
                if (IsDetail)
                {
                    dyn = await _service.GetDetailCommentsAsync(Oid, Type, Sort, RootId, Offset, token);
                }
                else
                {
                    dyn = await _service.GetCommentsAsync(Oid, Type, Sort, Offset, token);
                }
                Offset = dyn.NextOffset;
                _hasMore = !dyn.IsEnd;

                if (dyn.Comments != null && dyn.Comments.Count > 0)
                {
                    foreach (var item in dyn.Comments)
                    {
                        Add(item);
                    }
                }
                return new LoadMoreItemsResult() { Count = (uint)dyn.Comments?.Count };
            });
        }
    }
}
