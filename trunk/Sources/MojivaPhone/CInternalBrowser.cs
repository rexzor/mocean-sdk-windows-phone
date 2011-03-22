/****
 * © 2010-2011 mOcean Mobile. A subsidiary of Mojiva, Inc. All Rights Reserved.
 * */

using System;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Microsoft.Phone.Controls;

namespace MojivaPhone
{
	public class CInternalBrowser
	{
		private const int DEFAULT_VIDEO_WIDTH = 480;
		private const int DEFAULT_VIDEO_HEIGHT = 800;
		private Popup popup = null;
		private WebBrowser webBrowser;
		private string navigateUrl = String.Empty;

		private event EventHandler<StringEventArgs> ClosedEvent;

		public event EventHandler<StringEventArgs> Closed
		{
			add { ClosedEvent += value; }
			remove { ClosedEvent -= value; }
		}

		public bool IsShow
		{
			get { return popup.IsOpen; }
		}

		public CInternalBrowser()
		{
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

		private void Init()
		{
			popup = new System.Windows.Controls.Primitives.Popup();
			popup.Width = DEFAULT_VIDEO_WIDTH;
			popup.Height = DEFAULT_VIDEO_HEIGHT;
			popup.HorizontalOffset = 0;
			popup.VerticalOffset = 0;

			webBrowser = new WebBrowser();
			webBrowser.Width = DEFAULT_VIDEO_WIDTH;
			webBrowser.Height = DEFAULT_VIDEO_HEIGHT;

			Grid g = new Grid();
			g.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 0, 0));
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
				ClosedEvent(this, new StringEventArgs(navigateUrl));
			}
		}

		private void WebBrowser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
		{
			navigateUrl = e.Uri.ToString();
		}
	}
}
