﻿using HotPotPlayer.Services;
using Jellyfin.Sdk.Generated.Models;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace HotPotPlayer.Models
{
    public class ArtistCollection(JellyfinMusicService service) : ObservableCollection<BaseItemDto>, ISupportIncrementalLoading
    {
        readonly JellyfinMusicService _service = service;
        int _pageNum;
        const int _perPageItem = 50;
        bool _hasMore = true;
        public bool HasMoreItems => _hasMore;

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            return AsyncInfo.Run(async (token) =>
            {
                var list = await _service.GetJellyfinArtistListAsync(_pageNum * _perPageItem, _perPageItem);
                _pageNum++;
                if (list == null)
                {
                    _hasMore = false;
                    return new LoadMoreItemsResult() { Count = 0 };
                }
                else if (list.Count < _perPageItem)
                {
                    _hasMore = false;
                    foreach (var item in list)
                    {
                        Add(item);
                    }
                    return new LoadMoreItemsResult() { Count = (uint)list.Count };
                }
                else
                {
                    _hasMore = true;
                    foreach (var item in list)
                    {
                        Add(item);
                    }
                    return new LoadMoreItemsResult() { Count = (uint)list.Count };
                }
            });
        }
    }
}
