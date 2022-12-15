using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HttpClientFactoryLite;
using Microsoft.Net.Http.Headers;

namespace HotPotPlayer.Services.BiliBili
{
    public class ProxyServer : IDisposable
    {
        private readonly HttpListener _listener;
        private readonly HttpClientFactory _httpClientFactory;

        public string VideoUrl { get; set; }
        public string AudioUrl { get; set; }
        public string CookieString { get; set; }

        public ProxyServer(HttpClientFactory httpClientFactory, params string[] prefixes)
        {

            if (httpClientFactory == null)
                throw new ArgumentNullException(nameof(httpClientFactory));

            if (prefixes == null)
                throw new ArgumentNullException(nameof(prefixes));

            if (prefixes.Length == 0)
                throw new ArgumentException(null, nameof(prefixes));

            _httpClientFactory = httpClientFactory;
            Prefixes = prefixes;

            _listener = new HttpListener();
            foreach (var prefix in prefixes)
            {
                _listener.Prefixes.Add(prefix);
            }
        }

        public string[] Prefixes { get; }

        public void Start()
        {
            _listener.Start();
            _listener.BeginGetContext(ProcessRequest, null);
        }

        private async void ProcessRequest(IAsyncResult result)
        {
            if (!_listener.IsListening)
                return;

            var ctx = _listener.EndGetContext(result);
            _listener.BeginGetContext(ProcessRequest, null);
            await ProcessRequest(ctx).ConfigureAwait(false);
        }

        protected virtual async Task ProcessRequest(HttpListenerContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var url = context.Request.RawUrl switch
            {
                "/video.m4s" => VideoUrl,
                "/audio.m4s" => AudioUrl,
                _ => null,
            };

            using var _client = _httpClientFactory.CreateClient("web");
            _client.DefaultRequestHeaders.Add("Cookie", CookieString);
            //using var response = await _client.GetAsync(url).ConfigureAwait(false);
            using var os = await _client.GetStreamAsync(url).ConfigureAwait(false);
            //response.EnsureSuccessStatusCode();

            //context.Response.ProtocolVersion = response.Version;
            //context.Response.StatusCode = (int)response.StatusCode;
            //context.Response.StatusDescription = response.ReasonPhrase;

            context.Response.ProtocolVersion = Version.Parse("1.1");
            context.Response.StatusCode = 200;
            context.Response.StatusDescription = "Ok";

            //foreach (var header in response.Headers)
            //{
            //    context.Response.Headers.Add(header.Key, string.Join(", ", header.Value));
            //}

            //foreach (var header in response.Content.Headers)
            //{
            //    if (header.Key == "Content-Length") // this will be set automatically at dispose time
            //        continue;

            //    context.Response.Headers.Add(header.Key, string.Join(", ", header.Value));
            //}

            var ct = context.Response.ContentType;
            //using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            try
            {
                await os.CopyToAsync(context.Response.OutputStream).ConfigureAwait(false);
            }
            catch (HttpListenerException )
            {

            }
        }

        public void Stop() => _listener.Stop();
        public void Dispose() => ((IDisposable)_listener)?.Dispose();

        // out-of-the-box replace doesn't tell if something *was* replaced or not
        private static bool TryReplace(string input, string oldValue, string newValue, out string result)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(oldValue))
            {
                result = input;
                return false;
            }

            var oldLen = oldValue.Length;
            var sb = new StringBuilder(input.Length);
            bool changed = false;
            var offset = 0;
            for (int i = 0; i < input.Length; i++)
            {
                var c = input[i];

                if (offset > 0)
                {
                    if (c == oldValue[offset])
                    {
                        offset++;
                        if (oldLen == offset)
                        {
                            changed = true;
                            sb.Append(newValue);
                            offset = 0;
                        }
                        continue;
                    }

                    for (int j = 0; j < offset; j++)
                    {
                        sb.Append(input[i - offset + j]);
                    }

                    sb.Append(c);
                    offset = 0;
                }
                else
                {
                    if (c == oldValue[0])
                    {
                        if (oldLen == 1)
                        {
                            changed = true;
                            sb.Append(newValue);
                        }
                        else
                        {
                            offset = 1;
                        }
                        continue;
                    }

                    sb.Append(c);
                }
            }

            if (changed)
            {
                result = sb.ToString();
                return true;
            }

            result = input;
            return false;
        }
    }
}
