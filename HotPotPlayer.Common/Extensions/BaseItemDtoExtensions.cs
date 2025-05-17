using Jellyfin.Sdk.Generated.Models;
using Newtonsoft.Json;
using Richasy.BiliKernel.Models.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Extensions
{
    public static class BaseItemDtoExtensions
    {
        public static BaseItemDto ToBaseItemDto(this VideoInformation v)
        {
            return new BaseItemDto
            {
                Name = v.Identifier.Title,
                PlaylistItemId = v.Identifier.Id,
                IsFolder = false,
                Etag = "Bilibili",
                Overview = v.Identifier.Cover.Uri.ToString()
            };
        }
    }
}
