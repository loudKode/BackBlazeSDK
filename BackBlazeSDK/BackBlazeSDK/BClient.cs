using BackBlazeSDK.JSON;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using static BackBlazeSDK.Basic;
using static BackBlazeSDK.Utilitiez;


// Create Account & Get 10 GB Free
// https://www.backblaze.com/b2/sign-up.html

namespace BackBlazeSDK
{
    public class BClient : IClient
    {
        public BClient(string API_Url, string Authorization_Token, ConnectionSettings Settings = null)
        {
            APIbase = string.Format("{0}/b2api/v2/", API_Url);
            AuthorizationToken = Authorization_Token;
            ConnectionSetting = Settings;

            if (Settings == null)
            {
                m_proxy = null;
            }
            else
            {
                m_proxy = Settings.Proxy;
                m_CloseConnection = Settings.CloseConnection ?? true;
                m_TimeOut = Settings.TimeOut ?? TimeSpan.FromMinutes(60);
            }
            ServicePointManager.Expect100Continue = true; ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;
        }



        #region ListBuckets
        public async Task<JSON_ListBuckets> ListBuckets(string AccountID, BucketTypesEnum BucketType = BucketTypesEnum.all)
        {
            var parameters = new Dictionary<object, object>() { { "accountId", AccountID }, { "bucketTypes", new List<string>() { { BucketType.ToString() } } } };
            using (HtpClient localHttpClient = new HtpClient(new HCHandler()))
            {
                var HtpReqMessage = new HtpRequestMessage(HttpMethod.Post, new pUri("b2_list_buckets")) { Content = SerializeDictionary(parameters) };
                using (HttpResponseMessage response = await localHttpClient.SendAsync(HtpReqMessage, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false))
                {
                    string result = await response.Content.ReadAsStringAsync();
                    return response.StatusCode == HttpStatusCode.OK ? JsonConvert.DeserializeObject<JSON_ListBuckets>(result, JSONhandler) : throw ShowError(result, (int)response.StatusCode);
                }
            }
        }
        #endregion

        #region List
        public async Task<JSON_List> List(string DestinationBucketID, string StartWith = null, string Contains = null, string DelimiterMark = null, int Limit = 999)
        {
            var parameters = new Dictionary<object, object>();
            parameters.Add("bucketId", DestinationBucketID);
            parameters.Add("startFileName", StartWith);
            parameters.Add("prefix", Contains);
            if (DelimiterMark != null) { parameters.Add("delimiter", DelimiterMark); }
            parameters.Add("maxFileCount", Limit);

            using (HtpClient localHttpClient = new HtpClient(new HCHandler()))
            {
                var HtpReqMessage = new HtpRequestMessage(HttpMethod.Post, new pUri("b2_list_file_names")) { Content = SerializeDictionary(parameters) };
                using (HttpResponseMessage response = await localHttpClient.SendAsync(HtpReqMessage, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false))
                {
                    string result = await response.Content.ReadAsStringAsync();
                    return response.StatusCode == HttpStatusCode.OK ? JsonConvert.DeserializeObject<JSON_List>(result, JSONhandler) : throw ShowError(result, (int)response.StatusCode);
                }
            }
        }
        #endregion

        #region UploadFile
        public async Task<JSON_GetUploadUrl> GETUploadUrl(string DestinationBucketID)
        {
            Dictionary<object, object> parameters = new Dictionary<object, object>() { { "bucketId", DestinationBucketID } };
            using (HtpClient localHttpClient = new HtpClient(new HCHandler()))
            {
                var HtpReqMessage = new HtpRequestMessage(HttpMethod.Post, new pUri("b2_get_upload_url")) { Content = SerializeDictionary(parameters) };
                using (HttpResponseMessage response = await localHttpClient.SendAsync(HtpReqMessage, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false))
                {
                    string result = await response.Content.ReadAsStringAsync();
                    return response.StatusCode == HttpStatusCode.OK ? JsonConvert.DeserializeObject<JSON_GetUploadUrl>(result, JSONhandler) : throw ShowError(result, (int)response.StatusCode);
                }
            }
        }

        public async Task<JSON_FileMetadata> Upload(object FileToUpload, UploadTypes UploadType, string DestinationBucketID, string FileName, string SHA1, IProgress<ReportStatus> ReportCls = null, CancellationToken token = default)
        {
            ReportCls = ReportCls ?? new Progress<ReportStatus>();
            var uploadUrl = await GETUploadUrl(DestinationBucketID);
            ReportCls.Report(new ReportStatus() { Finished = false, TextStatus = "Initializing..." });
            try
            {
                System.Net.Http.Handlers.ProgressMessageHandler progressHandler = new System.Net.Http.Handlers.ProgressMessageHandler(new HCHandler());
                progressHandler.HttpSendProgress += (sender, e) => { ReportCls.Report(new ReportStatus() { ProgressPercentage = e.ProgressPercentage, BytesTransferred = e.BytesTransferred, TotalBytes = e.TotalBytes ?? 0, TextStatus = "Uploading..." }); };
                using (HtpClient localHttpClient = new HtpClient(progressHandler))
                {
                    var HtpReqMessage = new HtpRequestMessage(HttpMethod.Post, new Uri(uploadUrl.uploadUrl));
                    HttpContent streamContent = null; ;
                    switch (UploadType)
                    {
                        case UploadTypes.FilePath:
                            streamContent = new StreamContent(new System.IO.FileStream(FileToUpload.ToString(), System.IO.FileMode.Open, System.IO.FileAccess.Read));
                            break;
                        case UploadTypes.Stream:
                            streamContent = new StreamContent((System.IO.Stream)FileToUpload);
                            break;
                        case UploadTypes.BytesArry:
                            streamContent = new StreamContent(new System.IO.MemoryStream((byte[])FileToUpload));
                            break;
                    }
                    streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                    // #If NET452 Then
                    // streamContent.Headers.ContentType = New Net.Http.Headers.MediaTypeHeaderValue(system.Web.MimeMapping.GetMimeMapping(FileName))
                    // #End If
                    HtpReqMessage.Content = streamContent;
                    HtpReqMessage.Headers.TryAddWithoutValidation("Authorization", uploadUrl.authorizationToken);
                    HtpReqMessage.Headers.TryAddWithoutValidation("X-Bz-File-Name", WebUtility.UrlEncode(FileName));
                    HtpReqMessage.Headers.TryAddWithoutValidation("Content-Length", "2000");
                    HtpReqMessage.Headers.TryAddWithoutValidation("X-Bz-Content-Sha1", SHA1);

                    // ''''''''''''''''will write the whole content to H.D WHEN download completed'''''''''''''''''''''''''''''
                    using (HttpResponseMessage ResPonse = await localHttpClient.SendAsync(HtpReqMessage, HttpCompletionOption.ResponseContentRead, token).ConfigureAwait(false))
                    {
                        string result = await ResPonse.Content.ReadAsStringAsync();

                        token.ThrowIfCancellationRequested();
                        if (ResPonse.StatusCode == HttpStatusCode.OK)
                        {
                            var userInfo = JsonConvert.DeserializeObject<JSON_FileMetadata>(result);
                            ReportCls.Report(new ReportStatus() { Finished = true, TextStatus = $"[{FileName}] Uploaded successfully" });
                            return userInfo;
                        }
                        else
                        {
                            var errorInfo = JsonConvert.DeserializeObject<JSON_Error>(result);
                            ReportCls.Report(new ReportStatus() { Finished = true, TextStatus = $"The request returned with HTTP status code {errorInfo._ErrorMessage ?? errorInfo.code}" });
                            return null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ReportCls.Report(new ReportStatus() { Finished = true });
                if (ex.Message.ToString().ToLower().Contains("a task was canceled"))
                {
                    ReportCls.Report(new ReportStatus() { TextStatus = ex.Message });
                }
                else
                {
                    throw new BackBlazeException(ex.Message, 1001);
                }
                return null;
            }
        }

        #endregion

        #region FileMetadata
        public async Task<JSON_FileMetadata> FileMetadata(string DestinationFileID)
        {
            var parameters = new Dictionary<object, object>() { { "fileId", DestinationFileID } };
            using (HtpClient localHttpClient = new HtpClient(new HCHandler()))
            {
                var HtpReqMessage = new HtpRequestMessage(HttpMethod.Post, new pUri("b2_get_file_info")) { Content = SerializeDictionary(parameters) };
                using (HttpResponseMessage response = await localHttpClient.SendAsync(HtpReqMessage, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false))
                {
                    string result = await response.Content.ReadAsStringAsync();
                    return response.StatusCode == HttpStatusCode.OK ? JsonConvert.DeserializeObject<JSON_FileMetadata>(result, JSONhandler) : throw ShowError(result, (int)response.StatusCode);
                }
            }
        }
        #endregion

        #region DeleteBucket
        public async Task<JSON_BucketMetadata> DeleteBucket(string AccountID, string DestinationBucketID)
        {
            var parameters = new Dictionary<object, object>() { { "accountId", AccountID }, { "bucketId", DestinationBucketID } };
            using (HtpClient localHttpClient = new HtpClient(new HCHandler()))
            {
                var HtpReqMessage = new HtpRequestMessage(HttpMethod.Post, new pUri("b2_delete_bucket")) { Content = SerializeDictionary(parameters) };
                using (HttpResponseMessage response = await localHttpClient.SendAsync(HtpReqMessage, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false))
                {
                    string result = await response.Content.ReadAsStringAsync();
                    return response.StatusCode == HttpStatusCode.OK ? JsonConvert.DeserializeObject<JSON_BucketMetadata>(result, JSONhandler) : throw ShowError(result, (int)response.StatusCode);
                }
            }
        }
        #endregion

        #region CreateBucket
        public async Task<JSON_BucketMetadata> CreateBucket(string AccountID, string BucketName, BucketTypesEnum BucketType)
        {
            Dictionary<object, object> parameters = new Dictionary<object, object>() { { "accountId", AccountID }, { "bucketName", BucketName.Replace(" ", "-") }, { "bucketType", BucketType.ToString() } };
            using (HtpClient localHttpClient = new HtpClient(new HCHandler()))
            {
                var HtpReqMessage = new HtpRequestMessage(HttpMethod.Post, new pUri("b2_create_bucket")) { Content = SerializeDictionary(parameters) };
                using (HttpResponseMessage response = await localHttpClient.SendAsync(HtpReqMessage, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false))
                {
                    string result = await response.Content.ReadAsStringAsync();
                    return response.StatusCode == HttpStatusCode.OK ? JsonConvert.DeserializeObject<JSON_BucketMetadata>(result, JSONhandler) : throw ShowError(result, (int)response.StatusCode);
                }
            }
        }
        #endregion

        #region CopyFile
        public async Task<JSON_FileMetadata> CopyFile(string SorceFileID, string DestinationBucketID, string RenameTo)
        {
            var parameters = new Dictionary<object, object>
            {
                { "destinationBucketId", DestinationBucketID },
                { "fileName", WebUtility.UrlEncode(RenameTo) },
                { "sourceFileId", SorceFileID },
                { "metadataDirective", "COPY" }
            };

            using (HtpClient localHttpClient = new HtpClient(new HCHandler()))
            {
                var HtpReqMessage = new HtpRequestMessage(HttpMethod.Post, new pUri("b2_copy_file")) { Content = SerializeDictionary(parameters) };
                using (HttpResponseMessage response = await localHttpClient.SendAsync(HtpReqMessage, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false))
                {
                    string result = await response.Content.ReadAsStringAsync();
                    return response.StatusCode == HttpStatusCode.OK ? JsonConvert.DeserializeObject<JSON_FileMetadata>(result, JSONhandler) : throw ShowError(result, (int)response.StatusCode);
                }
            }
        }
        #endregion

        #region DeleteFile
        public async Task<bool> DeleteFile(string DestinationFileID, string DestinationFileName)
        {
            var parameters = new Dictionary<object, object>() { { "fileId", DestinationFileID }, { "fileName", DestinationFileName } };
            using (HtpClient localHttpClient = new HtpClient(new HCHandler()))
            {
                var HtpReqMessage = new HtpRequestMessage(HttpMethod.Post, new Uri(APIbase + "b2_delete_file_version")) { Content = SerializeDictionary(parameters) };
                using (HttpResponseMessage response = await localHttpClient.SendAsync(HtpReqMessage, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false))
                {
                    string result = await response.Content.ReadAsStringAsync();
                    return response.StatusCode == HttpStatusCode.OK ? true : throw ShowError(result, (int)response.StatusCode);
                }
            }
        }
        #endregion

        #region UpdateBucket
        public async Task<JSON_BucketMetadata> UpdateBucket(string AccountID, string DestinationBucketID, BucketTypesEnum BucketType)
        {
            var parameters = new Dictionary<object, object>() { { "accountId", AccountID }, { "bucketId", DestinationBucketID }, { "bucketType", BucketType.ToString() } };
            using (HtpClient localHttpClient = new HtpClient(new HCHandler()))
            {
                var HtpReqMessage = new HtpRequestMessage(HttpMethod.Post, new Uri(APIbase + "b2_update_bucket")) { Content = SerializeDictionary(parameters) };
                using (HttpResponseMessage response = await localHttpClient.SendAsync(HtpReqMessage, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false))
                {
                    string result = await response.Content.ReadAsStringAsync();
                    return response.StatusCode == HttpStatusCode.OK ? JsonConvert.DeserializeObject<JSON_BucketMetadata>(result, JSONhandler) : throw ShowError(result, (int)response.StatusCode);
                }
            }
        }
        #endregion

        #region PublicBucket_DownloadFile
        public async Task PublicBucket_DownloadFile(string DestinationFileID, string FileSaveDir, string FileName, IProgress<ReportStatus> ReportCls = null, CancellationToken token = default)
        {
            ReportCls = ReportCls ?? new Progress<ReportStatus>();
            ReportCls.Report(new ReportStatus() { Finished = false, TextStatus = "Initializing..." });
            try
            {
                System.Net.Http.Handlers.ProgressMessageHandler progressHandler = new System.Net.Http.Handlers.ProgressMessageHandler(new HCHandler());
                progressHandler.HttpReceiveProgress += (sender, e) => { ReportCls.Report(new ReportStatus() { ProgressPercentage = e.ProgressPercentage, BytesTransferred = e.BytesTransferred, TotalBytes = e.TotalBytes ?? 0, TextStatus = "Downloading..." }); };
                using (HtpClient localHttpClient = new HtpClient(progressHandler))
                {
                    var HtpReqMessage = new HttpRequestMessage(HttpMethod.Get, new Uri(APIbase + $"b2_download_file_by_id?fileId={DestinationFileID}"));
                    // ''''''''''''''''will write the whole content to H.D WHEN download completed'''''''''''''''''''''''''''''
                    using (HttpResponseMessage ResPonse = await localHttpClient.GetAsync(HtpReqMessage.RequestUri, HttpCompletionOption.ResponseContentRead, token).ConfigureAwait(false))
                    {
                        token.ThrowIfCancellationRequested();
                        if (ResPonse.StatusCode == HttpStatusCode.OK)
                        {
                            ReportCls.Report(new ReportStatus() { Finished = true, TextStatus = $"[{FileName}] Downloaded successfully." });
                        }
                        else
                        {
                            string result = await ResPonse.Content.ReadAsStringAsync();
                            var errorInfo = JsonConvert.DeserializeObject<JSON_Error>(result);
                            ReportCls.Report(new ReportStatus() { Finished = true, TextStatus = $"Error code: {errorInfo._ErrorMessage ?? errorInfo.code}" });
                        }

                        ResPonse.EnsureSuccessStatusCode();
                        var stream_ = await ResPonse.Content.ReadAsStreamAsync();
                        string FPathname = System.IO.Path.Combine(FileSaveDir, FileName);
                        using (var fileStream = new System.IO.FileStream(FPathname, System.IO.FileMode.Append, System.IO.FileAccess.Write))
                        {
                            stream_.CopyTo(fileStream);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ReportCls.Report(new ReportStatus() { Finished = true });
                if (ex.Message.ToString().ToLower().Contains("a task was canceled"))
                {
                    ReportCls.Report(new ReportStatus() { TextStatus = ex.Message });
                }
                else
                {
                    throw new BackBlazeException(ex.Message, 1001);
                }
            }
        }
        #endregion

        #region PublicBucket_DownloadLargeFile
        public async Task PublicBucket_DownloadLargeFile(string DestinationFileID, string FileSaveDir, string FileName, IProgress<ReportStatus> ReportCls = null, CancellationToken token = default)
        {
            ReportCls = ReportCls ?? new Progress<ReportStatus>();
            ReportCls.Report(new ReportStatus() { Finished = false, TextStatus = "Initializing..." });
            try
            {
                System.Net.Http.Handlers.ProgressMessageHandler progressHandler = new System.Net.Http.Handlers.ProgressMessageHandler(new HCHandler());
                progressHandler.HttpReceiveProgress += (sender, e) => { ReportCls.Report(new ReportStatus() { ProgressPercentage = e.ProgressPercentage, BytesTransferred = e.BytesTransferred, TotalBytes = e.TotalBytes ?? 0, TextStatus = "Downloading..." }); };
                HtpClient localHttpClient = new HtpClient(progressHandler);
                var HtpReqMessage = new HttpRequestMessage(HttpMethod.Get, new Uri(APIbase + $"b2_download_file_by_id?fileId={DestinationFileID}"));
                // ''''''''''''''''will write the whole content to H.D WHEN download completed'''''''''''''''''''''''''''''
                using (HttpResponseMessage ResPonse = await localHttpClient.GetAsync(HtpReqMessage.RequestUri, HttpCompletionOption.ResponseHeadersRead, token).ConfigureAwait(false))
                {
                    token.ThrowIfCancellationRequested();
                    if (ResPonse.StatusCode == HttpStatusCode.OK)
                    {
                        ResPonse.EnsureSuccessStatusCode();
                        // ''''''''''''''' write byte by byte to H.D '''''''''''''''''''''''''''''
                        string FPathname = System.IO.Path.Combine(FileSaveDir, FileName);
                        using (System.IO.Stream streamToReadFrom = await ResPonse.Content.ReadAsStreamAsync())
                        {
                            using (System.IO.Stream streamToWriteTo = System.IO.File.Open(FPathname, System.IO.FileMode.Create))
                            {
                                await streamToReadFrom.CopyToAsync(streamToWriteTo, 1024, token);
                            }
                        }
                        ReportCls.Report(new ReportStatus() { Finished = true, TextStatus = (string.Format("[{0}] Downloaded successfully.", FileName)) });
                    }
                    else
                    {
                        string result = await ResPonse.Content.ReadAsStringAsync();
                        var errorInfo = JsonConvert.DeserializeObject<JSON_Error>(result);
                        ReportCls.Report(new ReportStatus() { Finished = true, TextStatus = ((string.Format("Error code: {0}", string.IsNullOrEmpty(errorInfo._ErrorMessage) ? errorInfo.code : errorInfo._ErrorMessage))) });
                    }
                }
            }
            catch (Exception ex)
            {
                ReportCls.Report(new ReportStatus() { Finished = true });
                if (ex.Message.ToString().ToLower().Contains("a task was canceled"))
                {
                    ReportCls.Report(new ReportStatus() { TextStatus = ex.Message });
                }
                else
                {
                    throw new BackBlazeException(ex.Message, 1001);
                }
            }
        }
        #endregion

        #region PublicBucket_DownloadFileAsStream
        public async Task<System.IO.Stream> PublicBucket_DownloadFileAsStream(string DestinationFileID, IProgress<ReportStatus> ReportCls = null, CancellationToken token = default)
        {
            ReportCls = ReportCls ?? new Progress<ReportStatus>();
            ReportCls.Report(new ReportStatus() { Finished = false, TextStatus = "Initializing..." });
            try
            {
                System.Net.Http.Handlers.ProgressMessageHandler progressHandler = new System.Net.Http.Handlers.ProgressMessageHandler(new HCHandler());
                progressHandler.HttpReceiveProgress += (sender, e) => { ReportCls.Report(new ReportStatus() { ProgressPercentage = e.ProgressPercentage, BytesTransferred = e.BytesTransferred, TotalBytes = e.TotalBytes ?? 0, TextStatus = "Downloading..." }); };
                HtpClient localHttpClient = new HtpClient(progressHandler);
                var HtpReqMessage = new HttpRequestMessage(HttpMethod.Get, new Uri(APIbase + $"b2_download_file_by_id?fileId={DestinationFileID}"));
                // ''''''''''''''''will write the whole content to H.D WHEN download completed'''''''''''''''''''''''''''''
                HttpResponseMessage ResPonse = await localHttpClient.SendAsync(HtpReqMessage, HttpCompletionOption.ResponseContentRead, token).ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                if (ResPonse.StatusCode == HttpStatusCode.OK)
                {
                    ReportCls.Report(new ReportStatus() { Finished = true, TextStatus = ("File Downloaded successfully.") });
                }
                else
                {
                    string result = await ResPonse.Content.ReadAsStringAsync();
                    var errorInfo = JsonConvert.DeserializeObject<JSON_Error>(result, JSONhandler);
                    ReportCls.Report(new ReportStatus() { Finished = true, TextStatus = ((string.Format("Error code: {0}", string.IsNullOrEmpty(errorInfo._ErrorMessage) ? errorInfo.code : errorInfo._ErrorMessage))) });
                }

                ResPonse.EnsureSuccessStatusCode();
                System.IO.Stream stream_ = await ResPonse.Content.ReadAsStreamAsync();
                return stream_;
            }
            catch (Exception ex)
            {
                ReportCls.Report(new ReportStatus() { Finished = true });
                if (ex.Message.ToString().ToLower().Contains("a task was canceled"))
                {
                    ReportCls.Report(new ReportStatus() { TextStatus = ex.Message });
                }
                else
                {
                    throw new BackBlazeException(ex.Message, 1001);
                }
                return null;
            }
        }
        #endregion

        #region PrivateBucket_DownloadFile
        public async Task PrivateBucket_DownloadFile(string DestinationFileID, string FileSaveDir, string FileName, IProgress<ReportStatus> ReportCls = null, CancellationToken token = default)
        {
            ReportCls = ReportCls ?? new Progress<ReportStatus>();
            ReportCls.Report(new ReportStatus() { Finished = false, TextStatus = "Initializing..." });
            try
            {
                System.Net.Http.Handlers.ProgressMessageHandler progressHandler = new System.Net.Http.Handlers.ProgressMessageHandler(new HCHandler());
                progressHandler.HttpReceiveProgress += (sender, e) => { ReportCls.Report(new ReportStatus() { ProgressPercentage = e.ProgressPercentage, BytesTransferred = e.BytesTransferred, TotalBytes = e.TotalBytes ?? 0, TextStatus = "Downloading..." }); };
                HtpClient localHttpClient = new HtpClient(progressHandler);
                var HtpReqMessage = new HtpRequestMessage(HttpMethod.Get, new Uri(APIbase + $"b2_download_file_by_id?fileId={DestinationFileID}"));
                // ''''''''''''''''will write the whole content to H.D WHEN download completed'''''''''''''''''''''''''''''
                using (HttpResponseMessage ResPonse = await localHttpClient.GetAsync(HtpReqMessage.RequestUri, HttpCompletionOption.ResponseContentRead, token).ConfigureAwait(false))
                {
                    token.ThrowIfCancellationRequested();
                    if (ResPonse.StatusCode == HttpStatusCode.OK)
                    {
                        ReportCls.Report(new ReportStatus() { Finished = true, TextStatus = (string.Format("[{0}] Downloaded successfully.", FileName)) });
                    }
                    else
                    {
                        string result = await ResPonse.Content.ReadAsStringAsync();
                        var errorInfo = JsonConvert.DeserializeObject<JSON_Error>(result);
                        ReportCls.Report(new ReportStatus() { Finished = true, TextStatus = ((string.Format("Error code: {0}", string.IsNullOrEmpty(errorInfo._ErrorMessage) ? errorInfo.code : errorInfo._ErrorMessage))) });
                    }

                    ResPonse.EnsureSuccessStatusCode();
                    var stream_ = await ResPonse.Content.ReadAsStreamAsync();
                    string FPathname = System.IO.Path.Combine(FileSaveDir, FileName);
                    using (var fileStream = new System.IO.FileStream(FPathname, System.IO.FileMode.Append, System.IO.FileAccess.Write))
                    {
                        stream_.CopyTo(fileStream);
                    }
                }
            }
            catch (Exception ex)
            {
                ReportCls.Report(new ReportStatus() { Finished = true });
                if (ex.Message.ToString().ToLower().Contains("a task was canceled"))
                {
                    ReportCls.Report(new ReportStatus() { TextStatus = ex.Message });
                }
                else
                {
                    throw new BackBlazeException(ex.Message, 1001);
                }
            }
        }
        #endregion

        #region PrivateBucket_DownloadLargeFile
        public async Task PrivateBucket_DownloadLargeFile(string DestinationFileID, string FileSaveDir, string FileName, IProgress<ReportStatus> ReportCls = null, CancellationToken token = default)
        {
            ReportCls = ReportCls ?? new Progress<ReportStatus>();
            ReportCls.Report(new ReportStatus() { Finished = false, TextStatus = "Initializing..." });
            try
            {
                System.Net.Http.Handlers.ProgressMessageHandler progressHandler = new System.Net.Http.Handlers.ProgressMessageHandler(new HCHandler());
                progressHandler.HttpReceiveProgress += (sender, e) => { ReportCls.Report(new ReportStatus() { ProgressPercentage = e.ProgressPercentage, BytesTransferred = e.BytesTransferred, TotalBytes = e.TotalBytes ?? 0, TextStatus = "Downloading..." }); };
                HtpClient localHttpClient = new HtpClient(progressHandler);
                var HtpReqMessage = new HtpRequestMessage(HttpMethod.Get, new Uri(APIbase + $"b2_download_file_by_id?fileId={DestinationFileID}"));
                // ''''''''''''''''will write the whole content to H.D WHEN download completed'''''''''''''''''''''''''''''
                using (HttpResponseMessage ResPonse = await localHttpClient.GetAsync(HtpReqMessage.RequestUri, HttpCompletionOption.ResponseHeadersRead, token).ConfigureAwait(false))
                {
                    token.ThrowIfCancellationRequested();
                    if (ResPonse.StatusCode == HttpStatusCode.OK)
                    {
                        ResPonse.EnsureSuccessStatusCode();
                        // ''''''''''''''' write byte by byte to H.D '''''''''''''''''''''''''''''
                        string FPathname = System.IO.Path.Combine(FileSaveDir, FileName);
                        using (System.IO.Stream streamToReadFrom = await ResPonse.Content.ReadAsStreamAsync())
                        {
                            using (System.IO.Stream streamToWriteTo = System.IO.File.Open(FPathname, System.IO.FileMode.Create))
                            {
                                await streamToReadFrom.CopyToAsync(streamToWriteTo, 1024, token);
                            }
                        }
                        ReportCls.Report(new ReportStatus() { Finished = true, TextStatus = (string.Format("[{0}] Downloaded successfully.", FileName)) });
                    }
                    else
                    {
                        string result = await ResPonse.Content.ReadAsStringAsync();
                        var errorInfo = JsonConvert.DeserializeObject<JSON_Error>(result);
                        ReportCls.Report(new ReportStatus() { Finished = true, TextStatus = ((string.Format("Error code: {0}", string.IsNullOrEmpty(errorInfo._ErrorMessage) ? errorInfo.code : errorInfo._ErrorMessage))) });
                    }
                }
            }
            catch (Exception ex)
            {
                ReportCls.Report(new ReportStatus() { Finished = true });
                if (ex.Message.ToString().ToLower().Contains("a task was canceled"))
                {
                    ReportCls.Report(new ReportStatus() { TextStatus = ex.Message });
                }
                else
                {
                    throw new BackBlazeException(ex.Message, 1001);
                }
            }
        }
        #endregion

        #region PrivateBucket_DownloadFileAsStream
        public async Task<System.IO.Stream> PrivateBucket_DownloadFileAsStream(string DestinationFileID, IProgress<ReportStatus> ReportCls = null, CancellationToken token = default)
        {
            ReportCls = ReportCls ?? new Progress<ReportStatus>();
            ReportCls.Report(new ReportStatus() { Finished = false, TextStatus = "Initializing..." });
            try
            {
                System.Net.Http.Handlers.ProgressMessageHandler progressHandler = new System.Net.Http.Handlers.ProgressMessageHandler(new HCHandler());
                progressHandler.HttpReceiveProgress += (sender, e) => { ReportCls.Report(new ReportStatus() { ProgressPercentage = e.ProgressPercentage, BytesTransferred = e.BytesTransferred, TotalBytes = e.TotalBytes ?? 0, TextStatus = "Downloading..." }); };
                HtpClient localHttpClient = new HtpClient(progressHandler);
                var HtpReqMessage = new HttpRequestMessage(HttpMethod.Get, new Uri(APIbase + $"b2_download_file_by_id?fileId={DestinationFileID}"));
                // ''''''''''''''''will write the whole content to H.D WHEN download completed'''''''''''''''''''''''''''''
                HttpResponseMessage ResPonse = await localHttpClient.SendAsync(HtpReqMessage, HttpCompletionOption.ResponseContentRead, token).ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                if (ResPonse.StatusCode == HttpStatusCode.OK)
                {
                    ReportCls.Report(new ReportStatus() { Finished = true, TextStatus = "File Downloaded successfully." });
                }
                else
                {
                    string result = await ResPonse.Content.ReadAsStringAsync();
                    var errorInfo = JsonConvert.DeserializeObject<JSON_Error>(result);
                    ReportCls.Report(new ReportStatus() { Finished = true, TextStatus = ((string.Format("Error code: {0}", string.IsNullOrEmpty(errorInfo._ErrorMessage) ? errorInfo.code : errorInfo._ErrorMessage))) });
                }

                ResPonse.EnsureSuccessStatusCode();
                System.IO.Stream stream_ = await ResPonse.Content.ReadAsStreamAsync();
                return stream_;
            }
            catch (Exception ex)
            {
                ReportCls.Report(new ReportStatus() { Finished = true });
                if (ex.Message.ToString().ToLower().Contains("a task was canceled"))
                {
                    ReportCls.Report(new ReportStatus() { TextStatus = ex.Message });
                }
                else
                {
                    throw new BackBlazeException(ex.Message, 1001);
                }
                return null;
            }
        }
        #endregion

        #region PublicBucket_GetDirectUrl
        public string PublicBucket_GetDirectUrl(string downloadApiUrl, string BucketName, string FileName)
        {
            return string.Format("{0}/file/{1}/{2}", downloadApiUrl, BucketName, FileName);
        }
        #endregion


    }

}