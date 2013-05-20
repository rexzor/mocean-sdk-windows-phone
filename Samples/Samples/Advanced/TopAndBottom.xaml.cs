using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace com.moceanmobile.mast.samples.Advanced
{
    public partial class TopAndBottom : PhoneApplicationPage
    {
        public TopAndBottom()
        {
            InitializeComponent();
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            PhoneApplicationService.Current.State["refreshAdView"] = new MASTAdView[] { topAdView, bottomAdView };
            NavigationService.Navigate(new Uri("/RefreshPage.xaml", UriKind.Relative));
        }
    }
}