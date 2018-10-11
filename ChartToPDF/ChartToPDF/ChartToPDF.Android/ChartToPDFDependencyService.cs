using System;
using Android.Content;
using Xamarin.Forms;
using System.IO;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.SfChart.XForms;
using Syncfusion.SfChart.XForms.Droid;
using Java.IO;

[assembly: Dependency(typeof(ChartToPDF.Droid.ChartToPDFDependencyService))]
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
                : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

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
            catch (Exception) {}

            if (!file.Exists()) return;

            var path = Android.Net.Uri.FromFile(file);
            var extension = Android.Webkit.MimeTypeMap.GetFileExtensionFromUrl(Android.Net.Uri.FromFile(file).ToString());
            var mimeType = Android.Webkit.MimeTypeMap.Singleton.GetMimeTypeFromExtension(extension);
            var intent = new Intent(Intent.ActionView);
            intent.SetDataAndType(path, mimeType);
            MainActivity.Activity.StartActivity(Intent.CreateChooser(intent, "Choose App"));
        }
    }
}