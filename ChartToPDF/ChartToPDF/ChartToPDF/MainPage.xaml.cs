using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ChartToPDF
{
	public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			InitializeComponent();
		}

        private void ExportAsPDF(object sender, EventArgs e)
        {
            DependencyService.Get<IChartToPDFDependencyService>().ExportAsPDF("Chart.pdf", chart.GetStream(), chart);
        }
    }
}
