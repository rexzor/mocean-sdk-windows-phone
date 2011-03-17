﻿/****
 * © 2010-2011 mOcean Mobile. A subsidiary of Mojiva, Inc. All Rights Reserved.
 * */

namespace MojivaPhone
{
	internal class CExpandedView
	{
		private System.Windows.Controls.Primitives.Popup popup;

		public CExpandedView(string posStr, string propStr, System.Uri uri)
		{
			CPosition pos = new CPosition(posStr.Replace("-", "_"));
			CProperties prop = new CProperties(propStr.Replace("-", "_"));

			popup = new System.Windows.Controls.Primitives.Popup();

			var wb = new Microsoft.Phone.Controls.WebBrowser();
			wb.Width = pos.width;
			wb.Height = pos.height;

			popup.Child = wb;

			popup.HorizontalOffset = pos.x;
			popup.VerticalOffset = pos.y;
			wb.Opacity = prop.background_opacity;
			wb.Source = uri;

			var color = System.Windows.Media.Color.FromArgb(255, 255, 255, 255);
			if (prop.background_color.Length >= 7)
			{
				try
				{
					byte r = byte.Parse(prop.background_color.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
					byte g = byte.Parse(prop.background_color.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
					byte b = byte.Parse(prop.background_color.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);
					color = System.Windows.Media.Color.FromArgb(255, r, g, b);
				}
				catch (System.Exception /*ex*/)
				{
					color = System.Windows.Media.Color.FromArgb(100, 255, 255, 255);
				}
			}

			wb.Background = new System.Windows.Media.SolidColorBrush(color);
		}

		~CExpandedView()
		{
		}

		public void Show()
		{
			popup.IsOpen = true;
		}

		public void Close()
		{
			popup.IsOpen = false;
		}
	}
}