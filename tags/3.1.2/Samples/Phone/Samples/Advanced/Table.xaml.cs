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
    public partial class Table : PhoneApplicationPage
    {
        private List<MASTAdView> adViews = new List<MASTAdView>();

        public Table()
        {
            InitializeComponent();
        }

        private void MASTAdView_Loaded(object sender, RoutedEventArgs e)
        {
            if (adViews.Contains((MASTAdView)sender))
                return;

            adViews.Add((MASTAdView)sender);
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            PhoneApplicationService.Current.State["refreshAdView"] = adViews;
            NavigationService.Navigate(new Uri("/RefreshPage.xaml", UriKind.Relative));
        }
    }
}