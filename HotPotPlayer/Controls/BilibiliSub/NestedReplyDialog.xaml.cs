// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using CommunityToolkit.Mvvm.ComponentModel;
using HotPotPlayer.Models.BiliBili;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Richasy.BiliKernel.Models.Comment;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.Controls.BilibiliSub
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NestedReplyDialog : PageBase
    {
        public NestedReplyDialog(CommentInformation reply)
        {
            Reply = reply;
            NestedReplies = new ReplyItemCollection(BiliBiliService)
            {
                IsDetail = true,
                Oid = Reply.CommentId,
                RootId = Reply.Id,
                Type = Richasy.BiliKernel.Models.CommentTargetType.Video,
                Sort = Richasy.BiliKernel.Models.CommentSortType.Hot
            };
            this.InitializeComponent();
        }

        readonly CommentInformation Reply;

        [ObservableProperty]
        public partial ReplyItemCollection NestedReplies { get; set; }
    }
}
