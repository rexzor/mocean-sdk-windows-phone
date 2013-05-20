using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace com.moceanmobile.mast.samples
{
    public partial class RefreshPage : PhoneApplicationPage
    {
        public RefreshPage()
        {
            InitializeComponent();

            this.Loaded += RefreshPage_Loaded;
            this.DoneButton.Click += DoneButton_Click;
            this.CancelButton.Click += CancelButton_Click;
        }

        private MASTAdView adView = null;
        private List<MASTAdView> adViews = null;
        private MASTAdView topAdView = null;
        private MASTAdView bottomAdView = null;
        
        private void RefreshPage_Loaded(object sender, RoutedEventArgs e)
        {
            object obj = null;

            if (PhoneApplicationService.Current.State.TryGetValue("refreshAdView", out obj) == true)
                PhoneApplicationService.Current.State.Remove("refreshAdView");

            if (obj == null)
                throw new ArgumentNullException("refreshAdView");

            if (obj is MASTAdView)
            {
                adView = (MASTAdView)obj;
            }
            else if (obj is List<MASTAdView>)
            {
                adViews = (List<MASTAdView>)obj;
                adView = adViews.ElementAt(0);
            }
            else if (obj is MASTAdView[])
            {
                topAdView = ((MASTAdView[])obj)[0];
                bottomAdView = ((MASTAdView[])obj)[1];
            }

            if (adView != null)
            {
                ZonePanel.Visibility = System.Windows.Visibility.Visible;
                TopBottomPanel.Visibility = System.Windows.Visibility.Collapsed;

                ZoneTextBox.Text = adView.Zone.ToString();
            }
            else
            {
                ZonePanel.Visibility = System.Windows.Visibility.Collapsed;
                TopBottomPanel.Visibility = System.Windows.Visibility.Visible;

                TopZoneTextBox.Text = topAdView.Zone.ToString();
                BottomZoneTextBox.Text = bottomAdView.Zone.ToString();
            }
        }

        private void DoneButton_Click(object sender, RoutedEventArgs e)
        {
            if (adView != null)
            {
                if (adViews != null)
                {
                    foreach (MASTAdView av in adViews)
                    {
                        av.Zone = int.Parse(ZoneTextBox.Text);
                    }
                }
                else
                {
                    adView.Zone = int.Parse(ZoneTextBox.Text);
                }
            }
            else
            {
                topAdView.Zone = int.Parse(TopZoneTextBox.Text);
                bottomAdView.Zone = int.Parse(BottomZoneTextBox.Text);
            }

            NavigationService.GoBack();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}