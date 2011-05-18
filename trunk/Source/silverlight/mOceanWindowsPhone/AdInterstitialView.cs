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
	public class AdInterstitialView : UserControl
	{
		public enum ButtonPosition
		{
			Top = 0,
			Bottom = 1,
			Left = 2,
			Right = 3,
			Center = 4,
		};

		private const int DEFAULT_CLOSE_BUTTON_WIDTH = 173;
		private const int DEFAULT_CLOSE_BUTTON_HEIGHT = 173;
		private const string DEFAULT_CLOSE_BUTTON_TEXT = "";

		#region "Variables"
		private Popup popup = null;
		private AdView adView = null;
		private ButtonPosition closeButtonPosition = ButtonPosition.Left;
		private uint showCloseButtonTime = 0;
		private uint autoCloseInterstitialTime = System.UInt32.MaxValue;
		private bool isPhoneStatusBarWasVisible = false;
		private bool isApplicationBarWasVisible = false;
		private System.Threading.Timer closeButtonTimer = null;
		private System.Threading.Timer adTimer = null;
		private CustomButton closeButton = null;

		private bool firstUpdate = true;
		private readonly double width = 0;
		private readonly double height = 0;
		#endregion

		#region "Public methods"
		public AdInterstitialView()
		{
			width = 480;
			height = 800;

			Init();
		}

		public AdInterstitialView(int site, int zone) : this()
		{
			SetSite(site);
			SetZone(zone);
		}

		public void Update()
		{
			adView.Update();
		}
		#endregion

		#region "Setting parameters"
		public void SetSite(int site)
		{
			adView.SetSite(site);
		}
		public void SetZone(int zone)
		{
			adView.SetZone(zone);
		}
		public void SetTest(bool test)
		{
			adView.SetTest(test);
		}
		public void SetPremium(int premium)
		{
			adView.SetPremium(premium);
		}
		public void SetKeywords(string keywords)
		{
			adView.SetKeywords(keywords);
		}
		public void SetAdsType(int adsType)
		{
			adView.SetAdsType(adsType);
		}
		public void SetMinSizeX(int size)
		{
			adView.SetMinSizeX(size);
		}
		public void SetMinSizeY(int size)
		{
			adView.SetMinSizeY(size);
		}
		public void SetMaxSizeX(int size)
		{
			adView.SetMaxSizeX(size);
		}
		public void SetMaxSizeY(int size)
		{
			adView.SetMaxSizeY(size);
		}
		public void SetBackgroundColor(Color backgroundColor)
		{
			adView.SetBackgroundColor(backgroundColor);
		}
		public void SetTextColor(Color textColor)
		{
			adView.SetTextColor(textColor);
		}
		public void SetCustomParameters(string customParameters)
		{
			adView.SetCustomParameters(customParameters);
		}
		public void SetAdserverURL(string adServerUrl)
		{
			adView.SetAdserverURL(adServerUrl);
		}
		public void SetDefaultImage(Image defaultImage)
		{
			adView.SetDefaultImage(defaultImage);
		}
		public void SetInternalBrowser(bool internalBrowser)
		{
			adView.SetInternalBrowser(internalBrowser);
		}
		public void SetAdvertiserId(int advertiserId)
		{
			adView.SetAdvertiserId(advertiserId);
		}
		public void SetGroupCode(string groupCode)
		{
			adView.SetGroupCode(groupCode);
		}
		public void SetUpdateTime(int updateTime)
		{
			adView.SetUpdateTime(updateTime);
		}
		public void SetLatitude(string latitude)
		{
			adView.SetLatitude(latitude);
		}
		public void SetLongitude(string longitude)
		{
			adView.SetLongitude(longitude);
		}
		public void SetCountry(string country)
		{
			adView.SetCountry(country);
		}
		public void SetRegion(string region)
		{
			adView.SetRegion(region);
		}
		public void SetCity(string city)
		{
			adView.SetCity(city);
		}
		public void SetArea(string area)
		{
			adView.SetArea(area);
		}
		public void SetMetro(string metro)
		{
			adView.SetMetro(metro);
		}
		public void SetZip(string zip)
		{
			adView.SetZip(zip);
		}
		public void SetCarrier(string carrier)
		{
			adView.SetCarrier(carrier);
		}
		public void SetLogger(Logger logger)
		{
			adView.SetLogger(logger);
		}
		public void SetLogLevel(Logger.LogLevel logLevel)
		{
			adView.SetLogLevel(logLevel);
		}
		public void SetCloseButtonPosition(ButtonPosition position)
		{
			closeButtonPosition = position;
		}
		public void SetShowCloseButtonTime(uint time)
		{
			showCloseButtonTime = time * 1000;
		}
		public void SetAutoCloseInterstitialTime(uint time)
		{
			if (time == 0)
			{
				autoCloseInterstitialTime = System.UInt32.MaxValue;
			}
			else
			{
				autoCloseInterstitialTime = time * 1000;
			}
		}
		public void SetCloseButtonTextColor(Color color)
		{
			closeButton.TextColor = color;
		}
		public void SetCloseButtonBackgroundColor(Color color)
		{
			closeButton.BackgroundColor = color;
		}
		public void SetCloseButtonImage(Image image)
		{
			closeButton.Image = image;
		}
		public void SetCloseButtonSelectedImage(Image image)
		{
			closeButton.ImagePressed = image;
		}
		public void SetCloseButtonText(string text)
		{
			closeButton.Text = text;
		}
		public void SetCloseButtonTransparency(byte transparency)
		{
			closeButton.Transparency = transparency;
		}
		public void SetCloseButtonSize(Size size)
		{
			closeButton.Size = size;
		}
		#endregion

		#region "Private methods"
		~AdInterstitialView()
		{
			closeButtonTimer = null;
			adTimer = null;
			closeButton = null;
			adView = null;
			popup = null;
		}

		private void Init()
		{
			adView = new AdView();
			adView.HorizontalAlignment = HorizontalAlignment.Left;
			adView.VerticalAlignment = VerticalAlignment.Top;
			adView.AdViewLoaded += new EventHandler(AdViewLoaded);
			adView.AdDownloadError += new AdDownloadErrorEventHandler(AdViewDownloadError);

			adView.Opacity = 1;

			popup = new Popup();
			popup.Width = width;
			popup.Height = height;
			popup.HorizontalOffset = 0;
			popup.VerticalOffset = 0;
			popup.Closed += new EventHandler(Popup_Closed);

			Grid grid = new Grid();
			grid.Width = width;
			grid.Height = height;
			grid.Background = new SolidColorBrush(Color.FromArgb(0, 0, 255, 0));
			grid.Children.Add(adView);

			closeButton = new CustomButton();
			grid.Children.Add(closeButton);

			popup.Child = grid;

			adTimer = new System.Threading.Timer(new System.Threading.TimerCallback(ProcessAdTimer), this, System.UInt32.MaxValue, System.UInt32.MaxValue);
			closeButtonTimer = new System.Threading.Timer(new System.Threading.TimerCallback(ProcessCloseButtonTimer), this, System.UInt32.MaxValue, System.UInt32.MaxValue);

			closeButton.HorizontalAlignment = HorizontalAlignment.Center;
			closeButton.VerticalAlignment = VerticalAlignment.Center;
			closeButton.Visibility = System.Windows.Visibility.Collapsed;
			closeButton.BackgroundColor = Colors.Magenta;
			closeButton.Text = DEFAULT_CLOSE_BUTTON_TEXT;
			closeButton.Size = new Size(DEFAULT_CLOSE_BUTTON_WIDTH, DEFAULT_CLOSE_BUTTON_HEIGHT);

			popup.IsOpen = true;
		}

		private void InitFirstUpdate()
		{
			adTimer.Change(autoCloseInterstitialTime, System.UInt32.MaxValue);
			closeButtonTimer.Change(showCloseButtonTime, System.UInt32.MaxValue);

			closeButton.Init();
			closeButton.Click += new RoutedEventHandler(CloseButton_Click);
			SetCloseButtonPosition();
		}

		private void AdViewLoaded(object sender, EventArgs e)
		{
			if (adView.OwnerPage != null)
			{
				isPhoneStatusBarWasVisible = Microsoft.Phone.Shell.SystemTray.IsVisible;
				Microsoft.Phone.Shell.SystemTray.IsVisible = false;

				if (adView.OwnerPage.ApplicationBar != null)
				{
					isApplicationBarWasVisible = adView.OwnerPage.ApplicationBar.IsVisible;
					adView.OwnerPage.ApplicationBar.IsVisible = false;
				}

				Expand(adView.OwnerPage.Orientation);
				adView.OwnerPage.OrientationChanged += new EventHandler<OrientationChangedEventArgs>(OwnerPage_OrientationChanged);
			}

			if (firstUpdate)
			{
				firstUpdate = false;
				InitFirstUpdate();
			}
		}

		private void AdViewDownloadError(string url)
		{
			Deployment.Current.Dispatcher.BeginInvoke(() => Close());
		}

		private void OwnerPage_OrientationChanged(object sender, OrientationChangedEventArgs e)
		{
			Expand(e.Orientation);
		}

		private void Expand(PageOrientation orientation)
		{
			PageOrientation pageOrientation = orientation;

			TransformGroup transformGroup = new TransformGroup();
			RotateTransform rotateTransform = new RotateTransform();
			TranslateTransform translateTransform = new TranslateTransform();

			if (pageOrientation == PageOrientation.Landscape || pageOrientation == PageOrientation.LandscapeLeft || pageOrientation == PageOrientation.LandscapeRight)
			{
				if (pageOrientation == PageOrientation.LandscapeLeft)
				{
					rotateTransform.Angle = 90;
					translateTransform.X = width;
				}
				else if (pageOrientation == PageOrientation.LandscapeRight)
				{
					rotateTransform.Angle = -90;
					translateTransform.Y = height;
				}

				transformGroup.Children.Add(rotateTransform);
				transformGroup.Children.Add(translateTransform);

				double d = 0;

				popup.Child.RenderTransform = transformGroup;
				((Grid)popup.Child).Width = height - d;
				((Grid)popup.Child).Height = width - d;
				adView.Width = height - d;
				adView.Height = width - d;
				adView.Resize(height, width);

				adView.metaTags = "<meta name=\"viewport\" content=\"width=" + height.ToString("F0") + ", user-scalable=yes\"/>";
			}
			else if (pageOrientation == PageOrientation.Portrait || pageOrientation == PageOrientation.PortraitDown || pageOrientation == PageOrientation.PortraitUp)
			{
				transformGroup.Children.Add(rotateTransform);
				transformGroup.Children.Add(translateTransform);

				popup.Child.RenderTransform = transformGroup;
				((Grid)popup.Child).Width = width;
				((Grid)popup.Child).Height = height;
				adView.Width = width;
				adView.Height = height;
				adView.Resize(width, height);

				adView.metaTags = "<meta name=\"viewport\" content=\"width=" + width.ToString("F0") + ", user-scalable=yes\"/>";
			}

			adView.ShowAd();
		}

		private void ProcessCloseButtonTimer(object sender)
		{
			Deployment.Current.Dispatcher.BeginInvoke(() => ShowCloseBtn());
		}

		private void ShowCloseBtn()
		{
			closeButtonTimer.Dispose();
			closeButton.Visibility = System.Windows.Visibility.Visible;
		}

		private void ProcessAdTimer(object sender)
		{
			Deployment.Current.Dispatcher.BeginInvoke(() => Close());
		}

		private void SetCloseButtonPosition()
		{
			switch (closeButtonPosition)
			{
				case ButtonPosition.Top:
					closeButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
					closeButton.VerticalAlignment = System.Windows.VerticalAlignment.Top;
					break;
				case ButtonPosition.Bottom:
					closeButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
					closeButton.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
					break;
				case ButtonPosition.Left:
					closeButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
					closeButton.VerticalAlignment = System.Windows.VerticalAlignment.Center;
					break;
				case ButtonPosition.Right:
					closeButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
					closeButton.VerticalAlignment = System.Windows.VerticalAlignment.Center;
					break;
				case ButtonPosition.Center:
				default:
					closeButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
					closeButton.VerticalAlignment = System.Windows.VerticalAlignment.Center;
					break;
			}
		}

		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void Close()
		{
			if (adView != null)
			{
				adView.PauseUpdateTimer();
			}

			if (popup != null && popup.IsOpen)
			{
				popup.IsOpen = false;
			}

			if (adTimer != null)
			{
				adTimer.Dispose();
			}

			if (closeButtonTimer != null)
			{
				closeButtonTimer.Dispose();
			}

			if (adView != null && adView.OwnerPage != null && adView.OwnerPage.ApplicationBar != null)
			{
				adView.OwnerPage.ApplicationBar.IsVisible = isApplicationBarWasVisible;
			}

			Microsoft.Phone.Shell.SystemTray.IsVisible = isPhoneStatusBarWasVisible;
		}

		private void Popup_Closed(object sender, EventArgs e)
		{
			adView.OwnerPage.OrientationChanged -= OwnerPage_OrientationChanged;

			adView = null;
			popup = null;
		}
		#endregion

		#region "Events"
		public event AdNavigateEventHandler AdNavigateBanner
		{
			add { adView.AdNavigateBanner += value; }
			remove { adView.AdNavigateBanner -= value; }
		}

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

		public event AdDownloadErrorEventHandler AdDownloadError
		{
			add { adView.AdDownloadError += value; }
			remove { adView.AdDownloadError -= value; }
		}

		public event AdWebViewClosingEventHandler AdWebViewClosing
		{
			add { adView.AdWebViewClosing += value; }
			remove { adView.AdWebViewClosing -= value; }
		}
		#endregion
	}
}
