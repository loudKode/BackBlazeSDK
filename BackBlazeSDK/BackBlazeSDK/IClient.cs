
using BackBlazeSDK;
using BackBlazeSDK.JSON;
using System;
using System.Threading.Tasks;

namespace BackBlazeSDK
{
    public interface IClient
    {
        /// <summary>
        /// Lists buckets associated with an account, in alphabetical order by bucket name.
        /// https://www.backblaze.com/b2/docs/b2_list_buckets.html
        /// </summary>
        Task<JSON_ListBuckets> ListBuckets(string AccountID, Utilitiez.BucketTypesEnum bucketTypes = Utilitiez.BucketTypesEnum.all);

        /// <summary>
        /// Lists the names of all files in a bucket, starting at a given name.
        /// https://www.backblaze.com/b2/docs/b2_list_file_names.html
        /// </summary>
        Task<JSON_List> List(string DestinationBucketID, string StartWith = null, string Contains = null, string DelimiterMark = null, int Limit = 999);

        /// <summary>
        /// Uploads one file to B2, returning its unique file ID.
        /// https://www.backblaze.com/b2/docs/b2_upload_file.html
        /// </summary>
        Task<JSON_FileMetadata> Upload(object FileToUpload, Utilitiez.UploadTypes UploadType, string DestinationBucketID, string FileName, string SHA1, IProgress<ReportStatus> ReportCls = null, System.Threading.CancellationToken token = default);

        /// <summary>
        /// Gets information about one file stored in B2.
        /// https://www.backblaze.com/b2/docs/b2_get_file_info.html
        /// </summary>
        Task<JSON_FileMetadata> FileMetadata(string DestinationFileID);

        /// <summary>
        /// Deletes the bucket specified. Only buckets that contain no version of any files can be deleted.
        /// https://www.backblaze.com/b2/docs/b2_delete_bucket.html
        /// </summary>
        Task<JSON_BucketMetadata> DeleteBucket(string AccountID, string DestinationBucketID);
       
        /// <summary>
        /// Creates a new bucket. A bucket belongs to the account used to create it.
        /// https://www.backblaze.com/b2/docs/b2_create_bucket.html
        /// </summary>
        Task<JSON_BucketMetadata> CreateBucket(string AccountID, string BucketName, Utilitiez.BucketTypesEnum BucketType);

        /// <summary>
        /// Creates a new file by copying from an existing file.
        /// https://www.backblaze.com/b2/docs/b2_copy_file.html
        /// </summary>
        Task<JSON_FileMetadata> CopyFile(string SorceFileID, string DestinationBucketID, string RenameTo);

        /// <summary>
        /// Deletes one version of a file from B2.
        /// https://www.backblaze.com/b2/docs/b2_delete_file_version.html
        /// </summary>
        Task<bool> DeleteFile(string DestinationFileID, string DestinationFileName);

        /// <summary>
        /// Update an existing bucket.
        /// https://www.backblaze.com/b2/docs/b2_update_bucket.html
        /// </summary>
        Task<JSON_BucketMetadata> UpdateBucket(string AccountID, string DestinationBucketID, Utilitiez.BucketTypesEnum BucketType);


        Task PublicBucket_DownloadFile(string DestinationFileID, string FileSaveDir, string FileName, IProgress<ReportStatus> ReportCls = null, System.Threading.CancellationToken token = default);
        Task PublicBucket_DownloadLargeFile(string DestinationFileID, string FileSaveDir, string FileName, IProgress<ReportStatus> ReportCls = null, System.Threading.CancellationToken token = default);
        Task<System.IO.Stream> PublicBucket_DownloadFileAsStream(string DestinationFileID, IProgress<ReportStatus> ReportCls = null, System.Threading.CancellationToken token = default);
        Task PrivateBucket_DownloadFile(string DestinationFileID, string FileSaveDir, string FileName, IProgress<ReportStatus> ReportCls = null, System.Threading.CancellationToken token = default);
        Task PrivateBucket_DownloadLargeFile(string DestinationFileID, string FileSaveDir, string FileName, IProgress<ReportStatus> ReportCls = null, System.Threading.CancellationToken token = default);
        Task<System.IO.Stream> PrivateBucket_DownloadFileAsStream(string DestinationFileID, IProgress<ReportStatus> ReportCls = null, System.Threading.CancellationToken token = default);

        /// <summary>
        /// </summary>
        /// <param name="downloadApiUrl">JSON_AuthorizeAccount</param>
        string PublicBucket_GetDirectUrl(string downloadApiUrl, string BucketName, string FileName);
    }
}