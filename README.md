# BackBlazeSDK
.NET API Library for BackBlaze.com


`Download:`[https://github.com/loudKode/BackBlazeSDK/releases](https://github.com/loudKode/BackBlazeSDK/releases)<br>
`NuGet:`
[![NuGet](https://img.shields.io/nuget/v/DeQmaTech.BackBlazeSDK.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/DeQmaTech.BackBlazeSDK)<br>

**Features**
* Assemblies for .NET 4.5.2 and .NET Standard 2.0
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
**get token**
```vb.net
Dim tkn = Await BackBlazeSDK.GetToken.GetToken_24Hrs("Key_ID", "Application_Key")
```
**set client**
```vb.net
Dim cLENT As BackBlazeSDK.IClient = New BackBlazeSDK.BClient(tkn.apiUrl, tkn.authorizationToken)
```
**set client with proxy**
```vb.net
Dim roxy = New BackBlazeSDK.ProxyConfig With {.ProxyIP = "172.0.0.0", .ProxyPort = 80, .ProxyUsername = "myname", .ProxyPassword = "myPass", .SetProxy = true}
Dim cLENT As BackBlazeSDK.IClient = New BackBlazeSDK.BClient(tkn.apiUrl, tkn.authorizationToken, roxy)
```
**list files/folders**
```vb.net
Dim RSLT = Await cLENT.List(TextBox1.Text, Nothing, Nothing, Nothing, 300)
For Each fle As BackBlazeSDK.JSON.JSON_FileMetadata In RSLT.FilesList
    DataGridView1.Rows.Add(fle.Name, fle.fileId, fle.contentType, fle.CreatedDate,(fle.Size), fle.File_Folder.ToString, fle.ParentPath)
Next
```
**list buckets**
```vb.net
Dim RSLT = Await cLENT.ListBuckets("xxxxxxxxx")
```
**upload local file with progress tracking**
```vb.net
Dim UploadCancellationToken As New Threading.CancellationTokenSource()
Dim prog_ReportCls As New Progress(Of BackBlazeSDK.ReportStatus)(Sub(ReportClass As BackBlazeSDK.ReportStatus)
                   Label1.Text = String.Format("{0}/{1}",(ReportClass.BytesTransferred),(ReportClass.TotalBytes))
                   ProgressBar1.Value = CInt(ReportClass.ProgressPercentage)
                   Label2.Text = If(CStr(ReportClass.TextStatus)
                   End Sub)
Dim fle = Await cLENT.Upload("C:\myFile.exe", UploadTypes.FilePath, "BucketID", "myFile.exe","File-Hash-SHA1", prog_ReportCls , UploadCancellationToken.Token)
DataGridView1.Rows.Add(fle.fileName, fle.fileId, fle.contentType, fle.CreatedDate,(fle.Size))
```
