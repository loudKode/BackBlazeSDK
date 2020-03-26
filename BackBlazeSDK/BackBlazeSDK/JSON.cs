
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using static BackBlazeSDK.Utilitiez;

namespace BackBlazeSDK.JSON
{

    #region JSON_AuthorizeAccount
    public class JSON_AuthorizeAccount
    {
        public Newtonsoft.Json.Linq.JToken JSON { get; set; }
        public int absoluteMinimumPartSize { get; set; }
        public string accountId { get; set; }
        public JSON_AuthorizeAccountAllowed allowed { get; set; }
        public string apiUrl { get; set; }
        public string authorizationToken { get; set; } // authorization token is valid for at most 24 hours
        [JsonProperty("downloadUrl", NullValueHandling = NullValueHandling.Ignore)] public string downloadApiUrl { get; set; }
        public int recommendedPartSize { get; set; }
    }
    public class JSON_AuthorizeAccountAllowed
    {
        public string bucketId { get; set; }
        public string bucketName { get; set; }
        public List<string> capabilities { get; set; }
        public object namePrefix { get; set; }
    }
    #endregion

    #region JSON_ListBuckets
    public class JSON_ListBuckets
    {
        public Newtonsoft.Json.Linq.JToken JSON
        {
            get
            {
                return Newtonsoft.Json.Linq.JToken.Parse(ToString());
            }
        }
        [JsonProperty("buckets")] public List<JSON_BucketMetadata> BucketsList { get; set; }
    }
    public class JSON_BucketMetadata
    {
        public string accountId { get; set; }
        public string bucketId { get; set; }
        // Public Property bucketInfo As JSON_ListBucketsBucketinfo
        public string bucketName { get; set; }
        public BucketTypesEnum bucketType { get; set; }
    }
    #endregion

    #region JSON_List
    public class JSON_List
    {
        [JsonProperty("files")] public List<JSON_FileMetadata> FilesList { get; set; }
        public object nextFileName { get; set; }
    }
    public class JSON_FileMetadata
    {
        public enum fileORfolder
        {
            file,
            folder
        }
        public fileORfolder File_Folder
        {
            get
            {
                return action == "folder" ? fileORfolder.folder : fileORfolder.file;
            }
        }
        public string accountId { get; set; }
        [Browsable(false)] [Bindable(false)] [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] [EditorBrowsable(EditorBrowsableState.Never)] public string action { get; set; }
        public string bucketId { get; set; }
        [JsonProperty("contentLength")] public long Size { get; set; }
        [JsonProperty("contentSha1")]
        public string SHA1 { get; set; }
        public string contentType { get; set; }
        public string fileId { get; set; }
        // Public Property fileInfo As Fileinfo
        [JsonProperty("fileName")] public string Path { get; set; }
        [JsonProperty("uploadTimestamp")] public long CreatedDate { get; set; }

        public string ParentPath
        {
            get
            {
                return System.IO.Path.GetDirectoryName(Path);
            }
        }
        public string Name
        {
            get
            {
                return System.IO.Path.GetFileName(Path);
            }
        }
        public string Ext
        {
            get
            {
                return System.IO.Path.GetExtension(Path);
            }
        }
    }
    #endregion

    #region JSON_GetUploadUrl
    public class JSON_GetUploadUrl
    {
        public string bucketId { get; set; }
        public string uploadUrl { get; set; }
        public string authorizationToken { get; set; }
    }
    #endregion

    #region JSON_Error
    public class JSON_Error
    {
        public string code { get; set; }
        [JsonProperty("message")] public string _ErrorMessage { get; set; }
        public int status { get; set; }
    }
    #endregion

}



