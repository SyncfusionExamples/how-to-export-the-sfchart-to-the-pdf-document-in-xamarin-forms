using Syncfusion.SfChart.XForms.UWP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms.Platform.UWP;
using NativeChart = Syncfusion.UI.Xaml.Charts;
using System.IO;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Syncfusion.SfChart.XForms;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Media.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;

[assembly: Xamarin.Forms.Dependency(typeof(ChartToPDF.UWP.ChartToPDFDependencyService))]
namespace ChartToPDF.UWP
{
    public class ChartToPDFDependencyService : IChartToPDFDependencyService
    {
        public async void ExportAsPDF(string filename, Stream chartStream, SfChart chart)
        {
            NativeChart.SfChart nativeChart = SfChartRenderer.GetNativeObject(typeof(SfChart), chart) as NativeChart.SfChart;

            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFile file = await localFolder.CreateFileAsync("filename", CreationCollisionOption.ReplaceExisting);

            if (file != null)
            {
                IRandomAccessStream stream = await file.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);
                await Save_UWP(nativeChart, stream);
            }

            using (var fileStream = await file.OpenAsync(FileAccessMode.Read))
            {
                var reader = new DataReader(fileStream.GetInputStreamAt(0));
                var bytes = new byte[fileStream.Size];
                await reader.LoadAsync((uint)fileStream.Size);
                reader.ReadBytes(bytes);
                MemoryStream imageStream = new MemoryStream(bytes);

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
