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
