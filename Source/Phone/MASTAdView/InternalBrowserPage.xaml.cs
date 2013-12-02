using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace com.moceanmobile.mast
{
    public partial class InternalBrowserPage : PhoneApplicationPage
    {
        public InternalBrowserPage()
        {
            InitializeComponent();

            System.IO.Stream resourceStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(Defaults.IB_BACK_BUTTON_RESOURCE);
            System.Windows.Media.Imaging.BitmapImage bitmapImage = new System.Windows.Media.Imaging.BitmapImage();
            bitmapImage.SetSource(resourceStream);
            backImage.Source = bitmapImage;

            resourceStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(Defaults.IB_FORWARD_BUTTON_RESOURCE);
            bitmapImage = new System.Windows.Media.Imaging.BitmapImage();
            bitmapImage.SetSource(resourceStream);
            forwardImage.Source = bitmapImage;

            resourceStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(Defaults.IB_REFRESH_BUTTON_RESOURCE);
            bitmapImage = new System.Windows.Media.Imaging.BitmapImage();
            bitmapImage.SetSource(resourceStream);
            refreshImage.Source = bitmapImage;

            resourceStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(Defaults.IB_OPEN_BUTTON_RESOURCE);
            bitmapImage = new System.Windows.Media.Imaging.BitmapImage();
            bitmapImage.SetSource(resourceStream);
            openImage.Source = bitmapImage;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string url = null;
            if (this.NavigationContext.QueryString.ContainsKey("url"))
            {
                url = Uri.UnescapeDataString(NavigationContext.QueryString["url"]);
            }

            if (url != null)
            {
                this.webBrowserControl.Source = new Uri(url);
            }
        }

        private void backButton_Click(object sender, EventArgs e)
        {
            try
            {
                this.webBrowserControl.InvokeScript("eval", "history.go(-1)");
            }
            catch (Exception)
            {

            }
        }

        private void forwardButton_Click(object sender, EventArgs e)
        {
            try
            {
                this.webBrowserControl.InvokeScript("eval", "history.go(1)");
            }
            catch (Exception)
            {

            }
        }

        private void refreshButton_Click(object sender, EventArgs e)
        {
            this.webBrowserControl.Navigate(this.webBrowserControl.Source);
        }

        private void openButton_Click(object sender, EventArgs e)
        {
            if (this.webBrowserControl.Source == null)
                return;

            Microsoft.Phone.Tasks.WebBrowserTask task = new Microsoft.Phone.Tasks.WebBrowserTask();
            task.Uri = this.webBrowserControl.Source;
            task.Show();
        }
    }
}
