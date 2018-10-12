using System;
using System.IO;
using Foundation;
using QuickLook;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.SfChart.XForms;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(ChartToPDF.iOS.ChartToPDFDependencyService))]
namespace ChartToPDF.iOS
{
    public class ChartToPDFDependencyService : IChartToPDFDependencyService
    {
        public void ExportAsPDF(string filename, Stream chartStream, SfChart chart)
        {
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
            catch (Exception e)
            {

            }

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
}