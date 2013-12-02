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
    public partial class Image : PhoneApplicationPage
    {
        public Image()
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
                adView.RemoveContent();

                // Load the image resource to display when the adView fails to load a new ad.
                System.Windows.Media.Imaging.BitmapImage bitmapImage = new System.Windows.Media.Imaging.BitmapImage(new Uri("/ErrorImage.png", UriKind.Relative));

                // Use the adView's own containers to render the image.
                adView.ImageControl.Source = bitmapImage;
                adView.ImageBorder.Child = adView.ImageControl;

                // Add the border as a child to the ad.  This way when/if a new ad is rendered the adView will handle resetting everything properly.
                adView.Children.Add(adView.ImageBorder);
            });
        }
    }
}
