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
    public sealed partial class ThirdPartyRequest : Page
    {
        public ThirdPartyRequest()
        {
            this.InitializeComponent();
        }

        async private void adView_ReceivedThirdPartyRequest(object sender, MASTAdView.ThirdPartyRequestEventArgs e)
        {
            // Since these events can come from a non-main/UI thread, dispatch properly.
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, delegate()
            {
                textBlock.Text += "Properties:" + App.FormattedString(e.Properties) +
                    " Parameters:" + App.FormattedString(e.Parameters) + "\n\n";
            });
        }
    }
}
