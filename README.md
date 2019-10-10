## BackBlazeSDK ##

`Download:`[https://github.com/loudKode/BackBlazeSDK/releases](https://github.com/loudKode/BackBlazeSDK/releases)<br>
`NuGet:`
[![NuGet](https://img.shields.io/nuget/v/DeQmaTech.BackBlazeSDK.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/DeQmaTech.BackBlazeSDK)<br>


**Features**

* Assemblies for .NET 4.5.2 and .NET Standard 2.0 and .NET Core 2.1
* Just one external reference (Newtonsoft.Json)
* Easy installation using NuGet
* Upload/Download tracking support
* Proxy Support
* Upload/Download cancellation support

# Functions:
* ListBuckets
* List
* Upload
* FileMetadata
* DeleteBucket
* CreateBucket
* CopyFile
* DeleteFile
* UpdateBucket
* PublicBucket_DownloadFile
* PublicBucket_DownloadFileAsStream
* PrivateBucket_DownloadFile
* PrivateBucket_DownloadFileAsStream
* PublicBucket_GetDirectUrl


# Example:
```vb.net
    Async Sub Get_24Hour_Token()
        Dim tkn = Await BackBlazeSDK.GetToken.GetToken_24Hrs("ApplicationKeyID OR accountID", "ApplicationKey")
        DataGridView1.Rows.Add(tkn.accountId, tkn.apiUrl, tkn.authorizationToken, tkn.downloadApiUrl)
    End Sub
```
```vb.net
    Sub SetClient()
        Dim MyClient As BackBlazeSDK.IClient = New BackBlazeSDK.BClient("tkn.apiUrl", "tkn.authorizationToken")
    End Sub
```
```vb.net
    Sub SetClientWithOptions()
        Dim Optians As New BackBlazeSDK.ConnectionSettings With {.CloseConnection = True, .TimeOut = TimeSpan.FromMinutes(30), .Proxy = New BackBlazeSDK.ProxyConfig With {.ProxyIP = "172.0.0.0", .ProxyPort = 80, .ProxyUsername = "myname", .ProxyPassword = "myPass", .SetProxy = True}}
        Dim MyClient As BackBlazeSDK.IClient = New BackBlazeSDK.BClient("tkn.apiUrl", "tkn.authorizationToken", Optians)
    End Sub
```
```vb.net
    Async Sub ListMyBuckets()
        Dim result = Await MyClient.ListBuckets("my account Id", BucketTypesEnum.allPublic)
        For Each vid In result.BucketsList
            DataGridView1.Rows.Add(vid.bucketName, vid.bucketId, vid.bucketType, vid.accountId)
        Next
    End Sub
```
```vb.net
    Async Sub CreateNewBucket()
        Dim result = Await MyClient.CreateBucket("my account Id", "new bucket name", BucketTypesEnum.allPrivate)
        DataGridView1.Rows.Add(result.bucketName, result.bucketId, result.bucketType)
    End Sub
```
```vb.net
    Async Sub DeleteABucket()
        Dim result = Await MyClient.DeleteBucket("my account Id", "my bucket id")
        DataGridView1.Rows.Add(result.bucketName, result.bucketId, result.bucketType)
    End Sub
```
```vb.net
    Async Sub ListMyFiles()
        Dim result = Await MyClient.List("bucket Id", Nothing, Nothing, Nothing, 500)
        For Each vid In result.FilesList
            DataGridView1.Rows.Add(vid.Name, vid.bucketId, vid.fileId, vid.Path, vid.Size)
        Next
    End Sub
```
```vb.net
    Async Sub GetFileMetadata()
        Dim result = Await MyClient.FileMetadata("file Id")
        DataGridView1.Rows.Add(result.Name, result.bucketId, result.fileId, result.Path, result.Size)
    End Sub
```
```vb.net
    Async Sub DeleteAFile()
        Dim result = Await MyClient.DeleteFile("file Id", "file name")
        DataGridView1.Rows.Add(result)
    End Sub
```
```vb.net
    Async Sub Upload_Local_WithProgressTracking()
        Dim UploadCancellationToken As New Threading.CancellationTokenSource()
        Dim _ReportCls As New Progress(Of BackBlazeSDK.ReportStatus)(Sub(ReportClass As BackBlazeSDK.ReportStatus)
                                                                         Label1.Text = String.Format("{0}/{1}", (ReportClass.BytesTransferred), (ReportClass.TotalBytes))
                                                                         ProgressBar1.Value = CInt(ReportClass.ProgressPercentage)
                                                                         Label2.Text = CStr(ReportClass.TextStatus)
                                                                     End Sub)
        Dim RSLT = Await MyClient.Upload("J:\DB\myvideo.mp4", UploadTypes.FilePath, "bucked id", "myvideo.mp4", FileHash(IO.File.ReadAllBytes("J:\DB\myvideo.mp4")), _ReportCls, UploadCancellationToken.Token)
    End Sub
```
```vb.net
    Async Sub DownloadFileLocateInPublicBucket_WithProgressTracking()
        Dim DownloadCancellationToken As New Threading.CancellationTokenSource()
        Dim _ReportCls As New Progress(Of BackBlazeSDK.ReportStatus)(Sub(ReportClass As BackBlazeSDK.ReportStatus)
                                                                         Label1.Text = String.Format("{0}/{1}", (ReportClass.BytesTransferred), (ReportClass.TotalBytes))
                                                                         ProgressBar1.Value = CInt(ReportClass.ProgressPercentage)
                                                                         Label2.Text = CStr(ReportClass.TextStatus)
                                                                     End Sub)
        Await MyClient.PublicBucket_DownloadFile("file id", "J:\DB\", "myvideo.mp4", _ReportCls, DownloadCancellationToken.Token)
    End Sub
```
