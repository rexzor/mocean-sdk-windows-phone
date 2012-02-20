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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

namespace mOceanSample
{
	public partial class MainPage : PhoneApplicationPage
	{
		public MainPage()
		{
			InitializeComponent();
		}

		private void standartSamplesButton_Click(object sender, RoutedEventArgs e)
		{
			NavigationService.Navigate(new Uri("/SamplesPage.xaml", UriKind.Relative));
		}

		private void customSampleButton_Click(object sender, RoutedEventArgs e)
		{
			NavigationService.Navigate(new Uri("/CustomMASTAdViewPage.xaml", UriKind.Relative));
		}
	}
}