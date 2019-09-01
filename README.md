# BackBlazeSDK
.NET API Library for BackBlaze.com


`Download:`[https://github.com/loudKode/BackBlazeSDK/releases](https://github.com/loudKode/BackBlazeSDK/releases)<br>
`NuGet:`
[![NuGet](https://img.shields.io/nuget/v/DeQmaTech.BackBlazeSDK.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/DeQmaTech.BackBlazeSDK)<br>

# Functions:
[https://github.com/loudKode/BackBlazeSDK/blob/master/IClient.cs](https://github.com/loudKode/BackBlazeSDK/blob/master/IClient.cs)

# Example:
```vb.net
Dim tkn = Await BackBlazeSDK.GetToken.GetToken_24Hrs("xxxxxxxxx", "xxxxxxx")
Dim cLENT As BackBlazeSDK.IClient = New BackBlazeSDK.BClient("https://api002.backblazeb2.com", "xxxxxxxxxxxx")
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
