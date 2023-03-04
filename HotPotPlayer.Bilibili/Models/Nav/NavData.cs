using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Bilibili.Models.Nav
{
    public class NavData
    {
        [JsonProperty("isLogin")] public bool? IsLogin { get; set; }
        [JsonProperty("email_verified")] public int EmailVerified { get; set; }
        [JsonProperty("face")] public string? Face { get; set; }
        [JsonProperty("level_info")] public LevelInfo? LevelInfo { get; set; }
        [JsonProperty("mid")] public string? Mid { get; set; }
        [JsonProperty("money")] public int Money { get; set; }
        [JsonProperty("moral")] public int Moral { get; set; }
        [JsonProperty("pendant")] public Pendant? Pendant { get; set; }
        [JsonProperty("uname")] public string? UName { get; set; }
        [JsonProperty("vipDueDate")] public long VipDueDate { get; set; }
        [JsonProperty("vipStatus")] public int VipStatus { get; set; }
        [JsonProperty("vipType")] public int VipType { get; set; }
        [JsonProperty("wallet")] public Wallet? Wallet { get; set; }

        public bool IsVip => VipStatus == 1;

        public string GetVipTitle => VipType switch
        {
            1 => "月度大会员",
            2 => "年度大会员",
            _ => ""
        };
    }
}
