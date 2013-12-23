using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Reflection;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace com.moceanmobile.mast.samples
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SamplePage : Page
    {
        public SamplePage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            Type sampleType = e.Parameter as Type;
            sampleFrame.Navigate(sampleType);
        }

        private void sampleFrame_Navigated(object sender, NavigationEventArgs e)
        {
            Page samplePage = sampleFrame.Content as Page;
            MASTAdView adViewBottom = samplePage.FindName("adViewBottom") as MASTAdView;

            if ("CustomAdSetup".Equals(samplePage.Tag))
            {
                this.BottomAppBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
                this.CustomConfigurePanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            else if (adViewBottom != null)
            {
                this.BottomAppBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
                this.BottomAdViewRefreshPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
        }

        private void TopAppBar_Opened(object sender, object e)
        {
            Page samplePage = sampleFrame.Content as Page;
            MASTAdView adView = samplePage.FindName("adView") as MASTAdView;

            if (adView != null)
            {
                ZoneText.Text = adView.Zone.ToString();
            }
            else
            {
                MethodInfo refreshMethod = samplePage.GetType().GetRuntimeMethod("GetZone", new Type[] { });
                if (refreshMethod != null)
                {
                    int zone = (int)refreshMethod.Invoke(samplePage, null);
                    ZoneText.Text = zone.ToString();
                }
            }
        }

        private void BottomAppBar_Opened(object sender, object e)
        {
            Page samplePage = sampleFrame.Content as Page;
            MASTAdView adView = samplePage.FindName("adViewBottom") as MASTAdView;

            if (adView != null)
            {
                BottomZoneText.Text = adView.Zone.ToString();
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            int zone = int.Parse(ZoneText.Text);

            Page samplePage = sampleFrame.Content as Page;
            MASTAdView adView = samplePage.FindName("adView") as MASTAdView;

            if (adView != null)
            {
                adView.Zone = zone;
                adView.Update();
            }
            else
            {
                MethodInfo refreshMethod = samplePage.GetType().GetRuntimeMethod("Refresh", new Type[] {typeof(int)});
                if (refreshMethod != null)
                {
                    refreshMethod.Invoke(samplePage, new object[] { zone });
                }
            }

            this.TopAppBar.IsOpen = false;
            this.BottomAppBar.IsOpen = false;
        }

        private void BottomRefreshButton_Click(object sender, RoutedEventArgs e)
        {
            int zone = int.Parse(BottomZoneText.Text);

            Page samplePage = sampleFrame.Content as Page;
            MASTAdView adView = samplePage.FindName("adViewBottom") as MASTAdView;

            if (adView != null)
            {
                adView.Zone = zone;
                adView.Update();
            }

            this.TopAppBar.IsOpen = false;
            this.BottomAppBar.IsOpen = false;
        }

        private void CustomConfigureButton_Click(object sender, RoutedEventArgs e)
        {
            this.TopAppBar.IsOpen = false;
            this.BottomAppBar.IsOpen = false;

            CustomPopup.IsOpen = true;
        }

        private void CustomPopup_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Popup popup = sender as Popup;
            FrameworkElement child = popup.Child as FrameworkElement;
            child.Width = e.NewSize.Width;
            child.Height = e.NewSize.Height;
        }

        private void CustomPopup_Opened(object sender, object e)
        {
            Page samplePage = sampleFrame.Content as Page;
            MASTAdView adView = samplePage.FindName("adView") as MASTAdView;

            UseInternalBrowserCheckBox.IsChecked = adView.UseInteralBrowser;
            UseLocationDetectionCheckBox.IsChecked = adView.LocationDetectionEnabled;

            xTextBox.Text = adView.Margin.Left.ToString();
            yTextBox.Text = adView.Margin.Right.ToString();

            if (adView.HorizontalAlignment != Windows.UI.Xaml.HorizontalAlignment.Stretch)
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

        private void CustomCancelButton_Click(object sender, RoutedEventArgs e)
        {
            CustomPopup.IsOpen = false;
        }

        private void CustomDoneButton_Click(object sender, RoutedEventArgs e)
        {
            Page samplePage = sampleFrame.Content as Page;
            MASTAdView adView = samplePage.FindName("adView") as MASTAdView;

            if (UseInternalBrowserCheckBox.IsChecked.HasValue)
                adView.UseInteralBrowser = (bool)UseInternalBrowserCheckBox.IsChecked;

            if (UseLocationDetectionCheckBox.IsChecked.HasValue)
                  adView.LocationDetectionEnabled = (bool)UseLocationDetectionCheckBox.IsChecked;

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
                adView.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch;
                adView.Width = Double.NaN;
            }
            else
            {
                adView.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center;
                adView.Width = int.Parse(value);
            }

            value = heightTextBox.Text;
            if (string.IsNullOrWhiteSpace(value))
            {
                adView.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Top;
                adView.Height = 100;
            }
            else
            {
                adView.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Top;
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
            if (string.IsNullOrWhiteSpace(value))
            {
                adView.AdRequestParameters.Remove("size_y");
            }
            else
            {
                adView.AdRequestParameters["size_y"] = value;
            }

            value = minWidthTextBox.Text;
            if (string.IsNullOrWhiteSpace(value))
            {
                adView.AdRequestParameters.Remove("min_size_y");
            }
            else
            {
                adView.AdRequestParameters["min_size_y"] = value;
            }

            value = minHeightTextBox.Text;
            if (string.IsNullOrWhiteSpace(value))
            {
                adView.AdRequestParameters.Remove("min_size_x");
            }
            else
            {
                adView.AdRequestParameters["min_size_x"] = value;
            }

            CustomPopup.IsOpen = false;

            System.Threading.Tasks.Task.Run(async delegate()
            {
                await System.Threading.Tasks.Task.Delay(500);
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, delegate()
                {
                    adView.Update();
                });
            });
        }
    }
}
