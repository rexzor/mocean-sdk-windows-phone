/****
 * © 2010-2011 mOcean Mobile. A subsidiary of Mojiva, Inc. All Rights Reserved.
 * */

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
using MojivaPhone;

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
			adViewControl.ZoneId = "20249";
			adViewControl.SiteId = "8061";
			adViewControl.Owner = this;

#if DEBUG
            adViewControl.InternalBrowser = false;
#endif
			adViewControl.InternalBrowser = true;
            adViewControl.UpdateTime = 30;
			adViewControl.AdvertiserId = "1111";
			adViewControl.GroupCode = "2222";
			adViewControl.Run();
			
/*
			adInterstitial.ZoneId = "16112";
			adInterstitial.SiteId = "8061";
            adInterstitial.Owner = this;
            //adInterstitial.UseCache = true;

#if DEBUG
            //adInterstitial.TestModeEnabled = true;
			adInterstitial.InternalBrowser = false;
#endif
			adInterstitial.UpdateTime = 120;
			adInterstitial.MinSizeX = 320;
			adInterstitial.MinSizeY = 460;
			adInterstitial.MaxSizeX = 320;
			adInterstitial.MaxSizeY = 460;

			adInterstitial.AutoCloseInterstitialTime = 10;
			adInterstitial.ShowCloseButtonTime = 2;
			adInterstitial.CloseButtonPosition = AdInterstitialView.AdInterstitialCloseButtonPosition.AdInterstitialCloseButtonPositionTop | AdInterstitialView.AdInterstitialCloseButtonPosition.AdInterstitialCloseButtonPositionRight;
			adInterstitial.IsShowPhoneStatusBar = true;
			adInterstitial.CloseButtonTransparency = 230;
			adInterstitial.CloseButtonTextColor = Colors.Red;
			adInterstitial.CloseButtonBackgroundColor = Colors.Cyan;
			adInterstitial.CloseButtonSize = new Size(180, 180);
			//adInterstitial.CloseButtonImage = "http://ru.www.mozilla.com/img/covehead/firefox/survey/thanks-background.png";
			adInterstitial.CloseButtonImage = "SplashScreenImage.jpg";
			adInterstitial.CloseButtonSelectedImage = "image";
			adInterstitial.CloseButtonText = "close";
            adInterstitial.Run();
//*/
        }

		private void btnOrmmaHtml_Click(object sender, RoutedEventArgs e)
		{
			adViewControl.ZoneId = "17490";
			adViewControl.SiteId = "8061";
			adViewControl.Update();

			textBlock2.Text = "ORMMA HTML banner";
		}

		private void btnOrmma1_Click(object sender, RoutedEventArgs e)
		{
			adViewControl.ZoneId = "17487";
			adViewControl.SiteId = "8061";
			adViewControl.Update();

			textBlock2.Text = "ORMMA level 1";
		}

		private void btnOrmma2_Click(object sender, RoutedEventArgs e)
		{
			adViewControl.ZoneId = "17488";
			adViewControl.SiteId = "8061";
			adViewControl.Update();

			textBlock2.Text = "ORMMA level 2";
		}

		private void btnOrmma3_Click(object sender, RoutedEventArgs e)
		{
			adViewControl.ZoneId = "17489";
			adViewControl.SiteId = "8061";
			adViewControl.Update();

			textBlock2.Text = "ORMMA level 3";
		}
    }
}
