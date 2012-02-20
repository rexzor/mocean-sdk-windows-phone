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
	public partial class SamplesPage : PhoneApplicationPage
	{
		public SamplesPage()
		{
			InitializeComponent();
		}

		private void PhoneApplicationPage_OrientationChanged(object sender, OrientationChangedEventArgs e)
		{
			switch (e.Orientation)
			{
				case PageOrientation.Landscape:
				case PageOrientation.LandscapeLeft:
				case PageOrientation.LandscapeRight:
					adViewSimple.Width = 468;
					adViewSimple.Height = 60;
					break;
				case PageOrientation.Portrait:
				case PageOrientation.PortraitUp:
				case PageOrientation.PortraitDown:
				default:
					adViewSimple.Width = 320;
					adViewSimple.Height = 50;
					break;
			}

			adViewSimple.Update();
		}

		private void adViewSimpleUpdateButton_Click(object sender, RoutedEventArgs e)
		{
			adViewSimple.Update();
		}

		private void updateButton_Click(object sender, RoutedEventArgs e)
		{
			callBacks.Text = String.Empty;
			adViewCallback.Update();
		}

		private void AddCallBackLog(string message)
		{
			Deployment.Current.Dispatcher.BeginInvoke(() =>
			{
				callBacks.Text += message;
			});
		}

		private void adViewCallback_AdDownloadBegin(object sender, EventArgs e)
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
		void adViewCallback_AdWebViewClosing(object sender, mOceanWindowsPhone.MASTAdView.WebViewClosingEventArgs e)
		{
			AddCallBackLog("AdWebViewClosing\n");
		}
	}
}