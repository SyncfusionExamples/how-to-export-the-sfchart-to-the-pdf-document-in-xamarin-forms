# How to export the SfChart to the PDF document in Xamarin.Forms

You can export a chart to PDF using the **DependencyService** and [GetStream](https://help.syncfusion.com/cr/xamarin/Syncfusion.SfChart.XForms.SfChart.html#Syncfusion_SfChart_XForms_SfChart_GetStream) method in the chart for Android and iOS. From the chart stream, you can get the PDF of the chart using **Syncfusion.Pdf.PdfDocument**. The [GetStream](https://help.syncfusion.com/cr/xamarin/Syncfusion.SfChart.XForms.SfChart.html#Syncfusion_SfChart_XForms_SfChart_GetStream) method restricts to work only for Android and iOS platforms. So, for UWP, export the chart as an image stream first, and then get the pdf for the chart by using image stream.

**PCL:**
```
public interface IChartToPDFDependencyService
{
    void ExportAsPDF(string filename, Stream chartStream, SfChart chart); 
}
private void ExportAsPDF(object sender, EventArgs e)
{
    DependencyService.Get<IChartToPDFDependencyService>().ExportAsPDF("Chart.pdf", chart.GetStream(), chart);
}
```

**Android**
```
[assembly: Dependency(typeof(ChartToPDFDependencyService))]
namespace ChartToPDF.Droid
{
    public class ChartToPDFDependencyService : IChartToPDFDependencyService
    {
        Java.IO.File file;
 
        public void ExportAsPDF(string filename,Stream chartStream, SfChart chart)
        {
            try
            {
                //Create a new PDF document.
                var document = new PdfDocument();
                var page = document.Pages.Add();
                var graphics = page.Graphics;
                graphics.DrawImage(PdfImage.FromStream(chartStream), 0, 0, page.GetClientSize().Width, page.GetClientSize().Height);
                MemoryStream stream = new MemoryStream();
                document.Save(stream);
                document.Close(true);
                SavePDF(filename, stream);
            }
            finally
            {
                chartStream.Flush();
                chartStream.Close();
                var nativeChart = SfChartRenderer.GetNativeObject(typeof(SfChart), chart);
                ((Com.Syncfusion.Charts.SfChart)nativeChart).DrawingCacheEnabled = false;
            }
        }
 
        private void SavePDF(string fileName, MemoryStream stream)
        {
            var root = Android.OS.Environment.IsExternalStorageEmulated
                ? Android.OS.Environment.ExternalStorageDirectory.ToString()
                : System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            var myDir = new Java.IO.File(root + "/Chart");
            myDir.Mkdir();
 
            if (file != null && file.Exists())
                file.Delete();
 
            file = new Java.IO.File(myDir, fileName);
 
            try
            {
                var outs = new FileOutputStream(file);
                outs.Write(stream.ToArray());
                outs.Flush();
                outs.Close();
            }
            catch (Exception) { }
 
            if (!file.Exists()) return;
 
            var path = Android.Net.Uri.FromFile(file);
            var extension = Android.Webkit.MimeTypeMap.GetFileExtensionFromUrl(Android.Net.Uri.FromFile(file).ToString());
            var mimeType = Android.Webkit.MimeTypeMap.Singleton.GetMimeTypeFromExtension(extension);
            var intent = new Intent(Intent.ActionView);
            intent.SetDataAndType(path, mimeType);
            Forms.Context.StartActivity(Intent.CreateChooser(intent, "Choose App"));
        }
    }
}
```

**iOS:**
```
[assembly: Dependency(typeof(ChartToPDFDependencyService))]
namespace ChartToPDF.iOS
{
    public class ChartToPDFDependencyService : IChartToPDFDependencyService
    {
        public void ExportAsPDF(string filename, Stream chartStream, SfChart chart)
        {
            //Create a new PDF document.
            var doc = new PdfDocument();
            doc.PageSettings.Margins.All = 0;
            var page = doc.Pages.Add();
            var g = page.Graphics;
            g.DrawImage(PdfImage.FromStream(chartStream), 0, 0, 600, 800);
 
            MemoryStream stream = new MemoryStream();
            doc.Save(stream);
            doc.Close(true);
 
            var path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var filePath = Path.Combine(path, filename);
            try
            {
                var fileStream = File.Open(filePath, FileMode.Create);
                stream.Position = 0;
                stream.CopyTo(fileStream);
                fileStream.Flush();
                fileStream.Close();
            }
            catch (Exception e) { }
 
            var currentController = UIApplication.SharedApplication.KeyWindow.RootViewController;
            while (currentController.PresentedViewController != null)
                currentController = currentController.PresentedViewController;
            var currentView = currentController.View;
            var qlPreview = new QLPreviewController();
            QLPreviewItem item = new QLPreviewItemBundle(filename, filePath);
            qlPreview.DataSource = new PreviewControllerDS(item);
            currentController.PresentViewController(qlPreview, true, null);
        }        
    }
 
    public class QLPreviewItemBundle : QLPreviewItem
    {
        string _fileName, _filePath;
 
        public QLPreviewItemBundle(string fileName, string filePath)
        {
            _fileName = fileName;
            _filePath = filePath;
        }
 
        public override string ItemTitle
        {
            get
            {
                return _fileName;
            }
        }
 
        public override NSUrl ItemUrl
        {
            get
            {
                var documents = NSBundle.MainBundle.BundlePath;
                var lib = Path.Combine(documents, _filePath);
                var url = NSUrl.FromFilename(lib);
                return url;
            }
        }
    }
 
    public class PreviewControllerDS : QLPreviewControllerDataSource
    {
        private QLPreviewItem _item;
 
        public PreviewControllerDS(QLPreviewItem item)
        {
            _item = item;
        }
 
        public override nint PreviewItemCount(QLPreviewController controller)
        {
            return 1;
        }
 
        public override IQLPreviewItem GetPreviewItem(QLPreviewController controller, nint index)
        {
            return _item;
        }
    }
}
```

**UWP:**
```
[assembly:Dependency(typeof(ChartToPDFDependencyService))]
namespace ChartToPDF.UWP
{
    public class ChartToPDFDependencyService : IChartToPDFDependencyService
    {
        public async void ExportAsPDF(string filename, Stream chartStream , SfChart chart)
        {
            NativeChart.SfChart nativeChart = SfChartRenderer.GetNativeObject(typeof(SfChart), chart) as NativeChart.SfChart;
 
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFile  file = await localFolder.CreateFileAsync("filename", CreationCollisionOption.ReplaceExisting);
 
            if (file != null)
            {
                IRandomAccessStream stream = await file.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);
                await Save_UWP(nativeChart, stream);
            }
 
            using (var fileStream = await file.OpenAsync(FileAccessMode.Read))
            {
                var reader = new DataReader(fileStream.GetInputStreamAt(0));
                var bytes = new byte[fileStream.Size];
                await reader.LoadAsync((uint) fileStream.Size);
                reader.ReadBytes(bytes);
                MemoryStream  imageStream = new MemoryStream(bytes);
                
                if (file != null)
               {
                   var document = new PdfDocument();
                   var page = document.Pages.Add();
                   var graphics = page.Graphics;
                   PdfBitmap image = new PdfBitmap(imageStream);
                   graphics.DrawImage(image, 0, 0, page.GetClientSize().Width, page.GetClientSize().Height);
                   chartStream = new MemoryStream();
                   document.Save(chartStream);
                   document.Close(true);
                   Save(chartStream, filename, ".pdf");
                }
            }
        }
 
        async void Save(Stream stream, string filename, string type)
        {
            stream.Position = 0;
            StorageFile stFile;
 
            if (!(Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons")))
            {
                FileSavePicker savePicker = new FileSavePicker();
                savePicker.DefaultFileExtension = type;
                savePicker.SuggestedFileName = filename.Substring(0,filename.IndexOf("."));
                if(type == ".pdf")
                    savePicker.FileTypeChoices.Add("Adobe PDF Document", new List<string>() { type });
                else
                    savePicker.FileTypeChoices.Add("JPG", new List<string>() { type });
                stFile = await savePicker.PickSaveFileAsync();
            }
            else
            {
                StorageFolder local = Windows.Storage.ApplicationData.Current.LocalFolder;
                stFile = await local.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
            }
            if (stFile != null)
            {
                Windows.Storage.Streams.IRandomAccessStream fileStream = await stFile.OpenAsync(FileAccessMode.ReadWrite);
                Stream st = fileStream.AsStreamForWrite();
                st.Write((stream as MemoryStream).ToArray(), 0, (int)stream.Length);
                st.Flush();
                st.Dispose();
                fileStream.Dispose();
            }
        }
 
        public async Task Save_UWP(NativeChart.SfChart nativechart, IRandomAccessStream stream)
        {
            var renderTargetBitmap = new RenderTargetBitmap();
            await renderTargetBitmap.RenderAsync(nativechart);
            var pixels = await renderTargetBitmap.GetPixelsAsync();
            Guid encoderId = BitmapEncoder.JpegEncoderId;
            var encoder = await BitmapEncoder.CreateAsync(encoderId, stream);
            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, (uint)renderTargetBitmap.PixelWidth,
                (uint)renderTargetBitmap.PixelHeight, 96.0, 96.0, pixels.ToArray());
            await encoder.FlushAsync();
        }
    }
}
```

## Output:

| **Sample Output** | **Exported PDF** |
| -------- | --------- |
|  ![image](https://user-images.githubusercontent.com/53489303/200586736-9ee3d9aa-b308-41ab-9192-94e21a768679.png)  |  ![Exported Chart Output](https://user-images.githubusercontent.com/53489303/200586186-5197b103-fd7e-4f1d-84cb-75a0dc306bc1.png)  |

KB article - [How to export the Chart to the PDF document in Xamarin.Forms?](https://www.syncfusion.com/kb/9404/how-to-export-the-chart-to-the-pdf-document-in-xamarin-forms)

## <a name="requirements-to-run-the-demo"></a>Requirements to run the demo ##

* [Visual Studio 2017](https://visualstudio.microsoft.com/downloads/) or [Visual Studio for Mac](https://visualstudio.microsoft.com/vs/mac/).
* Xamarin add-ons for Visual Studio (available via the Visual Studio installer).

## <a name="troubleshooting"></a>Troubleshooting ##
### Path too long exception
If you are facing path too long exception when building this example project, close Visual Studio and rename the repository to short and build the project.
