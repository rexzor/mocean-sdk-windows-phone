/****
 * © 2010-2011 mOcean Mobile. A subsidiary of Mojiva, Inc. All Rights Reserved.
 * */

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Microsoft.Phone.Controls;
using System.Windows.Media;

namespace mOceanWindowsPhone
{
	internal abstract class PopupWindow
	{
		protected Color BG_COLOR = Color.FromArgb(150, 0, 255, 0);

		#region "Variables"
		protected PhoneApplicationPage page = null;
		protected Popup popup = null;
		protected Grid container = null;
		protected FrameworkElement control = null;

		private Point parentPosition = new Point(0, 0);
		protected Size size = new Size(0, 0);

		private bool isPhoneStatusBarWasVisible = false;
		private bool isApplicationBarWasVisible = false;

		#endregion

		#region "Properties"
		public UIElement Window
		{
			get { return popup; }
			protected set {}
		}

		public bool IsOpen
		{
			get { return (popup != null ? popup.IsOpen : false); }
			private set {}
		}
		#endregion

		protected PopupWindow()
		{
		}

		protected PopupWindow(PhoneApplicationPage page, Size size)
		{
			this.page = page;
			this.size = size;
			Init();
		}

		protected PopupWindow(PhoneApplicationPage page, Point parentPosition, Size size)
 		{
			this.page = page;
			this.parentPosition = parentPosition;
			this.size = size;
			Init();
		}

		public void SetSize(Size size)
		{
			if (!size.IsEmpty)
			{
				if (popup != null)
				{
					popup.Width = size.Width;
					popup.Height = size.Height;

					if (container != null)
					{
						container.Width = size.Width;
						container.Height = size.Height;
					}

					if (control != null)
					{
						control.Width = size.Width;
						control.Height = size.Height;
					}
				}
			}
		}

		public void Show()
		{
			isPhoneStatusBarWasVisible = Microsoft.Phone.Shell.SystemTray.IsVisible;
			Microsoft.Phone.Shell.SystemTray.IsVisible = false;

			if (page != null)
			{
				if (page.ApplicationBar != null)
				{
					isApplicationBarWasVisible = page.ApplicationBar.IsVisible;
					page.ApplicationBar.IsVisible = false;
				}
			}

			if (popup != null)
			{
				popup.IsOpen = true;
			}
		}

		public virtual void Close()
		{
			if (popup != null)
			{
				popup.IsOpen = false;
				Microsoft.Phone.Shell.SystemTray.IsVisible = isPhoneStatusBarWasVisible;

				if (page != null)
				{
					if (page.ApplicationBar != null)
					{
						page.ApplicationBar.IsVisible = isApplicationBarWasVisible;
					}
				}
			}
		}

		protected virtual void Init()
		{
			popup = new Popup();
			popup.Width = size.Width;
			popup.Height = size.Height;
			popup.HorizontalOffset = -parentPosition.X;
			popup.VerticalOffset = -parentPosition.Y;

			container = new Grid();
			container.Background = new SolidColorBrush(BG_COLOR);
			container.HorizontalAlignment = HorizontalAlignment.Center;
			container.VerticalAlignment = VerticalAlignment.Center;
			container.Width = popup.Width;
			container.Height = popup.Height;

			if (control != null)
			{
				control.Width = popup.Width;
				control.Height = popup.Height;
				control.HorizontalAlignment = HorizontalAlignment.Center;
				control.VerticalAlignment = VerticalAlignment.Center;
				container.Children.Add(control);
			}

			popup.Child = container;
		}
	}
}
