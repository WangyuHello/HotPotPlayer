﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Video.Util
{
    public class ProxyServer : IDisposable
    {
        private readonly HttpListener _listener;
        private readonly int _targetPort;
        private readonly string _targetHost;
        private readonly HttpClient _client;

        public ProxyServer(string targetUrl, HttpClient client, params string[] prefixes)
            : this(new Uri(targetUrl), client, prefixes)
        {

        }

        public ProxyServer(Uri targetUrl, HttpClient client, params string[] prefixes)
        {
            if (targetUrl == null)
                throw new ArgumentNullException(nameof(targetUrl));

            if (client == null)
                throw new ArgumentNullException(nameof(client));

            if (prefixes == null)
                throw new ArgumentNullException(nameof(prefixes));

            if (prefixes.Length == 0)
                throw new ArgumentException(null, nameof(prefixes));

            _client = client;
            RewriteTargetInText = true;
            RewriteHost = true;
            RewriteReferer = true;
            TargetUrl = targetUrl;
            _targetHost = targetUrl.Host;
            _targetPort = targetUrl.Port;
            Prefixes = prefixes;

            _listener = new HttpListener();
            foreach (var prefix in prefixes)
            {
                _listener.Prefixes.Add(prefix);
            }
        }

        public Uri TargetUrl { get; }
        public string[] Prefixes { get; }
        public bool RewriteTargetInText { get; set; }
        public bool RewriteHost { get; set; }
        public bool RewriteReferer { get; set; } // this can have performance impact...

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

            var url = TargetUrl.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped);
            using (var msg = new HttpRequestMessage(new HttpMethod(context.Request.HttpMethod), url + context.Request.RawUrl))
            {
                msg.Version = context.Request.ProtocolVersion;

                if (context.Request.HasEntityBody)
                {
                    msg.Content = new StreamContent(context.Request.InputStream); // disposed with msg
                }

                string host = null;
                foreach (string headerName in context.Request.Headers)
                {
                    var headerValue = context.Request.Headers[headerName];
                    if (headerName == "Content-Length" && headerValue == "0") // useless plus don't send if we have no entity body
                        continue;

                    bool contentHeader = false;
                    switch (headerName)
                    {
                        // some headers go to content...
                        case "Allow":
                        case "Content-Disposition":
                        case "Content-Encoding":
                        case "Content-Language":
                        case "Content-Length":
                        case "Content-Location":
                        case "Content-MD5":
                        case "Content-Range":
                        case "Content-Type":
                        case "Expires":
                        case "Last-Modified":
                            contentHeader = true;
                            break;

                        case "Referer":
                            if (RewriteReferer && Uri.TryCreate(headerValue, UriKind.Absolute, out var referer)) // if relative, don't handle
                            {
                                var builder = new UriBuilder(referer);
                                builder.Host = TargetUrl.Host;
                                builder.Port = TargetUrl.Port;
                                headerValue = builder.ToString();
                            }
                            break;

                        case "Host":
                            host = headerValue;
                            if (RewriteHost)
                            {
                                headerValue = TargetUrl.Host + ":" + TargetUrl.Port;
                            }
                            break;
                    }

                    if (contentHeader)
                    {
                        msg.Content.Headers.Add(headerName, headerValue);
                    }
                    else
                    {
                        msg.Headers.Add(headerName, headerValue);
                    }
                }

                using (var response = await _client.SendAsync(msg).ConfigureAwait(false))
                {
                    using (var os = context.Response.OutputStream)
                    {
                        context.Response.ProtocolVersion = response.Version;
                        context.Response.StatusCode = (int)response.StatusCode;
                        context.Response.StatusDescription = response.ReasonPhrase;

                        foreach (var header in response.Headers)
                        {
                            context.Response.Headers.Add(header.Key, string.Join(", ", header.Value));
                        }

                        foreach (var header in response.Content.Headers)
                        {
                            if (header.Key == "Content-Length") // this will be set automatically at dispose time
                                continue;

                            context.Response.Headers.Add(header.Key, string.Join(", ", header.Value));
                        }

                        var ct = context.Response.ContentType;
                        if (RewriteTargetInText && host != null && ct != null &&
                            (ct.IndexOf("text/html", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            ct.IndexOf("application/json", StringComparison.OrdinalIgnoreCase) >= 0))
                        {
                            using (var ms = new MemoryStream())
                            {
                                using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                                {
                                    await stream.CopyToAsync(ms).ConfigureAwait(false);
                                    var enc = context.Response.ContentEncoding ?? Encoding.UTF8;
                                    var html = enc.GetString(ms.ToArray());
                                    if (TryReplace(html, "//" + _targetHost + ":" + _targetPort + "/", "//" + host + "/", out var replaced))
                                    {
                                        var bytes = enc.GetBytes(replaced);
                                        using (var ms2 = new MemoryStream(bytes))
                                        {
                                            ms2.Position = 0;
                                            await ms2.CopyToAsync(context.Response.OutputStream).ConfigureAwait(false);
                                        }
                                    }
                                    else
                                    {
                                        ms.Position = 0;
                                        await ms.CopyToAsync(context.Response.OutputStream).ConfigureAwait(false);
                                    }
                                }
                            }
                        }
                        else
                        {
                            using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                            {
                                await stream.CopyToAsync(context.Response.OutputStream).ConfigureAwait(false);
                            }
                        }
                    }
                }
            }
        }

        public void Stop() => _listener.Stop();
        public override string ToString() => string.Join(", ", Prefixes) + " => " + TargetUrl;
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
