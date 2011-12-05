/****
 * © 2010-2011 mOcean Mobile. A subsidiary of Mojiva, Inc. All Rights Reserved.
 * */

using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Microsoft.Phone.Controls;

namespace mOceanWindowsPhone
{
	public class AdInterstitialView : Panel
	{
		internal const int systemTrayHeight = 32;
		internal const int initWidth = 800;
		internal const int initHeight = 480;
		private Size screenSize = new Size(initWidth, initHeight);

		private Popup popup = new Popup();
		private Grid container = new Grid();
		private AdView adView = null;
		private Button closeButton = null;
		private Image imageClose = new Image();
		private PhoneApplicationFrame ownerFrame = null;
		private PhoneApplicationPage ownerPage = null;
		private PageOrientation pageOrientation = PageOrientation.PortraitUp;
		private bool appBarVisible = false;
		private bool systemTrayVisible = false;
		
		private Timer closeButtonTimer = null;
		private Timer adTimer = null;

		public AdInterstitialView()
		{
			this.Loaded += new RoutedEventHandler(AdInterstitialView_Loaded);
			adView = new AdView();
			adView.PlacementType = "interstitial";

			popup.Width = initWidth;
			popup.Height = initHeight;
			popup.HorizontalAlignment = HorizontalAlignment.Left;
			popup.VerticalAlignment = VerticalAlignment.Top;
			popup.HorizontalOffset = 0;
			popup.VerticalOffset = 0;
			popup.Loaded += new RoutedEventHandler(popup_Loaded);

			popup.Child = container;
			this.Children.Add(popup);
			popup.IsOpen = true;

			container.Width = initWidth;
			container.Height = initHeight;
			container.HorizontalAlignment = HorizontalAlignment.Left;
			container.VerticalAlignment = VerticalAlignment.Top;
			container.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

			adView.HorizontalAlignment = HorizontalAlignment.Stretch;
			adView.VerticalAlignment = VerticalAlignment.Stretch;
			container.Children.Add(adView);

			imageClose.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("customclose.png", UriKind.Relative));
			imageClose.Width = AdView.CUSTOM_CLOSE_BUTTON_SIZE;
			imageClose.Height = AdView.CUSTOM_CLOSE_BUTTON_SIZE;
			imageClose.HorizontalAlignment = HorizontalAlignment.Right;
			imageClose.VerticalAlignment = VerticalAlignment.Top;
			imageClose.Stretch = Stretch.Fill;
			imageClose.Tap += new EventHandler<System.Windows.Input.GestureEventArgs>(imageClose_Tap);
			imageClose.Visibility = Visibility.Collapsed;

			container.Children.Add(imageClose);

			adTimer = new Timer(new TimerCallback(ProcessAdTimer), this, UInt32.MaxValue, UInt32.MaxValue);
			closeButtonTimer = new Timer(new TimerCallback(ProcessCloseButtonTimer), this, UInt32.MaxValue, UInt32.MaxValue);

			ShowCloseButtonTime = 0;
			AutoCloseInterstitialTime = UInt32.MaxValue;
		}

		public AdInterstitialView(int site, int zone) : this()
		{
			Site = site;
			Zone = zone;
		}

		internal AdInterstitialView(string url) : this()
		{
			adView.UrlToLoad = url;
		}

		#region "Setting parameters"
		public int Site
		{
			get { return adView.Site; }
			set { adView.Site = value; }
		}
		public int Zone
		{
			get { return adView.Zone; }
			set { adView.Zone = value; }
		}
		public bool Test
		{
			get { return adView.Test; }
			set { adView.Test = value; }
		}
		public int Premium
		{
			get { return adView.Premium; }
			set { adView.Premium = value; }
		}
		public string Keywords
		{
			get { return adView.Keywords; }
			set { adView.Keywords = value; }
		}
		public int AdsType
		{
			get { return adView.AdsType; }
			set { adView.AdsType = value; }
		}
		public int Type
		{
			get { return adView.Type; }
			set { adView.Type = value; }
		}
		public int MinSizeX
		{
			get { return adView.MinSizeX; }
			set { adView.MinSizeX = value; }
		}
		public int MinSizeY
		{
			get { return adView.MinSizeY; }
			set { adView.MinSizeY = value; }
		}
		public int MaxSizeX
		{
			get { return adView.MaxSizeX; }
			set { adView.MaxSizeX = value; }
		}
		public int MaxSizeY
		{
			get { return adView.MaxSizeY; }
			set { adView.MaxSizeY = value; }
		}
		public Color BackgroundColor
		{
			get { return adView.BackgroundColor; }
			set { adView.BackgroundColor = value; }
		}
		public Color TextColor
		{
			get { return adView.TextColor; }
			set { adView.TextColor = value; }
		}
		public string CustomParameters
		{
			get { return adView.CustomParameters; }
			set { adView.CustomParameters = value; }
		}
		public string AdServerURL
		{
			get { return adView.AdServerURL; }
			set { adView.AdServerURL = value; }
		}
		public Image DefaultImage
		{
			get { return adView.DefaultImage; }
			set { adView.DefaultImage = value; }
		}
		public bool InternalBrowser
		{
			get { return adView.InternalBrowser; }
			set { adView.InternalBrowser = value; }
		}
		public int AdvertiserId
		{
			get { return adView.AdvertiserId; }
			set { adView.AdvertiserId = value; }
		}
		public string GroupCode
		{
			get { return adView.GroupCode; }
			set { adView.GroupCode = value; }
		}
		public int UpdateTime
		{
			get { return adView.UpdateTime; }
			set { adView.UpdateTime = value; }
		}
		public string Latitude
		{
			get { return adView.Latitude; }
			set { adView.Latitude = value; }
		}
		public string Longitude
		{
			get { return adView.Longitude; }
			set { adView.Longitude = value; }
		}
		public string Country
		{
			get { return adView.Country; }
			set { adView.Country = value; }
		}
		public string Region
		{
			get { return adView.Region; }
			set { adView.Region = value; }
		}
		public string City
		{
			get { return adView.City; }
			set { adView.City = value; }
		}
		public string Area
		{
			get { return adView.Area; }
			set { adView.Area = value; }
		}
		public string Metro
		{
			get { return adView.Metro; }
			set { adView.Metro = value; }
		}
		public string Zip
		{
			get { return adView.Zip; }
			set { adView.Zip = value; }
		}
		public string Carrier
		{
			get { return adView.Carrier; }
			set { adView.Carrier = value; }
		}
		public bool Track
		{
			get { return adView.Track; }
			set { adView.Track = value; }
		}
		public Button CloseButton
		{
			get { return closeButton; }
			set
			{
				if (value != null)
				{
					closeButton = value;
					closeButton.Visibility = Visibility.Collapsed;
					closeButton.Click += closeButton_Click;
					container.Children.Add(closeButton);
				}
			}
		}
		public uint ShowCloseButtonTime { get; set; }
		public uint AutoCloseInterstitialTime { get; set; }
		#endregion

		public void Update()
		{
			adView.Update();
		}

		
		#region Initializing
		private void AdInterstitialView_Loaded(object sender, RoutedEventArgs e)
		{
			ownerFrame = Application.Current.RootVisual as PhoneApplicationFrame;
			if (ownerFrame != null)
			{
				screenSize = ownerFrame.RenderSize;
				ResetView();
			}
		}

		private void popup_Loaded(object sender, RoutedEventArgs e)
		{
			ownerFrame = Application.Current.RootVisual as PhoneApplicationFrame;
			if (ownerFrame != null)
			{
				ownerFrame.Loaded += ownerFrame_Loaded;

				ownerPage = ownerFrame.Content as PhoneApplicationPage;
				if (ownerPage != null)
				{
					pageOrientation = ownerPage.Orientation;
					ownerPage.OrientationChanged += ownerPage_OrientationChanged;
					ownerPage.BackKeyPress += new EventHandler<System.ComponentModel.CancelEventArgs>(ownerPage_BackKeyPress);

					if (ownerPage.ApplicationBar != null)
					{
						appBarVisible = ownerPage.ApplicationBar.IsVisible;
						ownerPage.ApplicationBar.IsVisible = false;
					}

					systemTrayVisible = Microsoft.Phone.Shell.SystemTray.IsVisible;
					Microsoft.Phone.Shell.SystemTray.IsVisible = false;
				}
			}

			if (AutoCloseInterstitialTime == 0)
			{
				AutoCloseInterstitialTime = UInt32.MaxValue;
			}
			adTimer.Change(AutoCloseInterstitialTime * 1000, UInt32.MaxValue);

			closeButtonTimer.Change(ShowCloseButtonTime * 1000, System.UInt32.MaxValue);
		}

		private void ownerPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
		{
			try
			{
				if (adView != null)
				{
					adView.ownerPage_BackKeyPress(sender, e);
				}

				if (!e.Cancel)
				{
					if (popup.IsOpen)
					{
						e.Cancel = true;
						Close();
					}
				}
			}
			catch (Exception)
			{ }
		}

		private void ownerPage_OrientationChanged(object sender, OrientationChangedEventArgs e)
		{
			pageOrientation = e.Orientation;
			ResetView();
		}

		private void ownerFrame_Loaded(object sender, RoutedEventArgs e)
		{
			if (ownerFrame != null)
			{
				ownerFrame.Loaded -= ownerFrame_Loaded;
				screenSize = ownerFrame.RenderSize;
				ResetView();
			}
		}
		
		private void ResetView()
		{
			if (popup.IsOpen && screenSize.Width > 0 && screenSize.Height > 0)
			{
				try
				{
					switch (pageOrientation)
					{
						case PageOrientation.Landscape:
						case PageOrientation.LandscapeLeft:
						case PageOrientation.LandscapeRight:
							popup.VerticalOffset = 0;
							container.Width = screenSize.Height;
							container.Height = screenSize.Width;
							break;
						default:
							//popup.VerticalOffset = -systemTrayHeight;
							container.Width = screenSize.Width;
							container.Height = screenSize.Height;
							break;
					}
				}
				catch (Exception)
				{}
			}
		}
		#endregion

		#region Proccess
		private void closeButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void ProcessCloseButtonTimer(object sender)
		{
			ShowCloseButton();
		}

		private void ShowCloseButton()
		{
			if (closeButtonTimer != null)
			{
				closeButtonTimer.Dispose();
				closeButtonTimer = null;
			}

			Deployment.Current.Dispatcher.BeginInvoke(() =>
				{
					if (closeButton != null)
					{
						closeButton.Visibility = Visibility.Visible;
					}
					else
					{
						imageClose.Visibility = Visibility.Visible;
					}
				}
			);
		}

		private void ProcessAdTimer(object sender)
		{
			Deployment.Current.Dispatcher.BeginInvoke(() => Close());
		}

		private void imageClose_Tap(object sender, System.Windows.Input.GestureEventArgs e)
		{
			Close();
		}

		private void Close()
		{
			if (popup.IsOpen)
			{
				try
				{
					popup.IsOpen = false;
					popup.Child = null;
					adView = null;
					container.Children.Clear();
					container = null;

					if (adTimer != null)
					{
						adTimer.Dispose();
					}

					if (closeButtonTimer != null)
					{
						closeButtonTimer.Dispose();
					}

					if (ownerPage != null)
					{
						if (ownerPage.ApplicationBar != null)
						{
							if (!ownerPage.ApplicationBar.IsVisible)
							{
								ownerPage.ApplicationBar.IsVisible = appBarVisible;
							}
						}

						if (!Microsoft.Phone.Shell.SystemTray.IsVisible)
						{
							Microsoft.Phone.Shell.SystemTray.IsVisible = systemTrayVisible;
						}
					}
				}
				catch (Exception)
				{}
			}
		}
		#endregion

		#region Events
		public event EventHandler AdDownloadBegin
		{
			add { adView.AdDownloadBegin += value; }
			remove { adView.AdDownloadBegin -= value; }
		}

		public event EventHandler AdDownloadEnd
		{
			add { adView.AdDownloadEnd += value; }
			remove { adView.AdDownloadEnd -= value; }
		}

		public event EventHandler<AdView.DownloadErrorEventArgs> AdDownloadError
		{
			add { adView.AdDownloadError += value; }
			remove { adView.AdDownloadError -= value; }
		}

		public event EventHandler<NavigatingEventArgs> AdNavigateBanner
		{
			add { adView.AdNavigateBanner += value; }
			remove { adView.AdNavigateBanner -= value; }
		}

		public event EventHandler<AdView.WebViewClosingEventArgs> AdWebViewClosing
		{
			add { adView.AdWebViewClosing += value; }
			remove { adView.AdWebViewClosing -= value; }
		}

		public event EventHandler AdClose = null;
		protected virtual void OnAdClose()
		{
			EventHandler handler = AdClose;
			if (handler != null)
			{
				try
				{
					handler(this, EventArgs.Empty);
				}
				catch (Exception)
				{ }
			}
		}
		#endregion
	}
}
