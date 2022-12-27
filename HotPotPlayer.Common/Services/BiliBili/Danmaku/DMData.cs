using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.UI;

namespace HotPotPlayer.Services.BiliBili.Danmaku
{
    public class DMData
    {
        public string ChatServer { get; set; }
        public long ChatId { get; set; }

        public List<DMItem> Dms { get; set; }

        public DMData(string xml) 
        {
            Dms = new List<DMItem>();
            var x = XElement.Parse(xml);
            foreach (var item in x.Descendants())
            {
                if (item.Name == "d")
                {
                    var p = item.Attribute("p").Value;
                    var segs = p.Split(',');
                    var timeStr = segs[0];
                    var time = TimeSpan.FromSeconds(double.Parse(timeStr));
                    var content = item.FirstNode.ToString();
                    Dms.Add(new DMItem
                    {
                        Time = time,
                        Content = content
                    });
                }
            }
        }
    }

    public class DMItem
    {
        public TimeSpan Time { get; set; }
        public int Type { get; set; }
        public int FontSize { get; set; }
        public Color Color { get; set; }
        public DateTime SendTime { get; set; }
        public int PoolType { get; set; }

        public string Content { get; set; }
    }
}
