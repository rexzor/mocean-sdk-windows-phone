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
    public partial class CustomConfigurePage : PhoneApplicationPage
    {
        private MASTAdView adView = null;

        public CustomConfigurePage()
        {
            InitializeComponent();
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            object obj = null;

            if (PhoneApplicationService.Current.State.TryGetValue("configureAdView", out obj) == true)
                PhoneApplicationService.Current.State.Remove("configureAdView");

            if (obj == null)
                return;

            adView = (MASTAdView)obj;

            UseInternalBrowserCheckBox.IsChecked = adView.UseInteralBrowser;

            xTextBox.Text = adView.Margin.Left.ToString();
            yTextBox.Text = adView.Margin.Right.ToString();
            
            if (adView.HorizontalAlignment != System.Windows.HorizontalAlignment.Stretch)
                widthTextBox.Text = adView.Width.ToString();

            heightTextBox.Text = adView.Height.ToString();

            string value = null;
            if (adView.AdRequestParameters.TryGetValue("size_x", out value) == true)
                maxWidthTextBox.Text = value;

            if (adView.AdRequestParameters.TryGetValue("size_y", out value) == true)
                maxHeightTextBox.Text = value;

            if (adView.AdRequestParameters.TryGetValue("min_size_x", out value) == true)
                minWidthTextBox.Text = value;

            if (adView.AdRequestParameters.TryGetValue("min_size_y", out value) == true)
                minHeightTextBox.Text = value;
        }

        private void DoneButton_Click(object sender, RoutedEventArgs e)
        {
            if (adView != null)
            {
                if (UseInternalBrowserCheckBox.IsChecked.HasValue)
                    adView.UseInteralBrowser = (bool)UseInternalBrowserCheckBox.IsChecked;

                string value = xTextBox.Text;
                if (string.IsNullOrWhiteSpace(value))
                {
                    adView.Margin = new Thickness(0, adView.Margin.Top, adView.Margin.Right, adView.Margin.Bottom);
                }
                else
                {
                    adView.Margin = new Thickness(double.Parse(value), adView.Margin.Top, adView.Margin.Right, adView.Margin.Bottom);
                }

                value = yTextBox.Text;
                if (string.IsNullOrWhiteSpace(value))
                {
                    adView.Margin = new Thickness(adView.Margin.Left, 0, adView.Margin.Right, adView.Margin.Bottom);
                }
                else
                {
                    adView.Margin = new Thickness(adView.Margin.Left, double.Parse(value), adView.Margin.Right, adView.Margin.Bottom);
                } 
                
                value = widthTextBox.Text;
                if (string.IsNullOrWhiteSpace(value))
                {
                    adView.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                    adView.Width = Double.NaN;
                }
                else
                {
                    adView.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                    adView.Width = int.Parse(value);
                }

                value = heightTextBox.Text;
                if (string.IsNullOrWhiteSpace(value))
                {
                    adView.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                    adView.Height = 50;
                }
                else
                {
                    adView.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                    adView.Height = int.Parse(value);
                }

                value = maxWidthTextBox.Text;
                if (string.IsNullOrWhiteSpace(value))
                {
                    adView.AdRequestParameters.Remove("size_x");
                }
                else
                {
                    adView.AdRequestParameters["size_x"] = value;
                }

                value = maxHeightTextBox.Text;
                if (string.IsNullOrWhiteSpace(value) == false)
                {
                    adView.AdRequestParameters.Remove("size_y");
                }
                else
                {
                    adView.AdRequestParameters["size_y"] = value;
                }

                value = minWidthTextBox.Text;
                if (string.IsNullOrWhiteSpace(value) == false)
                {
                    adView.AdRequestParameters.Remove("min_size_y");
                }
                else
                {
                    adView.AdRequestParameters["min_size_y"] = value;
                }

                value = minHeightTextBox.Text;
                if (string.IsNullOrWhiteSpace(value) == false)
                {
                    adView.AdRequestParameters.Remove("min_size_x");
                }
                else
                {
                    adView.AdRequestParameters["min_size_x"] = value;
                }
            }

            NavigationService.GoBack();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}