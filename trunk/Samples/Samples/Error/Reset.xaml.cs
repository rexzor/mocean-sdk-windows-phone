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
    public partial class Reset : PhoneApplicationPage
    {
        public Reset()
        {
            InitializeComponent();
        }

        private void adView_AdFailed(object sender, mast.MASTAdView.AdFailedEventArgs e)
        {
            if (sender != adView)
                return;

            System.Windows.Deployment.Current.Dispatcher.BeginInvoke(delegate()
            {
                // Remove any existing ad content (last rendered ad).
                // The ad control itself will still be displayed but appear as if no ad has been loaded.
                adView.RemoveContent();
            });
        }
    }
}