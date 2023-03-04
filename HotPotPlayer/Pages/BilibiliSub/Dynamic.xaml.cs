// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using CommunityToolkit.Mvvm.ComponentModel;
using HotPotPlayer.Bilibili.Models.Dynamic;
using HotPotPlayer.Bilibili.Models.Reply;
using HotPotPlayer.Models.BiliBili;
using HotPotPlayer.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.Pages.BilibiliSub
{
    public sealed partial class Dynamic : UserControlBase
    {
        public Dynamic()
        {
            this.InitializeComponent();
        }

        [ObservableProperty]
        private DynamicItemCollection dynamicItems;

        [ObservableProperty]
        private ReplyItemCollection replies;

        private DynamicItem currentOpen;

        [ObservableProperty]
        private bool isCommentsOpen;

        bool isFirstLoad = true;

        public async void LoadDynamicAsync(bool force = false)
        {
            if (!force && !isFirstLoad)
            {
                return;
            }
            var dynamicData = (await BiliBiliService.API.GetDynamic(DynamicType.All)).Data;
            DynamicItems = new DynamicItemCollection(dynamicData, BiliBiliService);
            isFirstLoad = false;
        }

        public void ToggleComment(DynamicItem dyn)
        {
            if (IsCommentsOpen)
            {
                if (dyn != currentOpen)
                {
                    LoadReplies(dyn);
                }
                else
                {
                    IsCommentsOpen = !IsCommentsOpen;
                }
            }
            else
            {
                LoadReplies(dyn);
                IsCommentsOpen = !IsCommentsOpen;
            }
        }

        public async void LoadReplies(DynamicItem dyn)
        {
            Replies re;
            if (dyn.Modules.ModuleDynamic.HasArchive)
            {
                re = (await BiliBiliService.API.GetVideoReplyAsync(dyn.Modules.ModuleDynamic.Major.Archive.Aid)).Data;
                Replies = new ReplyItemCollection(re, "1", dyn.Modules.ModuleDynamic.Major.Archive.Aid, BiliBiliService);
            }
            else if (dyn.Modules.ModuleDynamic.HasArticle)
            {
                re = (await BiliBiliService.API.GetArtileDynamicReplyAsync(dyn.Modules.ModuleDynamic.Major.Article.Id)).Data;
                Replies = new ReplyItemCollection(re, "12", dyn.Modules.ModuleDynamic.Major.Article.Id, BiliBiliService);
            }
            else if (dyn.Modules.ModuleDynamic.HasDraw)
            {
                re = (await BiliBiliService.API.GetPictureDynamicReplyAsync(dyn.Modules.ModuleDynamic.Major.Draw.ID)).Data;
                Replies = new ReplyItemCollection(re, "11", dyn.Modules.ModuleDynamic.Major.Draw.ID, BiliBiliService);
            }
            else
            {
                re = (await BiliBiliService.API.GetTextDynamicReplyAsync(dyn.Id)).Data;
                Replies = new ReplyItemCollection(re, "17", dyn.Id, BiliBiliService);
            }
            currentOpen = dyn;
        }

        private async void DynamicItemClick(object sender, ItemClickEventArgs e)
        {
            var v = e.ClickedItem as DynamicItem;
            //VideoPlayer.Close();
            //await StartPlay(v);
            if (v.Modules.ModuleDynamic?.Major?.Archive != null)
            {
                var bvid = v.Modules.ModuleDynamic.Major.Archive.Bvid;
                NavigateTo("BilibiliSub.BiliVideoPlay", bvid);
            }
            else if(v.HasOrigin && v.Origin.Modules.ModuleDynamic?.Major?.Archive != null)
            {
                var bvid = v.Origin.Modules.ModuleDynamic.Major.Archive.Bvid;
                NavigateTo("BilibiliSub.BiliVideoPlay", bvid);
            }
            else if(v.Modules.ModuleDynamic?.Major?.Article != null)
            {
                var url = v.Modules.ModuleDynamic.Major.Article.JumpUrl;
                await Launcher.LaunchUriAsync(new Uri("https:" + url));
            }
            else if(v.Modules.ModuleDynamic?.Major?.LiveRcmd != null)
            {
                var url = v.Modules.ModuleDynamic.Major.LiveRcmd.GetLink;
                await Launcher.LaunchUriAsync(new Uri(url));
            }

        }
    }
}
