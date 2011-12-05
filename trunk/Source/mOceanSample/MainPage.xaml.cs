using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

namespace mOceanSample
{
	public partial class PanoramaPage : PhoneApplicationPage
	{
		public PanoramaPage()
		{
			InitializeComponent();

			adViewCallback.AdDownloadBegin += new EventHandler(adViewCallback_AdDownloadBegin);
			adViewCallback.AdDownloadEnd += new EventHandler(adViewCallback_AdDownloadEnd);
			adViewCallback.AdWebViewClosing += new EventHandler<mOceanWindowsPhone.AdView.WebViewClosingEventArgs>(adViewCallback_AdWebViewClosing);
			adViewCallback.AdNavigateBanner += new EventHandler<NavigatingEventArgs>(adViewCallback_AdNavigateBanner);
		}

		private void PhoneApplicationPage_OrientationChanged(object sender, OrientationChangedEventArgs e)
		{
			switch (e.Orientation)
			{
				case PageOrientation.Landscape:
				case PageOrientation.LandscapeLeft:
				case PageOrientation.LandscapeRight:
					adViewOrientation.Zone = 16741;
					break;
				case PageOrientation.Portrait:
				case PageOrientation.PortraitUp:
				case PageOrientation.PortraitDown:
				default:
					adViewOrientation.Zone = 20249;
					break;
			}
			
			adViewOrientation.Update();
		}

		private void updateButton_Click(object sender, RoutedEventArgs e)
		{
			callBacks.Text = String.Empty;
			adViewCallback.Update();
		}

		void AddCallBackLog(string message)
		{
			Deployment.Current.Dispatcher.BeginInvoke(() =>
			{
				callBacks.Text += message;
			});
		}

		void adViewCallback_AdDownloadBegin(object sender, EventArgs e)
		{
			AddCallBackLog("AdDownloadBegin\n");
		}
		void adViewCallback_AdDownloadEnd(object sender, EventArgs e)
		{
			AddCallBackLog("AdDownloadEnd\n");
		}
		void adViewCallback_AdNavigateBanner(object sender, NavigatingEventArgs e)
		{
			AddCallBackLog("AdNavigateBanner\n");
		}
		void adViewCallback_AdWebViewClosing(object sender, mOceanWindowsPhone.AdView.WebViewClosingEventArgs e)
		{
			AddCallBackLog("AdWebViewClosing\n");
		}
	}
}