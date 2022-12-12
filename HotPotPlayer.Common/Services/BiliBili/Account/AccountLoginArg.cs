using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BiliBiliAPI.Models.Account
{
    public record AccountLoginData
    {
        /// <summary>
        /// 二维码Url
        /// </summary>
        [JsonProperty("url")]
        public string PicUrl { get; set; }

        /// <summary>
        /// 校验码
        /// </summary>

        [JsonProperty("auth_code")]
        public string QRKey { get; set; }
    }

    public record LoginTrueString
    {
        /// <summary>
        /// 携带的json字符串
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// 检查状态
        /// </summary>
        public Checkenum Check { get; set; }

        /// <summary>
        /// 返回的Cookie信息
        /// </summary>
        public string Cookie { get; set; }

    }

    /// <summary>
    /// 检查状态
    /// </summary>
    public enum Checkenum
    {
        /// <summary>
        /// 超时
        /// </summary>
        OnTime,
        /// <summary>
        /// Post错误
        /// </summary>
        Post,
        /// <summary>
        /// 不知名报错
        /// </summary>
        NULL,
        /// <summary>
        /// 登录成功
        /// </summary>
        Yes,
        /// <summary>
        /// 未扫码
        /// </summary>
        No,
        /// <summary>
        /// 扫了码未确定
        /// </summary>
        YesOrNo
    }

    /// <summary>
    /// 登录cookie
    /// </summary>
    public record AccountToken
    {
        /// <summary>
        /// 你滴账号
        /// </summary>
        [JsonProperty("mid")]
        public string Mid { get; set; } = "";

        /// <summary>
        /// 你滴访问权限
        /// </summary>
        [JsonProperty("access_token")]
        public string SECCDATA { get; set; } = "";

        /// <summary>
        /// 用来刷新Token的字符串
        /// </summary>
        [JsonProperty("refresh_token")]
        public string RefToken { get; set; }

        /// <summary>
        /// 有效时间
        /// </summary>
        [JsonProperty("expires_in")]
        public string Expires_in { get; set; }



        [JsonProperty("token_info")]
        public AccountToken Info { get; set; }

        [JsonProperty("cookie_info")]
        public AccountTokenCookies cookies { get; set; }

        public string CookieString
        {
            get
            {
                if (cookies == null)
                    return null;
                string str = "";
                foreach (var item in cookies.Cookies)
                {
                    str += $"{item.Name}={item.Value};";
                }
                return str;
            }
        }

        [JsonProperty("sso")]
        public string[] SSO { get; set; }
    }

    public record AccountTokenCookies
    {
        /// <summary>
        /// Cookie列表
        /// </summary>
        [JsonProperty("cookies")]
        public List<Cookie> Cookies { get; set; }


        /// <summary>
        /// 可跨的域
        /// </summary>

        [JsonProperty("domains")]
        public string[] Domains { get; set; }

    }

    public record Cookie
    {
        /// <summary>
        /// Cookie名称
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Cookie值
        /// </summary>

        [JsonProperty("value")]
        public string Value { get; set; }

        /// <summary>
        /// 暂时不明白
        /// </summary>

        [JsonProperty("http_only")]
        public string Http_Only { get; set; }

        /// <summary>
        /// 剩余时间？
        /// </summary>

        [JsonProperty("expires")]
        public string Expires { get; set; }
    }


}
