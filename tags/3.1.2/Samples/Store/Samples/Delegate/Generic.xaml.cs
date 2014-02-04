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

namespace com.moceanmobile.mast.samples.Delegate
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Generic : Page
    {
        public Generic()
        {
            this.InitializeComponent();

            // These can be done in the designer or here:
            adView.AdReceived += adView_AdReceived;
            adView.AdFailed += adView_AdFailed;
            adView.OpeningURL += adView_OpeningURL;
            adView.CloseButtonPressed += adView_CloseButtonPressed;
            adView.AdExpanded += adView_AdExpanded;
            adView.AdResized += adView_AdResized;
            adView.AdCollapsed += adView_AdCollapsed;
            adView.LeavingApplication += adView_LeavingApplication;
            adView.InternalBrowserOpened += adView_InternalBrowserOpened;
            adView.InternalBrowserClosed += adView_InternalBrowserClosed;
            adView.LoggingEvent += adView_LoggingEvent;
            adView.ReceivedThirdPartyRequest += adView_ReceivedThirdPartyRequest;
            adView.PlayingVideo += adView_PlayingVideo;
            adView.SavingPhoto += adView_SavingPhoto;
            adView.ProcessedRichmediaRequest += adView_ProcessedRichmediaRequest;
        }

        async private void addEntry(string entry)
        {
            // Since events can come from a non-main/UI thread, dispatch properly.
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, delegate()
            {
                textBlock.Text += entry + "\n\n";
            });
        }

        private void adView_AdReceived(object sender, EventArgs e)
        {
            string entry = "adView_AdReceived";
            addEntry(entry);
        }

        private void adView_AdFailed(object sender, mast.MASTAdView.AdFailedEventArgs e)
        {
            string entry = "adView_AdFailed Exception:" + e.Exception;
            addEntry(entry);
        }

        private void adView_OpeningURL(object sender, MASTAdView.OpeningURLEventArgs e)
        {
            string entry = "adView_OpeningURL URL:" + e.URL;
            addEntry(entry);
        }

        private void adView_CloseButtonPressed(object sender, EventArgs e)
        {
            string entry = "adView_CloseButtonPressed";
            addEntry(entry);
        }

        private void adView_AdExpanded(object sender, EventArgs e)
        {
            string entry = "adView_AdExpanded";
            addEntry(entry);
        }

        private void adView_AdResized(object sender, EventArgs e)
        {
            string entry = "adView_AdResized";
            addEntry(entry);
        }

        private void adView_AdCollapsed(object sender, EventArgs e)
        {
            string entry = "adView_AdCollapsed";
            addEntry(entry);
        }

        private void adView_LeavingApplication(object sender, EventArgs e)
        {
            string entry = "adView_LeavingApplication";
            addEntry(entry);
        }

        void adView_InternalBrowserClosed(object sender, EventArgs e)
        {
            string entry = "adView_InternalBrowserClosed";
            addEntry(entry);
        }

        void adView_InternalBrowserOpened(object sender, EventArgs e)
        {
            string entry = "adView_InternalBrowserOpened";
            addEntry(entry);
        }

        private void adView_LoggingEvent(object sender, MASTAdView.LoggingEventEventArgs e)
        {
            string entry = "adView_LoggingEvent LogLevel:" + e.LogLevel + " Entry:" + e.Entry;
            addEntry(entry);
        }

        private void adView_ReceivedThirdPartyRequest(object sender, MASTAdView.ThirdPartyRequestEventArgs e)
        {
            string entry = "adView_ReceivedThirdPartyRequest Properties:" + App.FormattedString(e.Properties) +
                    " Parameters:" + App.FormattedString(e.Parameters);
            addEntry(entry);
        }

        private void adView_PlayingVideo(object sender, MASTAdView.PlayingVideoEventArgs e)
        {
            string entry = "adView_PlayingVideo URL:" + e.URL;
            addEntry(entry);
        }

        private void adView_SavingPhoto(object sender, MASTAdView.SavingPhotoEventArgs e)
        {
            string entry = "adView_SavingPhoto Photo:" + e.URL;
            addEntry(entry);
        }

        private void adView_ProcessedRichmediaRequest(object sender, MASTAdView.ProcessedRichmediaRequestEventArgs e)
        {
            string entry = "adView_ProcessedRichmediaRequest URI:" + e.URI + " Handled:" + e.Handled;
            addEntry(entry);
        }
    }
}
