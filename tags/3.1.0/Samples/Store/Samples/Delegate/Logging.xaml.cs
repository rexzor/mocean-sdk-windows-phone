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
    public sealed partial class Logging : Page
    {
        public Logging()
        {
            this.InitializeComponent();
        }

        async private void adView_LoggingEvent(object sender, MASTAdView.LoggingEventEventArgs e)
        {
            // Since these events can come from a non-main/UI thread, dispatch properly.
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, delegate()
            {
                textBlock.Text += "LogLevel:" + e.LogLevel + " Entry:" + e.Entry + "\n\n";
            });
        }
    }
}
