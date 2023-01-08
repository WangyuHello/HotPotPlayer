using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Services.BiliBili.Danmaku
{
    public class Pbp
    {
        [JsonProperty("step_sec")] public int StepSec { get; set; }
        [JsonProperty("tagstr")] public string TagStr { get; set; }
        [JsonProperty("debug")] public string Debug { get; set; }
        [JsonProperty("events")] public PbpEvents Events { get; set; }

        public int MaxTime => JObject.Parse(Debug)["max_time"].Value<int>();
    }

    public class PbpEvents
    {
        [JsonProperty("default")] public List<double> Default { get; set; }
    }
}
