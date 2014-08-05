/*
 * PubMatic Inc. (“PubMatic”) CONFIDENTIAL
 * Unpublished Copyright (c) 2006-2014 PubMatic, All Rights Reserved.
 *
 * NOTICE:  All information contained herein is, and remains the property of PubMatic. The intellectual and technical concepts contained
 * herein are proprietary to PubMatic and may be covered by U.S. and Foreign Patents, patents in process, and are protected by trade secret or copyright law.
 * Dissemination of this information or reproduction of this material is strictly forbidden unless prior written permission is obtained
 * from PubMatic.  Access to the source code contained herein is hereby forbidden to anyone except current PubMatic employees, managers or contractors who have executed 
 * Confidentiality and Non-disclosure agreements explicitly covering such access.
 *
 * The copyright notice above does not evidence any actual or intended publication or disclosure  of  this source code, which includes  
 * information that is confidential and/or proprietary, and is a trade secret, of  PubMatic.   ANY REPRODUCTION, MODIFICATION, DISTRIBUTION, PUBLIC  PERFORMANCE, 
 * OR PUBLIC DISPLAY OF OR THROUGH USE  OF THIS  SOURCE CODE  WITHOUT  THE EXPRESS WRITTEN CONSENT OF PubMatic IS STRICTLY PROHIBITED, AND IN VIOLATION OF APPLICABLE 
 * LAWS AND INTERNATIONAL TREATIES.  THE RECEIPT OR POSSESSION OF  THIS SOURCE CODE AND/OR RELATED INFORMATION DOES NOT CONVEY OR IMPLY ANY RIGHTS  
 * TO REPRODUCE, DISCLOSE OR DISTRIBUTE ITS CONTENTS, OR TO MANUFACTURE, USE, OR SELL ANYTHING THAT IT  MAY DESCRIBE, IN WHOLE OR IN PART.                
 */


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
            adView.InternalBrowserOpened += adView_InternalBrowserOpened;
            adView.InternalBrowserClosed += adView_InternalBrowserClosed;
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
