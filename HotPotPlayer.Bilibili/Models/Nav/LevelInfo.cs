using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Bilibili.Models.Nav
{
    public class LevelInfo
    {
        [JsonProperty("current_level")] public int CurrentLevel { get; set; }
        [JsonProperty("current_min")] public int CurrentMin { get; set; }
        [JsonProperty("current_exp")] public int CurrentExp { get; set; }
        [JsonProperty("next_exp")] public string? NextExp { get; set; }

        public string GetCurrentLevel => "LV" + CurrentLevel;
        public string GetNextLevel => CurrentLevel == 7 ? "--" : "LV" + (CurrentLevel + 1);

        public int GetNextExp => CurrentLevel == 7 ? 0 : int.Parse(NextExp!);
        public string GetLevelMessage => CurrentLevel == 7 ? "--" : $"当前成长{CurrentExp}，距离升级{GetNextLevel}还需要{int.Parse(NextExp!) - CurrentExp}";
    }
}
