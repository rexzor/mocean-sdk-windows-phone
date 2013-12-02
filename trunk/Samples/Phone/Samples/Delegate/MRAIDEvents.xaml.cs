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
    public partial class MRAIDEvents : PhoneApplicationPage
    {
        public MRAIDEvents()
        {
            InitializeComponent();
        }

        private void adView_ProcessedRichmediaRequest(object sender, mast.MASTAdView.ProcessedRichmediaRequestEventArgs e)
        {
            // Since these events can come from a non-main/UI thread, dispatch properly.
            System.Windows.Deployment.Current.Dispatcher.BeginInvoke(delegate()
            {
                textBlock.Text += "URI:" + e.URI + " Handled:" + e.Handled + "\n\n";
            });
        }
    }
}