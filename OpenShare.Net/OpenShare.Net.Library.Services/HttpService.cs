using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace OpenShare.Net.Library.Services
{
    public class HttpService : IHttpService
    {
        public CookieContainer Container { get; set; }

        public HttpService()
        {
            Container = new CookieContainer();
        }

        public virtual async Task<HttpResponseMessage> LoginAsync(
            string url, Dictionary<string, string> content,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (url == null)
                throw new ArgumentNullException("url");
            if (content == null)
                throw new ArgumentNullException("content");

            if (string.IsNullOrWhiteSpace(url))
                throw new Exception(string.Format("Invalid url value: {0}", url));

            using (var httpClientHandler = new HttpClientHandler { UseProxy = false, CookieContainer = Container })
            {
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new FormUrlEncodedContent(content)
                    //Content = new FormUrlEncodedContent(
                    //    new Dictionary<string, string>
                    //    {
                    //        {"Username", Username},
                    //        {"Password", Password}
                    //    })
                };

                using (var httpClient = new HttpClient(httpClientHandler))
                {
                    var response = await httpClient.SendAsync(httpRequestMessage, cancellationToken);
                    return response.EnsureSuccessStatusCode();
                }
            }
        }

        public virtual async Task<HttpResponseMessage> LoginJsonAsync(
            string url, Dictionary<string, string> content,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (url == null)
                throw new ArgumentNullException("url");
            if (content == null)
                throw new ArgumentNullException("content");

            if (string.IsNullOrWhiteSpace(url))
                throw new Exception(string.Format("Invalid url value: {0}", url));

            using (var httpClientHandler = new HttpClientHandler { UseProxy = false, CookieContainer = Container })
            {
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(
                        new JavaScriptSerializer { MaxJsonLength = int.MaxValue, RecursionLimit = 100 }.Serialize(content),
                        Encoding.UTF8,
                        "application/json")
                };

                using (var httpClient = new HttpClient(httpClientHandler))
                {
                    var response = await httpClient.SendAsync(httpRequestMessage, cancellationToken);
                    return response.EnsureSuccessStatusCode();
                }
            }
        }

        public virtual async Task<string> RequestAsync(
            HttpMethod httpMethod, string url, Dictionary<string, string> content, bool skipStatusCheck = false,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (httpMethod == null)
                throw new ArgumentNullException("httpMethod");
            if (url == null)
                throw new ArgumentNullException("url");
            if (content == null)
                throw new ArgumentNullException("content");

            if (string.IsNullOrWhiteSpace(url))
                throw new Exception(string.Format("Invalid url value: {0}", url));

            using (var httpClientHandler = new HttpClientHandler { UseProxy = false, CookieContainer = Container })
            {
                var httpRequestMessage = new HttpRequestMessage(httpMethod, url)
                {
                    Content = new FormUrlEncodedContent(content)
                };

                using (var httpClient = new HttpClient(httpClientHandler))
                {
                    var response = await httpClient.SendAsync(httpRequestMessage, cancellationToken);
                    if (skipStatusCheck)
                        return await response.Content.ReadAsStringAsync();

                    response.EnsureSuccessStatusCode();
                    if (!response.IsSuccessStatusCode)
                        throw new Exception(string.Format("Error fetching data from Url: {0}", url));

                    return await response.Content.ReadAsStringAsync();
                }
            }
        }

        public virtual async Task<string> RequestJsonAsync(
            HttpMethod httpMethod, string url, Dictionary<string, string> content, bool skipStatusCheck = false,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (httpMethod == null)
                throw new ArgumentNullException("httpMethod");
            if (url == null)
                throw new ArgumentNullException("url");
            if (content == null)
                throw new ArgumentNullException("content");

            if (string.IsNullOrWhiteSpace(url))
                throw new Exception(string.Format("Invalid url value: {0}", url));

            using (var httpClientHandler = new HttpClientHandler { UseProxy = false, CookieContainer = Container })
            {
                var httpRequestMessage = new HttpRequestMessage(httpMethod, url)
                {
                    Content = new StringContent(
                        new JavaScriptSerializer { MaxJsonLength = int.MaxValue, RecursionLimit = 100 }.Serialize(content),
                        Encoding.UTF8,
                        "application/json")
                };

                using (var httpClient = new HttpClient(httpClientHandler))
                {
                    var response = await httpClient.SendAsync(httpRequestMessage, cancellationToken);
                    if (skipStatusCheck)
                        return await response.Content.ReadAsStringAsync();

                    response.EnsureSuccessStatusCode();
                    if (!response.IsSuccessStatusCode)
                        throw new Exception(string.Format("Error fetching data from Url: {0}", url));

                    return await response.Content.ReadAsStringAsync();
                }
            }
        }

        public virtual async Task<byte[]> RequestBytesAsync(
            HttpMethod httpMethod, string url, Dictionary<string, string> content, bool skipStatusCheck = false,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (httpMethod == null)
                throw new ArgumentNullException("httpMethod");
            if (url == null)
                throw new ArgumentNullException("url");
            if (content == null)
                throw new ArgumentNullException("content");

            if (string.IsNullOrWhiteSpace(url))
                throw new Exception(string.Format("Invalid url value: {0}", url));

            using (var httpClientHandler = new HttpClientHandler { UseProxy = false, CookieContainer = Container })
            {
                var httpRequestMessage = new HttpRequestMessage(httpMethod, url)
                {
                    Content = new FormUrlEncodedContent(content)
                };

                using (var httpClient = new HttpClient(httpClientHandler))
                {
                    var response = await httpClient.SendAsync(httpRequestMessage, cancellationToken);
                    if (skipStatusCheck)
                        return await response.Content.ReadAsByteArrayAsync();

                    response.EnsureSuccessStatusCode();
                    if (!response.IsSuccessStatusCode)
                        throw new Exception(string.Format("Error fetching data from Url: {0}", url));

                    return await response.Content.ReadAsByteArrayAsync();
                }
            }
        }

        public virtual async Task<byte[]> RequestBytesJsonAsync(
            HttpMethod httpMethod, string url, Dictionary<string, string> content, bool skipStatusCheck = false,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (httpMethod == null)
                throw new ArgumentNullException("httpMethod");
            if (url == null)
                throw new ArgumentNullException("url");
            if (content == null)
                throw new ArgumentNullException("content");

            if (string.IsNullOrWhiteSpace(url))
                throw new Exception(string.Format("Invalid url value: {0}", url));

            using (var httpClientHandler = new HttpClientHandler { UseProxy = false, CookieContainer = Container })
            {
                var httpRequestMessage = new HttpRequestMessage(httpMethod, url)
                {
                    Content = new StringContent(
                        new JavaScriptSerializer { MaxJsonLength = int.MaxValue, RecursionLimit = 100 }.Serialize(content),
                        Encoding.UTF8,
                        "application/json")
                };

                using (var httpClient = new HttpClient(httpClientHandler))
                {
                    var response = await httpClient.SendAsync(httpRequestMessage, cancellationToken);
                    if (skipStatusCheck)
                        return await response.Content.ReadAsByteArrayAsync();

                    response.EnsureSuccessStatusCode();
                    if (!response.IsSuccessStatusCode)
                        throw new Exception(string.Format("Error fetching data from Url: {0}", url));

                    return await response.Content.ReadAsByteArrayAsync();
                }
            }
        }

        public virtual async Task RequestStreamAsync(
            HttpMethod httpMethod, string url, Dictionary<string, string> content, Stream stream, bool skipStatusCheck = false,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (httpMethod == null)
                throw new ArgumentNullException("httpMethod");
            if (url == null)
                throw new ArgumentNullException("url");
            if (content == null)
                throw new ArgumentNullException("content");
            if (stream == null)
                throw new ArgumentNullException("stream");

            if (string.IsNullOrWhiteSpace(url))
                throw new Exception(string.Format("Invalid url value: {0}", url));

            using (var httpClientHandler = new HttpClientHandler { UseProxy = false, CookieContainer = Container })
            {
                var httpRequestMessage = new HttpRequestMessage(httpMethod, url)
                {
                    Content = new FormUrlEncodedContent(content)
                };

                using (var httpClient = new HttpClient(httpClientHandler))
                {
                    var response = await httpClient.SendAsync(httpRequestMessage, cancellationToken);
                    if (skipStatusCheck)
                    {
                        await response.Content.CopyToAsync(stream);
                        return;
                    }

                    response.EnsureSuccessStatusCode();
                    if (!response.IsSuccessStatusCode)
                        throw new Exception(string.Format("Error fetching data from Url: {0}", url));

                    await response.Content.CopyToAsync(stream);
                }
            }
        }

        public virtual async Task RequestStreamJsonAsync(
            HttpMethod httpMethod, string url, Dictionary<string, string> content, Stream stream, bool skipStatusCheck = false,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (httpMethod == null)
                throw new ArgumentNullException("httpMethod");
            if (url == null)
                throw new ArgumentNullException("url");
            if (content == null)
                throw new ArgumentNullException("content");
            if (stream == null)
                throw new ArgumentNullException("stream");

            if (string.IsNullOrWhiteSpace(url))
                throw new Exception(string.Format("Invalid url value: {0}", url));

            using (var httpClientHandler = new HttpClientHandler { UseProxy = false, CookieContainer = Container })
            {
                var httpRequestMessage = new HttpRequestMessage(httpMethod, url)
                {
                    Content = new StringContent(
                        new JavaScriptSerializer { MaxJsonLength = int.MaxValue, RecursionLimit = 100 }.Serialize(content),
                        Encoding.UTF8,
                        "application/json")
                };

                using (var httpClient = new HttpClient(httpClientHandler))
                {
                    var response = await httpClient.SendAsync(httpRequestMessage, cancellationToken);
                    if (skipStatusCheck)
                    {
                        await response.Content.CopyToAsync(stream);
                        return;
                    }

                    response.EnsureSuccessStatusCode();
                    if (!response.IsSuccessStatusCode)
                        throw new Exception(string.Format("Error fetching data from Url: {0}", url));

                    await response.Content.CopyToAsync(stream);
                }
            }
        }
    }
}
