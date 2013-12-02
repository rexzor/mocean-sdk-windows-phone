using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace com.moceanmobile.mast.samples.Delegate
{
    public partial class Logging : PhoneApplicationPage
    {
        public Logging()
        {
            InitializeComponent();
        }

        private void adView_LoggingEvent(object sender, mast.MASTAdView.LoggingEventEventArgs e)
        {
            // Since these events can come from a non-main/UI thread, dispatch properly.
            System.Windows.Deployment.Current.Dispatcher.BeginInvoke(delegate()
            {
                textBlock.Text += "LogLevel:" + e.LogLevel + " Entry:" + e.Entry + "\n\n";
            });
        }
    }
}