using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BackBlazeSDK.JSON;

namespace BackBlazeSDK
{
	public interface IClient
	{
		Task<JSON_ListBuckets> ListBuckets(string AccountID, BBZutilities.BucketTypesEnum bucketTypes = BBZutilities.BucketTypesEnum.all);

		Task<JSON_List> List(string DestinationBucketID, string StartWith = null, string Contains = null, string DelimiterMark = null, int Limit = 999);

		Task<JSON_FileMetadata> Upload(object FileToUpload, BClient.UploadTypes UploadType, string DestinationBucketID, string FileName, IProgress<ReportStatus> ReportCls = null, ProxyConfig _proxi = null, CancellationToken token = default(CancellationToken));

		Task<JSON_FileMetadata> FileMetadata(string DestinationFileID);

		Task<JSON_BucketMetadata> DeleteBucket(string AccountID, string DestinationBucketID);

		Task<JSON_BucketMetadata> CreateBucket(string AccountID, string BucketName, BBZutilities.BucketTypesEnum BucketType);

		Task<JSON_FileMetadata> CopyFile(string SorceFileID, string DestinationBucketID, string RenameTo);

		Task<bool> DeleteFile(string DestinationFileID, string DestinationFileName);

		Task<JSON_BucketMetadata> UpdateBucket(string AccountID, string DestinationBucketID, BBZutilities.BucketTypesEnum BucketType);

		Task PublicBucket_DownloadFile(string DestinationFileID, string FileSaveDir, string FileName, IProgress<ReportStatus> ReportCls = null, ProxyConfig _proxi = null, int TimeOut = 60, CancellationToken token = default(CancellationToken));

		Task<Stream> PublicBucket_DownloadFileAsStream(string DestinationFileID, IProgress<ReportStatus> ReportCls = null, ProxyConfig _proxi = null, int TimeOut = 60, CancellationToken token = default(CancellationToken));

		Task PrivateBucket_DownloadFile(string DestinationFileID, string FileSaveDir, string FileName, IProgress<ReportStatus> ReportCls = null, ProxyConfig _proxi = null, int TimeOut = 60, CancellationToken token = default(CancellationToken));

		Task<Stream> PrivateBucket_DownloadFileAsStream(string DestinationFileID, IProgress<ReportStatus> ReportCls = null, ProxyConfig _proxi = null, int TimeOut = 60, CancellationToken token = default(CancellationToken));

		string PublicBucket_GetDirectUrl(string downloadApiUrl, string BucketName, string FileName);
	}
}
