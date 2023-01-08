using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Services.BiliBili.Video
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CodecStrategy
    {
        Default,
        AV1First,
        HEVCFirst,
        AVCFirst
    }
}
