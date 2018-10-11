using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ChartToPDF
{
    public class ChartModel : INotifyPropertyChanged
    {
        private string xValue;
        public string XValue
        {
            get
            {
                return xValue;
            }

            set
            {
                if (value != xValue)
                {
                    xValue = value;
                    OnPropertyChanged("XValue");
                }
            }
        }

        private double yValue;
        public double YValue
        {
            get
            {
                return yValue;
            }

            set
            {
                if (value != yValue)
                {
                    yValue = value;
                    OnPropertyChanged("YValue");
                }
            }
        }

        public ChartModel(string xVal, double yVal)
        {
            XValue = xVal;
            YValue = yVal;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}