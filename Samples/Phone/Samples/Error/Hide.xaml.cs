using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace com.moceanmobile.mast.samples.Error
{
    public partial class Hide : PhoneApplicationPage
    {
        public Hide()
        {
            InitializeComponent();
        }

        private void adView_AdReceived(object sender, EventArgs e)
        {
            if (sender != adView)
                return;

            System.Windows.Deployment.Current.Dispatcher.BeginInvoke(delegate()
            {
                adView.Visibility = System.Windows.Visibility.Visible;
            });
        }

        private void adView_AdFailed(object sender, mast.MASTAdView.AdFailedEventArgs e)
        {
            if (sender != adView)
                return;

            System.Windows.Deployment.Current.Dispatcher.BeginInvoke(delegate()
            {
                adView.Visibility = System.Windows.Visibility.Collapsed;
            });
        }
    }
}