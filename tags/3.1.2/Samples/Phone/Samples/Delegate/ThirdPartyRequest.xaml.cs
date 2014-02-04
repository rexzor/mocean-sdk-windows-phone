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
    public partial class ThirdPartyRequest : PhoneApplicationPage
    {
        public ThirdPartyRequest()
        {
            InitializeComponent();
        }

        private void adView_ReceivedThirdPartyRequest(object sender, mast.MASTAdView.ThirdPartyRequestEventArgs e)
        {
            // Since these events can come from a non-main/UI thread, dispatch properly.
            System.Windows.Deployment.Current.Dispatcher.BeginInvoke(delegate()
            {
                textBlock.Text += "Properties:" + App.FormattedString(e.Properties) +
                    " Parameters:" + App.FormattedString(e.Parameters) + "\n\n";
            });
        }
    }
}