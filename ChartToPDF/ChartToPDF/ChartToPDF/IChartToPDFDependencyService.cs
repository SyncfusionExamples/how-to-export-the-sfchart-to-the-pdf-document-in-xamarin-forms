using Syncfusion.SfChart.XForms;
using System.IO;

namespace ChartToPDF
{
    public interface IChartToPDFDependencyService
    {
        void ExportAsPDF(string filename, Stream chartStream, SfChart chart);
    }
}