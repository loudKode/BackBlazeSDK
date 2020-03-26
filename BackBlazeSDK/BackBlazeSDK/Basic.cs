using BackBlazeSDK.JSON;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BackBlazeSDK
{
    public static class Basic
    {


        //''api doc
        //'https://www.backblaze.com/b2/docs/

        public static string APIbase = "https://apidocs.zoho.com/files/v1/";
        public static JsonSerializerSettings JSONhandler = new JsonSerializerSettings() { MissingMemberHandling = MissingMemberHandling.Ignore, NullValueHandling = NullValueHandling.Ignore };
        public static string AuthorizationToken = null;
        public static TimeSpan m_TimeOut = System.Threading.Timeout.InfiniteTimeSpan; //' TimeSpan.FromMinutes(60)
        public static bool m_CloseConnection = true;
        public static ConnectionSettings ConnectionSetting = null;

        private static ProxyConfig _proxy;
        public static ProxyConfig m_proxy
        {
            get
            {
                return _proxy ?? new ProxyConfig();
            }
            set
            {
                _proxy = value;
            }
        }

        public class HCHandler : System.Net.Http.HttpClientHandler
        {
            public HCHandler() : base()
            {
                if (m_proxy.SetProxy)
                {
                    base.MaxRequestContentBufferSize = 1 * 1024 * 1024;
                    base.Proxy = new System.Net.WebProxy($"http://{m_proxy.ProxyIP}:{m_proxy.ProxyPort}", true, null, new System.Net.NetworkCredential(m_proxy.ProxyUsername, m_proxy.ProxyPassword));
                    base.UseProxy = m_proxy.SetProxy;
                }
            }
        }

        public class HtpRequestMessage : System.Net.Http.HttpRequestMessage
        {
            public HtpRequestMessage(System.Net.Http.HttpMethod Method, Uri RequestUri) : base(Method, RequestUri)
            {
                //base.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(AuthorizationToken);
                base.Headers.TryAddWithoutValidation("Authorization", AuthorizationToken);
            }
        }

        public class HtpClient : System.Net.Http.HttpClient
        {
            public HtpClient(HCHandler HCHandler) : base(HCHandler)
            {
                base.DefaultRequestHeaders.UserAgent.ParseAdd("BackBlazeSDK");
                base.DefaultRequestHeaders.ConnectionClose = m_CloseConnection;
                base.Timeout = m_TimeOut;
            }
            public HtpClient(System.Net.Http.Handlers.ProgressMessageHandler progressHandler) : base(progressHandler)
            {
                base.DefaultRequestHeaders.UserAgent.ParseAdd("BackBlazeSDK");
                base.DefaultRequestHeaders.ConnectionClose = m_CloseConnection;
                base.Timeout = m_TimeOut;
            }
        }

        public static System.Net.Http.HttpContent SerializeDictionary(Dictionary<object, object> Dict)
        {
            System.Net.Http.HttpContent streamContent = new System.Net.Http.StringContent(JsonConvert.SerializeObject(Dict, Formatting.Indented), System.Text.Encoding.UTF8, "application/json");
            return streamContent;
        }

        public class pUri : Uri
        {
            public pUri(string ApiAction, Dictionary<string, string> Parameters) : base(APIbase + ApiAction + Utilitiez.AsQueryString(Parameters)) { }
            public pUri(string ApiAction) : base(APIbase + ApiAction) { }
        }

        //public static void ShowError(string result, int StatusCode)
        //{
        //    var errorInfo = JsonConvert.DeserializeObject<JSON_Error>(result, JSONhandler);
        //    throw new BackBlazeException(string.IsNullOrEmpty(errorInfo._ErrorMessage) ? errorInfo.code : errorInfo._ErrorMessage, StatusCode);
        //}

        public static BackBlazeException ShowError(string result, int StatusCode)
        {
            var errorInfo = JsonConvert.DeserializeObject<JSON_Error>(result, JSONhandler);
            return new BackBlazeException(string.IsNullOrEmpty(errorInfo._ErrorMessage) ? errorInfo.code : errorInfo._ErrorMessage, StatusCode);
        }
    }
}
