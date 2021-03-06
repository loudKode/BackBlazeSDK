## BackBlazeSDK

`Download:`[https://github.com/loudKode/BackBlazeSDK/releases](https://github.com/loudKode/BackBlazeSDK/releases)<br>
`Help:`[https://github.com/loudKode/BackBlazeSDK/wiki](https://github.com/loudKode/BackBlazeSDK/wiki)<br>
`NuGet:`
[![NuGet](https://img.shields.io/nuget/v/DeQmaTech.BackBlazeSDK.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/DeQmaTech.BackBlazeSDK)<br>


**Features**
- [x] Assemblies for .NET 4.5.2 and .NET Standard 2.0 and .NET Core 2.1
- [x] Just one external reference (Newtonsoft.Json)
- [x] Easy installation using NuGet
- [x] Upload/Download tracking support
- [x] Proxy Support
- [x] Upload/Download cancellation support
- [ ] Support Upload/Download pause/resume 

# List of functions:
> * ListBuckets
> * List
> * Upload
> * FileMetadata
> * DeleteBucket
> * CreateBucket
> * CopyFile
> * DeleteFile
> * UpdateBucket
> * PublicBucket_DownloadFile
> * PublicBucket_DownloadFileAsStream
> * PrivateBucket_DownloadFile
> * PrivateBucket_DownloadFileAsStream
> * PublicBucket_GetDirectUrl


# Code simple:
```vb.net
Dim tkn = Await BackBlazeSDK.Authentication.GetToken_24Hrs("key_id", "app_id")
Dim cLENT As BackBlazeSDK.IClient = New BackBlazeSDK.BClient(tkn.apiUrl, tkn.authorizationToken, New BackBlazeSDK.ConnectionSettings With {.CloseConnection = True, .TimeOut = TimeSpan.FromMinutes(80), .Proxy = New BackBlazeSDK.ProxyConfig With {.SetProxy = True, .ProxyIP = "127.0.0.1", .ProxyPort = 80, .ProxyUsername = "user", .ProxyPassword = "123456"}})

Await cLENT.ListBuckets(tkn.accountId)
Await cLENT.List("bucket_id")
Await cLENT.CopyFile("file_id", "bucket_id", "mypic.jpg")
Await cLENT.CreateBucket(tkn.accountId, "bucket_name", BackBlazeSDK.Utilitiez.BucketTypesEnum.allPublic)
Await cLENT.DeleteBucket(tkn.accountId, "bucket_id")
Await cLENT.DeleteFile("file_id", "mypic.jpg")
Await cLENT.FileMetadata("file_id")
Await cLENT.UpdateBucket(tkn.accountId, "bucket_id", BackBlazeSDK.Utilitiez.BucketTypesEnum.allPrivate)
Dim CancelToken As New Threading.CancellationTokenSource()
Dim _ReportCls As New Progress(Of BackBlazeSDK.ReportStatus)(Sub(r) Console.WriteLine($"{r.BytesTransferred}/{r.TotalBytes}" + r.ProgressPercentage + If(r.TextStatus, "Downloading...")))
Await cLENT.PrivateBucket_DownloadFile("file_id", "C:\Down", "mypic.jpg", _ReportCls, CancelToken.Token)
Await cLENT.Upload("C:\Down\mypic.jpg", BackBlazeSDK.Utilitiez.UploadTypes.FilePath, "bucket_id", "mypic.jpg", BackBlazeSDK.Utilitiez.SHA1FileHash(IO.File.ReadAllBytes("C:\Down\mypic.jpg")), _ReportCls, CancelToken.Token)
cLENT.PublicBucket_GetDirectUrl(tkn.downloadApiUrl, "My_Bucket", "mypic.jpg")
```
