using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Bilibili.Models.Nav
{
    public class Wallet
    {
        [JsonProperty("mid")] public string? Mid { get; set; }
        [JsonProperty("bcoin_balance")] public int BCoinBalance { get; set; }
    }
}
