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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace com.moceanmobile.mast.samples.Error
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Hide : Page
    {
        public Hide()
        {
            this.InitializeComponent();
        }

        async private void adView_AdFailed(object sender, MASTAdView.AdFailedEventArgs e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, delegate()
            {
                adView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            });
        }

        async private void adView_AdReceived(object sender, EventArgs e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, delegate()
            {
                adView.Visibility = Windows.UI.Xaml.Visibility.Visible;
            });
        }
    }
}
