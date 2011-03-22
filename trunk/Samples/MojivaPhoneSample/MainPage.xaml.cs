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
			adViewControl.Zone = 17324;
			adViewControl.Site = 8061;
			adViewControl.Owner = this;
			adViewControl.InternalBrowser = true;
			adViewControl.UpdateTime = 10;
			adViewControl.AdvertiserId = "1111";
			adViewControl.GroupCode = "2222";
			adViewControl.Run();
		}

		private void btnOrmmaHtml_Click(object sender, RoutedEventArgs e)
		{
			adViewControl.Zone = 17490;
			adViewControl.Site = 8061;
			adViewControl.Update();

			textBlock2.Text = "ORMMA HTML banner";
		}

		private void btnOrmma1_Click(object sender, RoutedEventArgs e)
		{
			adViewControl.Zone = 17487;
			adViewControl.Site = 8061;
			adViewControl.Update();

			textBlock2.Text = "ORMMA level 1";
		}

		private void btnOrmma2_Click(object sender, RoutedEventArgs e)
		{
			adViewControl.Zone = 17488;
			adViewControl.Site = 8061;
			adViewControl.Update();

			textBlock2.Text = "ORMMA level 2";
		}

		private void btnOrmma3_Click(object sender, RoutedEventArgs e)
		{
			adViewControl.Zone = 17489;
			adViewControl.Site = 8061;
			adViewControl.Update();

			textBlock2.Text = "ORMMA level 3";
		}
	}
}
