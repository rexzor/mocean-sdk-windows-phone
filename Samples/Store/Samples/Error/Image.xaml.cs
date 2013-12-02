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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace com.moceanmobile.mast.samples.Error
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Image : Page
    {
        public Image()
        {
            this.InitializeComponent();
        }

        async private void adView_AdFailed(object sender, MASTAdView.AdFailedEventArgs e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, delegate()
            {
                // Remove any existing ad content (last rendered ad).
                adView.RemoveContent();

                // Load the image resource to display when the adView fails to load a new ad.
                BitmapImage imageSource = new BitmapImage();
                imageSource.UriSource = new Uri("ms-appx:///Assets/ErrorImage.png", UriKind.Absolute);

                // Use the adView's own containers to render the image.
                adView.ImageControl.Source = imageSource;
                adView.ImageBorder.Child = adView.ImageControl;

                // Add the border as a child to the ad.  This way when/if a new ad is rendered the adView will handle resetting everything properly.
                adView.Children.Add(adView.ImageBorder);
            });
        }

        private void adView_AdReceived(object sender, EventArgs e)
        {
            
        }
    }
}
