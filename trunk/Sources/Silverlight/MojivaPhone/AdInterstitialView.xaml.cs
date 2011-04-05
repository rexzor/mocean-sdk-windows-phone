/****
 * © 2010-2011 mOcean Mobile. A subsidiary of Mojiva, Inc. All Rights Reserved.
 * */

using System.Windows;
using System.Windows.Media;
using Microsoft.Phone.Controls;

namespace MojivaPhone
{
	public partial class AdInterstitialView : AdView
	{
		public enum AdInterstitialCloseButtonPosition
		{
			AdInterstitialCloseButtonPositionTop = 0,
			AdInterstitialCloseButtonPositionBottom = 1,
			AdInterstitialCloseButtonPositionLeft = 2,
			AdInterstitialCloseButtonPositionRight = 3,
			AdInterstitialCloseButtonPositionCenter = 4,
		};

		private const int DEFAULT_CLOSE_BUTTON_WIDTH = 173;
		private const int DEFAULT_CLOSE_BUTTON_HEIGHT = 173;
		private const string DEFAULT_CLOSE_BUTTON_TEXT = "";

		#region "Variables"
		private AdInterstitialCloseButtonPosition closeButtonPosition = AdInterstitialCloseButtonPosition.AdInterstitialCloseButtonPositionCenter;
		private uint showCloseButtonTime = 0;
		private uint autoCloseInterstitialTime = System.UInt32.MaxValue;
		private bool isPhoneStatusBarShowedBefore = false;
		private System.Threading.Timer closeButtonTimer = null;
		private System.Threading.Timer adTimer = null;
		private CustomButton closeButton = new CustomButton();
		#endregion

		#region "Properties"
		public AdInterstitialCloseButtonPosition CloseButtonPosition
		{
			get { return closeButtonPosition; }
			set { closeButtonPosition = value; }
		}
		public uint ShowCloseButtonTime
		{
			get { return showCloseButtonTime / 1000; }
			set { showCloseButtonTime = value * 1000; }
		}
		public uint AutoCloseInterstitialTime
		{
			get { return autoCloseInterstitialTime / 1000; }
			set
			{
				autoCloseInterstitialTime = value * 1000;
				if (autoCloseInterstitialTime == 0)
				{
					autoCloseInterstitialTime = System.UInt32.MaxValue;
				}
			}
		}
		public Color CloseButtonTextColor
		{
			get { return closeButton.TextColor; }
			set { closeButton.TextColor = value; }
		}
		public Color CloseButtonBackgroundColor
		{
			get { return closeButton.BackgroundColor; }
			set { closeButton.BackgroundColor = value; }
		}
		public string CloseButtonImage
		{
			set { closeButton.ImageSource = value; }
			get { return closeButton.ImageSource; }
		}
		public string CloseButtonSelectedImage
		{
			set { closeButton.ImagePressedSource = value; }
			get { return closeButton.ImagePressedSource; }
		}
		public string CloseButtonText
		{
			get { return closeButton.Text; }
			set { closeButton.Text = value; }
		}
		public byte CloseButtonTransparency
		{
			get { return closeButton.Transparency; }
			set { closeButton.Transparency = value; }
		}
		public Size CloseButtonSize
		{
			get { return closeButton.Size; }
			set { closeButton.Size = value; }
		}
		#endregion

		#region "Public methods"
		public AdInterstitialView() : base()
		{
			adTimer = new System.Threading.Timer(new System.Threading.TimerCallback(ProcessAdTimerEvent), this, System.UInt32.MaxValue, System.UInt32.MaxValue);
			closeButtonTimer = new System.Threading.Timer(new System.Threading.TimerCallback(ProcessCloseBtnTimerEvent), this, System.UInt32.MaxValue, System.UInt32.MaxValue);

			LayoutRoot.Visibility = System.Windows.Visibility.Collapsed;
		}

		protected override void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			base.UserControl_Loaded(sender, e);

			width = (int)Application.Current.RootVisual.RenderSize.Width;
			height = (int)Application.Current.RootVisual.RenderSize.Height;
		}

		public override void Update()
		{
			InitAdProperties();
			InitCloseButton();

			base.Update();

			LayoutRoot.Visibility = System.Windows.Visibility.Visible;
			closeButton.Visibility = System.Windows.Visibility.Collapsed;

			InitShowTimers();
		}

		public void Close()
		{
			adTimer.Dispose();
			closeButtonTimer.Dispose();

			LayoutRoot.Visibility = System.Windows.Visibility.Collapsed;

			if (Owner != null && Owner.ApplicationBar != null)
			{
				Owner.ApplicationBar.IsVisible = true;
			}

			Microsoft.Phone.Shell.SystemTray.IsVisible = isPhoneStatusBarShowedBefore;
		}

		~AdInterstitialView()
		{
			closeButtonTimer.Dispose();
			adTimer.Dispose();
		}
		#endregion

		#region "Private methods"
		private void InitAdProperties()
		{
			Padding = new Thickness(0);
			Margin = new Thickness(0);

			if (Owner != null)
			{
				if (Owner.ApplicationBar != null)
				{
					Owner.ApplicationBar.IsVisible = false;
				}

				Expand(new OrientationChangedEventArgs(Owner.Orientation));
			}

			isPhoneStatusBarShowedBefore = Microsoft.Phone.Shell.SystemTray.IsVisible;
			Microsoft.Phone.Shell.SystemTray.IsVisible = false;
		}

		private void Expand(OrientationChangedEventArgs e)
		{
			LayoutRoot.Margin = new Thickness(0);

			if (e.Orientation == PageOrientation.Portrait || e.Orientation == PageOrientation.PortraitUp || e.Orientation == PageOrientation.PortraitDown)
			{
				Resize(width, height);
			}
			else if (e.Orientation == PageOrientation.Landscape || e.Orientation == PageOrientation.LandscapeLeft || e.Orientation == PageOrientation.LandscapeRight)
			{
				Resize(height, width);
			}
		}

		private void InitShowTimers()
		{
			adTimer.Change(autoCloseInterstitialTime, System.UInt32.MaxValue);
			closeButtonTimer.Change(showCloseButtonTime, System.UInt32.MaxValue);
		}

		private void ProcessCloseBtnTimerEvent(object sender)
		{
			Deployment.Current.Dispatcher.BeginInvoke(() => ShowCloseBtn());
		}

		private void InitCloseButton()
		{
			closeButton.Init();
			closeButton.Click += new RoutedEventHandler(CloseBtn_Click);
			closeButton.Text = DEFAULT_CLOSE_BUTTON_TEXT;

			LayoutRoot.Children.Add(closeButton);
			SetCloseButtonPosition();
		}

		private void ShowCloseBtn()
		{
			closeButtonTimer.Dispose();
			closeButton.Visibility = System.Windows.Visibility.Visible;
		}

		private void ProcessAdTimerEvent(object sender)
		{
			Deployment.Current.Dispatcher.BeginInvoke(() => Close());
		}

		private void SetCloseButtonPosition()
		{
			switch (closeButtonPosition)
			{
				case AdInterstitialCloseButtonPosition.AdInterstitialCloseButtonPositionTop:
					closeButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
					closeButton.VerticalAlignment = System.Windows.VerticalAlignment.Top;
					break;
				case AdInterstitialCloseButtonPosition.AdInterstitialCloseButtonPositionBottom:
					closeButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
					closeButton.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
					break;
				case AdInterstitialCloseButtonPosition.AdInterstitialCloseButtonPositionLeft:
					closeButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
					closeButton.VerticalAlignment = System.Windows.VerticalAlignment.Center;
					break;
				case AdInterstitialCloseButtonPosition.AdInterstitialCloseButtonPositionRight:
					closeButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
					closeButton.VerticalAlignment = System.Windows.VerticalAlignment.Center;
					break;
				case AdInterstitialCloseButtonPosition.AdInterstitialCloseButtonPositionCenter:
				default:
					closeButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
					closeButton.VerticalAlignment = System.Windows.VerticalAlignment.Center;
					break;
			}
		}

		private void CloseBtn_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		protected override void OnOrientationChanged(object sender, OrientationChangedEventArgs e)
		{
			Expand(e);
			base.OnOrientationChanged(sender, e);
		}

		protected override string GetMetaTags()
		{
			return "<meta name=\"viewport\" content=\"width=device-width; user-scalable=yes\"/>";
		}

		#endregion
	}
}
