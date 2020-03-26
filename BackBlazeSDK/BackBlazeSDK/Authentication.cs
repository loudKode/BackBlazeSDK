using BackBlazeSDK.JSON;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static BackBlazeSDK.Basic;

namespace BackBlazeSDK
{
    public  class Authentication
    {

        /// <summary>
        /// https://secure.backblaze.com/app_keys.htm   ?bznetid=5282715381565789718559
        /// https://www.backblaze.com/b2/docs/b2_authorize_account.html
        /// </summary>
        /// <param name="Key_ID">ApplicationKeyID OR accountID</param>
        /// <param name="Application_Key">ApplicationKey</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static  async Task<JSON_AuthorizeAccount> GetToken_24Hrs(string Key_ID, string Application_Key)
        {
            ServicePointManager.Expect100Continue = true; ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;

            using (HtpClient localHttpClient = new HtpClient(new HCHandler()))
            {
                localHttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.Default.GetBytes($"{Key_ID}:{Application_Key}")));

                var HtpReqMessage = new HttpRequestMessage(HttpMethod.Get, new Uri("https://api.backblazeb2.com/b2api/v2/b2_authorize_account"));
                using (HttpResponseMessage response = await localHttpClient.SendAsync(HtpReqMessage, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false))
                {
                    string result = await response.Content.ReadAsStringAsync();

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var userInfo = JsonConvert.DeserializeObject<JSON_AuthorizeAccount>(result, JSONhandler);
                        userInfo.JSON = Newtonsoft.Json.Linq.JToken.Parse(result);
                        return userInfo;
                    }
                    else
                    {
                        throw new BackBlazeException(response.ReasonPhrase, (int)response.StatusCode);
                    }
                }
            }
        }


    }
}
