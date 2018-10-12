using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace ChartToPDF
{
    public class ChartViewModel
    {
        public ObservableCollection<ChartModel> ColumnData { get; set; }

        public ChartViewModel()
        {
            ColumnData = new ObservableCollection<ChartModel>
            {
                new ChartModel("A", 45),
                new ChartModel("B", 54),
                new ChartModel("C", 65),
                new ChartModel("D", 23),
                new ChartModel("E", 55),
            };
        }
    }
}
