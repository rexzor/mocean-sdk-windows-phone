/****
 * © 2010-2011 mOcean Mobile. A subsidiary of Mojiva, Inc. All Rights Reserved.
 * */

using System;
using System.Windows;
using System.Windows.Media;

namespace MojivaPhone
{
	public partial class AdInterstitialView : AdView
	{
		public enum AdInterstitialCloseButtonPosition
		{
			AdInterstitialCloseButtonPositionCenter = 0,
			AdInterstitialCloseButtonPositionTop = 1,
			AdInterstitialCloseButtonPositionBottom = 2,
			AdInterstitialCloseButtonPositionLeft = 4,
			AdInterstitialCloseButtonPositionRight = 8,
		};

		private const int DEFAULT_CLOSE_BUTTON_WIDTH = 60;
		private const int DEFAULT_CLOSE_BUTTON_HEIGHT = 60;
		private const string DEFAULT_CLOSE_BUTTON_CONTENT = "X";

		#region "Variables"
		private AdInterstitialCloseButtonPosition closeButtonPosition = AdInterstitialCloseButtonPosition.AdInterstitialCloseButtonPositionTop | AdInterstitialCloseButtonPosition.AdInterstitialCloseButtonPositionRight;
		private uint showCloseButtonTime = 0;
		private uint autoCloseInterstitialTime = 0;
		private Color closeButtonTextColor = Color.FromArgb(0, 0, 0, 0);
		private Color closeButtonBackgroundColor = Color.FromArgb(0, 0, 0, 0);
		private string closeButtonImage = null;
		private string closeButtonText = DEFAULT_CLOSE_BUTTON_CONTENT;
		private int closeButtonTransparency = 255;
		private Size closeButtonSize = new Size((double)DEFAULT_CLOSE_BUTTON_WIDTH, (double)DEFAULT_CLOSE_BUTTON_HEIGHT);
		private bool isShowPhoneStatusBar = false;
		private System.Threading.Timer closeBtnTimer = null;
		private System.Threading.Timer adTimer = null;
		private System.Windows.Controls.Button closeBtn = new System.Windows.Controls.Button();
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
			get { return autoCloseInterstitialTime/1000; }
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
			get { return closeButtonTextColor; }
			set { closeButtonTextColor = value; }
		}
		public Color CloseButtonBackgroundColor
		{
			get { return closeButtonBackgroundColor; }
			set { closeButtonBackgroundColor = value; }
		}
		public bool IsShowPhoneStatusBar
		{
			get { return isShowPhoneStatusBar; }
			set { isShowPhoneStatusBar = value; }
		}
		public string CloseButtonImage
		{
			set
			{
				closeButtonImage = value;
			}
		}
		[Obsolete("This property is not supported.")]
		public string CloseButtonSelectedImage
		{
			set{}
		}
		public string CloseButtonText
		{
			get { return closeButtonText; }
			set { closeButtonText = value; }
		}
		public int CloseButtonTransparency
		{
			get { return closeButtonTransparency; }
			set { closeButtonTransparency = value; }
		}
		public Size CloseButtonSize
		{
			get
			{
				return closeButtonSize;
			}
			set
			{
				closeButtonSize = value;
			}
		}
		#endregion

		#region "Public methods"
		public AdInterstitialView() : base()
		{
			adTimer = new System.Threading.Timer(new System.Threading.TimerCallback(ProcessAdTimerEvent), this, System.UInt32.MaxValue, System.UInt32.MaxValue);
			closeBtnTimer = new System.Threading.Timer(new System.Threading.TimerCallback(ProcessCloseBtnTimerEvent), this, System.UInt32.MaxValue, System.UInt32.MaxValue);

 			LayoutRoot.Children.Add(closeBtn);
			LayoutRoot.Visibility = System.Windows.Visibility.Collapsed;
		}

		public override void Run()
		{
			InitAdProperties();
			base.Run();

			LayoutRoot.Visibility = System.Windows.Visibility.Visible;
			closeBtn.Visibility = System.Windows.Visibility.Collapsed;

			InitShowTimers();
		}

		public void Close()
		{
			adTimer.Dispose();

			LayoutRoot.Visibility = System.Windows.Visibility.Collapsed;

			if (Owner != null && Owner.ApplicationBar != null)
			{
				Owner.ApplicationBar.IsVisible = true;
			}

			Microsoft.Phone.Shell.SystemTray.IsVisible = true;
		}

		~AdInterstitialView()
		{
			closeBtnTimer.Dispose();
			adTimer.Dispose();
		}
		#endregion

		#region "Private methods"
		private void InitAdProperties()
		{
			Padding = new System.Windows.Thickness(0);
			Margin = new System.Windows.Thickness(0);
			Resize((int)System.Windows.Application.Current.RootVisual.RenderSize.Width - 0, (int)System.Windows.Application.Current.RootVisual.RenderSize.Height - 0);

			if (Owner != null && Owner.ApplicationBar != null)
			{
				Owner.ApplicationBar.IsVisible = false;
			}
			Microsoft.Phone.Shell.SystemTray.IsVisible = isShowPhoneStatusBar;

			InitDefaultCloseBtn();
 			SetCloseBtnImage(closeButtonImage);
		}

		private void InitShowTimers()
		{
			adTimer.Change(autoCloseInterstitialTime, System.UInt32.MaxValue);
			closeBtnTimer.Change(showCloseButtonTime, System.UInt32.MaxValue);
		}

		private void ProcessCloseBtnTimerEvent(object sender)
		{
			System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() => ShowCloseBtn());
		}

		private void InitDefaultCloseBtn()
		{
			closeBtn.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
			closeBtn.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
			closeBtn.BorderThickness = new System.Windows.Thickness(0);
			closeBtn.Click += new System.Windows.RoutedEventHandler(CloseBtn_Click);

			if (closeButtonBackgroundColor != null)
			{
				closeBtn.Background = new System.Windows.Media.SolidColorBrush(closeButtonBackgroundColor);
			}

			if (closeButtonTextColor != null)
			{
				closeBtn.Foreground = new System.Windows.Media.SolidColorBrush(closeButtonTextColor);
			}

			closeBtn.Content = closeButtonText;
			closeBtn.Opacity = (double)closeButtonTransparency / 255.0d;

			closeBtn.Width = CloseButtonSize.Width;
			closeBtn.Height = CloseButtonSize.Height;

			InitCloseBtnPosition();
		}
		private void ShowCloseBtn()
		{
			closeBtnTimer.Dispose();
			closeBtn.Visibility = System.Windows.Visibility.Visible;
		}
		private void SetCloseBtnImage(string uri)
		{
			if (String.IsNullOrEmpty(uri))
			{
				return;
			}

			var brush = new System.Windows.Media.ImageBrush();
			var bmpImg = new System.Windows.Media.Imaging.BitmapImage(new System.Uri(uri, System.UriKind.RelativeOrAbsolute));

			brush.ImageSource = bmpImg;
			closeBtn.Background = brush;

/*			brush.ImageOpened += (sender, evt) =>
			{
				closeBtn.Content = System.String.Empty;
				closeBtn.Width = bmpImg.PixelWidth;
				closeBtn.Height = bmpImg.PixelHeight;
			};
*/
			brush.ImageFailed += (sender, evt) =>
			{
				InitDefaultCloseBtn();
			};
		}

		private void ProcessAdTimerEvent(object sender)
		{
			System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() => Close());
		}

		private void InitCloseBtnPosition()
		{
			switch (closeButtonPosition)
			{
				case AdInterstitialCloseButtonPosition.AdInterstitialCloseButtonPositionTop | AdInterstitialCloseButtonPosition.AdInterstitialCloseButtonPositionLeft:
					closeBtn.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
					closeBtn.VerticalAlignment = System.Windows.VerticalAlignment.Top;
					break;
				case AdInterstitialCloseButtonPosition.AdInterstitialCloseButtonPositionTop | AdInterstitialCloseButtonPosition.AdInterstitialCloseButtonPositionRight:
					closeBtn.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
					closeBtn.VerticalAlignment = System.Windows.VerticalAlignment.Top;
					break;
				case AdInterstitialCloseButtonPosition.AdInterstitialCloseButtonPositionBottom | AdInterstitialCloseButtonPosition.AdInterstitialCloseButtonPositionLeft:
					closeBtn.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
					closeBtn.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
					break;
				case AdInterstitialCloseButtonPosition.AdInterstitialCloseButtonPositionBottom | AdInterstitialCloseButtonPosition.AdInterstitialCloseButtonPositionRight:
					closeBtn.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
					closeBtn.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
					break;
				case AdInterstitialCloseButtonPosition.AdInterstitialCloseButtonPositionTop:
					closeBtn.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
					closeBtn.VerticalAlignment = System.Windows.VerticalAlignment.Top;
					break;
				case AdInterstitialCloseButtonPosition.AdInterstitialCloseButtonPositionBottom:
					closeBtn.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
					closeBtn.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
					break;
				case AdInterstitialCloseButtonPosition.AdInterstitialCloseButtonPositionRight:
					closeBtn.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
					closeBtn.VerticalAlignment = System.Windows.VerticalAlignment.Center;
					break;
				case AdInterstitialCloseButtonPosition.AdInterstitialCloseButtonPositionLeft:
					closeBtn.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
					closeBtn.VerticalAlignment = System.Windows.VerticalAlignment.Center;
					break;
				case AdInterstitialCloseButtonPosition.AdInterstitialCloseButtonPositionCenter:
				default:
					closeBtn.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
					closeBtn.VerticalAlignment = System.Windows.VerticalAlignment.Center;
					break;
			}
		}

		private void CloseBtn_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			Close();
		}
		#endregion
	}
}
