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
	public partial class CustomMASTAdViewPage : PhoneApplicationPage
	{
		public CustomMASTAdViewPage()
		{
			InitializeComponent();

			adViewCustom.Site = 8061;
			adViewCustom.Zone = 20249;
		}

		private void adViewCustomUpdateButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				adViewCustom.Site = Int32.Parse(adViewCustomSiteTextBox.Text);
				adViewCustom.Zone = Int32.Parse(adViewCustomZoneTextBox.Text);
			}
			catch (Exception)
			{ }

			adViewCustom.Update();
		}
	}
}