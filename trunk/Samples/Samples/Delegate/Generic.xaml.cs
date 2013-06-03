using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace com.moceanmobile.mast.samples.Delegate
{
    public partial class Generic : PhoneApplicationPage
    {
        public Generic()
        {
            InitializeComponent();

            // These can be done in the designer or here:
            adView.AdReceived += adView_AdReceived;
            adView.AdFailed += adView_AdFailed;
            adView.OpeningURL += adView_OpeningURL;
            adView.CloseButtonPressed += adView_CloseButtonPressed;
            adView.AdExpanded += adView_AdExpanded;
            adView.AdResized += adView_AdResized;
            adView.AdCollapsed += adView_AdCollapsed;
            adView.LeavingApplication += adView_LeavingApplication;
            adView.LoggingEvent += adView_LoggingEvent;
            adView.ReceivedThirdPartyRequest += adView_ReceivedThirdPartyRequest;
            adView.PlayingVideo += adView_PlayingVideo;
            adView.SavingPhoto += adView_SavingPhoto;
            adView.ProcessedRichmediaRequest += adView_ProcessedRichmediaRequest;
        }

        private void addEntry(string entry)
        {
            // Since events can come from a non-main/UI thread, dispatch properly.
            System.Windows.Deployment.Current.Dispatcher.BeginInvoke(delegate()
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
