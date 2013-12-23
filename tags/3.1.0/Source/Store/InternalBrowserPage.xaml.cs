using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace com.moceanmobile.mast
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class InternalBrowserPage : Page
    {
        public InternalBrowserPage()
        {
            this.InitializeComponent();
        }

        async private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Assembly assembly = this.GetType().GetTypeInfo().Assembly;

            BitmapImage bitmapImage = new BitmapImage();
            System.IO.Stream resourceStream = assembly.GetManifestResourceStream(Defaults.IB_CLOSE_BUTTON_RESOURCE);
            Windows.Storage.Streams.InMemoryRandomAccessStream imras = new Windows.Storage.Streams.InMemoryRandomAccessStream();
            await Windows.Storage.Streams.RandomAccessStream.CopyAsync(resourceStream.AsInputStream(), imras);
            imras.Seek(0);
            await bitmapImage.SetSourceAsync(imras);
            this.closeImage.Source = bitmapImage;

            bitmapImage = new BitmapImage();
            resourceStream = assembly.GetManifestResourceStream(Defaults.IB_BACK_BUTTON_RESOURCE);
            imras = new Windows.Storage.Streams.InMemoryRandomAccessStream();
            await Windows.Storage.Streams.RandomAccessStream.CopyAsync(resourceStream.AsInputStream(), imras);
            imras.Seek(0);
            await bitmapImage.SetSourceAsync(imras);
            this.backImage.Source = bitmapImage;

            bitmapImage = new BitmapImage();
            resourceStream = assembly.GetManifestResourceStream(Defaults.IB_FORWARD_BUTTON_RESOURCE);
            imras = new Windows.Storage.Streams.InMemoryRandomAccessStream();
            await Windows.Storage.Streams.RandomAccessStream.CopyAsync(resourceStream.AsInputStream(), imras);
            imras.Seek(0);
            await bitmapImage.SetSourceAsync(imras);
            this.forwardImage.Source = bitmapImage;

            bitmapImage = new BitmapImage();
            resourceStream = assembly.GetManifestResourceStream(Defaults.IB_REFRESH_BUTTON_RESOURCE);
            imras = new Windows.Storage.Streams.InMemoryRandomAccessStream();
            await Windows.Storage.Streams.RandomAccessStream.CopyAsync(resourceStream.AsInputStream(), imras);
            imras.Seek(0);
            await bitmapImage.SetSourceAsync(imras);
            this.refreshImage.Source = bitmapImage;

            bitmapImage = new BitmapImage();
            resourceStream = assembly.GetManifestResourceStream(Defaults.IB_OPEN_BUTTON_RESOURCE);
            imras = new Windows.Storage.Streams.InMemoryRandomAccessStream();
            await Windows.Storage.Streams.RandomAccessStream.CopyAsync(resourceStream.AsInputStream(), imras);
            imras.Seek(0);
            await bitmapImage.SetSourceAsync(imras);
            this.openImage.Source = bitmapImage;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string url = e.Parameter as string;
            if (url != null)
            {
                this.webBrowserControl.Source = new Uri(url);
            }
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.webBrowserControl.GoBack();
            }
            catch (Exception)
            {

            }
        }

        private void forwardButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.webBrowserControl.GoForward();
            }
            catch (Exception)
            {

            }
        }

        private void refreshButton_Click(object sender, RoutedEventArgs e)
        {
            this.webBrowserControl.Navigate(this.webBrowserControl.Source);
        }

        async private void openButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.webBrowserControl.Source == null)
                return;

            await Launcher.LaunchUriAsync(this.webBrowserControl.Source);
        }
    }
}
