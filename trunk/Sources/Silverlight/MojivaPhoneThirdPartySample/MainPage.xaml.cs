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

namespace MojivaPhoneThirdPartySample
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
			adViewControl.Update();
		}
	}
}