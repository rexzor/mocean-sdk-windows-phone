/****
 * © 2010-2011 mOcean Mobile. A subsidiary of Mojiva, Inc. All Rights Reserved.
 * */

using System.Windows;
using Microsoft.Phone.Controls;

namespace MojivaPhoneSample
{
	public partial class MainPage : PhoneApplicationPage
	{
		// Constructor
		public MainPage()
		{
			InitializeComponent();
		}

		private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
		{
			adViewControl.Zone = 20249;
			adViewControl.Site = 8061;
			adViewControl.Owner = this;
			adViewControl.InternalBrowser = true;
			adViewControl.UpdateTime = 10;
			adViewControl.AdvertiserId = "1111";
			adViewControl.GroupCode = "2222";
			adViewControl.Update();

			adInterstitialView.Zone = 16112;
			adInterstitialView.Site = 8061;
			adInterstitialView.Owner = this;
			adInterstitialView.InternalBrowser = true;
			adInterstitialView.UpdateTime = 10;
			adInterstitialView.AdvertiserId = "1111";
			adInterstitialView.GroupCode = "2222";
			adInterstitialView.CloseButtonBackgroundColor = System.Windows.Media.Colors.LightGray;
			adInterstitialView.CloseButtonText = "CLOSE";
			adInterstitialView.CloseButtonTextColor = System.Windows.Media.Colors.Black;
			adInterstitialView.CloseButtonSize = new Size(80, 60);
			adInterstitialView.CloseButtonPosition = 0;
			adInterstitialView.AutoCloseInterstitialTime = 15;
			adInterstitialView.ShowCloseButtonTime = 5;
			adInterstitialView.CloseButtonPosition = 1;
			adInterstitialView.MinSizeX = 320;
			adInterstitialView.MinSizeY = 460;
			adInterstitialView.Update();
		}
	}
}
