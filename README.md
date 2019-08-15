# BackBlazeSDK
.NET API Library for BackBlaze.com


`Download:`[https://github.com/loudKode/BackBlazeSDK/releases](https://github.com/loudKode/BackBlazeSDK/releases)
# Functions:
```vb.net
    Function ListBuckets(AccountID As String, Optional bucketTypes As BBZutilities.BucketTypesEnum = Nothing) As Task(Of JSON_ListBuckets)
    Function List(DestinationBucketID As String, Optional StartWith As String = Nothing, Optional Contains As String = Nothing, Optional DelimiterMark As String = "/", Optional Limit As Integer = 999) As Task(Of JSON_List)
    Function Upload(FileToUpload As Object, UploadType As BClient.UploadTypes, DestinationBucketID As String, FileName As String, Optional ReportCls As IProgress(Of ReportStatus) = Nothing, Optional _proxi As ProxyConfig = Nothing, Optional token As Threading.CancellationToken = Nothing) As Task(Of JSON_FileMetadata)
    Function FileMetadata(DestinationFileID As String) As Task(Of JSON_FileMetadata)
    Function DeleteBucket(AccountID As String, DestinationBucketID As String) As Task(Of JSON_BucketMetadata)
    Function CreateBucket(AccountID As String, BucketName As String, BucketType As BBZutilities.BucketTypesEnum) As Task(Of JSON_BucketMetadata)
    Function CopyFile(SorceFileID As String, DestinationBucketID As String, RenameTo As String) As Task(Of JSON_FileMetadata)
    Function DeleteFile(DestinationFileID As String, DestinationFileName As String) As Task(Of Boolean)
    Function UpdateBucket(AccountID As String, DestinationBucketID As String, BucketType As BBZutilities.BucketTypesEnum) As Task(Of JSON_BucketMetadata)
    Function PublicBucket_DownloadFile(DestinationFileID As String, FileSaveDir As String, FileName As String, Optional ReportCls As IProgress(Of ReportStatus) = Nothing, Optional _proxi As ProxyConfig = Nothing, Optional TimeOut As Integer = 60, Optional token As Threading.CancellationToken = Nothing) As Task
    Function PublicBucket_DownloadFileAsStream(DestinationFileID As String, Optional ReportCls As IProgress(Of ReportStatus) = Nothing, Optional _proxi As ProxyConfig = Nothing, Optional TimeOut As Integer = 60, Optional token As Threading.CancellationToken = Nothing) As Task(Of IO.Stream)
    Function PrivateBucket_DownloadFile(DestinationFileID As String, FileSaveDir As String, FileName As String, Optional ReportCls As IProgress(Of ReportStatus) = Nothing, Optional _proxi As ProxyConfig = Nothing, Optional TimeOut As Integer = 60, Optional token As Threading.CancellationToken = Nothing) As Task
    Function PrivateBucket_DownloadFileAsStream(DestinationFileID As String, Optional ReportCls As IProgress(Of ReportStatus) = Nothing, Optional _proxi As ProxyConfig = Nothing, Optional TimeOut As Integer = 60, Optional token As Threading.CancellationToken = Nothing) As Task(Of IO.Stream)
    Function PublicBucket_GenerateDownloadUrl(downloadApiUrl As String, BucketName As String, FileName As String) As String
```

# Example:
```vb.net
Dim cLENT As BackBlazeSDK.IClient = New BackBlazeSDK.BClient("https://api002.backblazeb2.com", "xxxxxxxxxxxx")
Dim RSLT = Await BackBlazeSDK.GetToken.GetToken_24Hrs("xxxxxxxxx", "xxxxxxx")
Dim RSLT = Await cLENT.List(xxxxxxxxxx)
Dim RSLT = Await cLENT.ListBuckets("xxxxxxxxx")
Dim RSLT = Await cLENT.CreateBucket("xxxxxxx", "tztbukt", BackBlazeSDK.BBZutilities.BucketTypesEnum.allPublic)
Dim RSLT = Await cLENT.CopyFile("xxxxxx", "xxxxxxx", "gogo.png")
Dim RSLT = Await cLENT.DeleteFile("xxxxxxx", "0ap5.jpeg")
Dim RSLT = cLENT.PublicBucket_GenerateDirectUrl("https://f002.backblazeb2.com", "xxxxx", "289.mp4")


        Dim frm As New DeQma.FileFolderDialogs.VistaFolderBrowserDialog With {.ShowNewFolderButton = True, .UseDescriptionForTitle = True, .Description = "Select PATH to download to"}
        If frm.ShowDialog = DialogResult.OK AndAlso Not String.IsNullOrEmpty(frm.SelectedPath) Then
            Dim DownloadCancellationToken As New Threading.CancellationTokenSource()
            Dim progressIndicator_ReportCls As New Progress(Of BackBlazeSDK.ReportStatus)(Sub(ReportClass As BackBlazeSDK.ReportStatus)
                                                                                              Label1.Text = String.Format("{0}/{1}", ISisFunctions.Bytes_To_KbMbGb.SetBytes(ReportClass.BytesTransferred), ISisFunctions.Bytes_To_KbMbGb.SetBytes(ReportClass.TotalBytes))
                                                                                              ProgressBar1.Value = CInt(ReportClass.ProgressPercentage)
                                                                                              Label2.Text = If(CStr(ReportClass.TextStatus) Is Nothing, "Downloading...", CStr(ReportClass.TextStatus))
                                                                                          End Sub)
            Await cLENT.PublicBucket_DownloadFile("xxxxxxxx", frm.SelectedPath, "CloseW20.png", progressIndicator_ReportCls, New BackBlazeSDK.ProxyConfig With {.SetProxi = False, .ProxiIP = "194.135.216.178", .ProxiPort = "56805"}, 60, DownloadCancellationToken.Token)
        End If


        Try
            Dim frm As New DeQma.FileFolderDialogs.VistaOpenFileDialog With {.Multiselect = False, .Title = "Select image/s to upload"}
            If frm.ShowDialog = DialogResult.OK AndAlso Not String.IsNullOrEmpty(frm.FileName) Then
                Dim UploadCancellationToken As New Threading.CancellationTokenSource()
                Dim progressIndicator_ReportCls As New Progress(Of BackBlazeSDK.ReportStatus)(Sub(ReportClass As BackBlazeSDK.ReportStatus)
                                                                                                  Label1.Text = String.Format("{0}/{1}", ISisFunctions.Bytes_To_KbMbGb.SetBytes(ReportClass.BytesTransferred), ISisFunctions.Bytes_To_KbMbGb.SetBytes(ReportClass.TotalBytes))
                                                                                                  ProgressBar1.Value = CInt(ReportClass.ProgressPercentage)
                                                                                                  Label2.Text = If(CStr(ReportClass.TextStatus) Is Nothing, "Uploading...", CStr(ReportClass.TextStatus))
                                                                                              End Sub)
                Dim fle = Await cLENT.Upload(frm.FileName, BackBlazeSDK.BClient.UploadTypes.FilePath, "xxxxxxxxx", IO.Path.GetFileName(frm.FileName), progressIndicator_ReportCls, Nothing, UploadCancellationToken.Token)
                DataGridView1.Rows.Add(fle.fileName, fle.fileId, fle.contentType, fle.CreatedDate, ISisFunctions.Bytes_To_KbMbGb.SetBytes(fle.Size))
            End If
        Catch ex As BackBlazeSDK.BackBlazeException
            MsgBox(ex.Message)
        End Try
```
