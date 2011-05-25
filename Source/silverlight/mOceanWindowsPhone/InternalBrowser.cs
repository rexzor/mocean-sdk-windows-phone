/****
 * © 2010-2011 mOcean Mobile. A subsidiary of Mojiva, Inc. All Rights Reserved.
 * */

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Microsoft.Phone.Controls;

namespace mOceanWindowsPhone
{
	internal class InternalBrowser
	{
		private Popup popup = null;
		private WebBrowser webBrowser;
		private readonly double width = 0;
		private readonly double height = 0;
		private const double SYSTEM_TRAY_WIDTH = 72;
		private string navigateUrl = String.Empty;

		public delegate void ClosedEventHandler(string url);
		private event ClosedEventHandler ClosedEvent;
		public event ClosedEventHandler Closed
		{
			add { ClosedEvent += value; }
			remove { ClosedEvent -= value; }
		}

		public bool IsOpen
		{
			get { return popup.IsOpen; }
		}

		public InternalBrowser()
		{
			width = 480;
			height = 800;

			Init();
		}

		public void Show(string url)
		{
			navigateUrl = url;
			popup.IsOpen = true;
		}

		public void Close()
		{
			popup.IsOpen = false;
		}

		public void ChangeOrientation(PageOrientation pageOrientation)
		{
			TransformGroup transformGroup = new TransformGroup();
			RotateTransform rotateTransform = new RotateTransform();
			TranslateTransform translateTransform = new TranslateTransform();

			if (pageOrientation == PageOrientation.Landscape || pageOrientation == PageOrientation.LandscapeLeft || pageOrientation == PageOrientation.LandscapeRight)
			{
				if (pageOrientation == PageOrientation.LandscapeLeft)
				{
					rotateTransform.Angle = 90;
					translateTransform.X = width;

					if (Microsoft.Phone.Shell.SystemTray.IsVisible)
					{
						translateTransform.Y += SYSTEM_TRAY_WIDTH;
					}
				}
				else if (pageOrientation == PageOrientation.LandscapeRight)
				{
					rotateTransform.Angle = -90;
					translateTransform.Y = height;
				}

				transformGroup.Children.Add(rotateTransform);
				transformGroup.Children.Add(translateTransform);
				webBrowser.RenderTransform = transformGroup;

				webBrowser.Width = height - SYSTEM_TRAY_WIDTH;
				webBrowser.Height = width;
			}
			else if (pageOrientation == PageOrientation.Portrait || pageOrientation == PageOrientation.PortraitDown || pageOrientation == PageOrientation.PortraitUp)
			{
				transformGroup.Children.Add(rotateTransform);
				transformGroup.Children.Add(translateTransform);
				webBrowser.RenderTransform = transformGroup;

				webBrowser.Width = width;
				webBrowser.Height = height;
			}
		}

		private void Init()
		{
			popup = new Popup();
			popup.Width = width;
			popup.Height = height;
			popup.HorizontalOffset = 0;
			popup.VerticalOffset = 0;

			webBrowser = new WebBrowser();
			webBrowser.Width = width;
			webBrowser.Height = height;
			webBrowser.HorizontalAlignment = HorizontalAlignment.Left;
			webBrowser.VerticalAlignment = VerticalAlignment.Top;

			Grid g = new Grid();
			g.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
			g.Children.Add(webBrowser);
			popup.Child = g;

			popup.Opened += new EventHandler(Popup_Opened);
			popup.Closed += new EventHandler(Popup_Closed);
			webBrowser.Navigated += new EventHandler<System.Windows.Navigation.NavigationEventArgs>(WebBrowser_Navigated);
		}

		private void Popup_Opened(object sender, EventArgs e)
		{
			webBrowser.Navigate(new Uri(navigateUrl, UriKind.RelativeOrAbsolute));
		}

		private void Popup_Closed(object sender, EventArgs e)
		{
			if (ClosedEvent != null)
			{
				ClosedEvent(navigateUrl);
			}
		}

		private void WebBrowser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
		{
			try
			{
				navigateUrl = e.Uri.ToString();
				System.Diagnostics.Debug.WriteLine(navigateUrl);
			}
			catch (System.Exception)
			{}
		}
	}
}
