using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace com.moceanmobile.mast.samples.Custom
{
    public partial class CustomAdSetup : PhoneApplicationPage
    {
        public CustomAdSetup()
        {
            InitializeComponent();
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            PhoneApplicationService.Current.State["refreshAdView"] = adView;
            NavigationService.Navigate(new Uri("/RefreshPage.xaml", UriKind.Relative));
        }

        private void ConfigureButton_Click(object sender, EventArgs e)
        {
            PhoneApplicationService.Current.State["configureAdView"] = adView;
            NavigationService.Navigate(new Uri("/CustomConfigurePage.xaml", UriKind.Relative));
        }
    }
}