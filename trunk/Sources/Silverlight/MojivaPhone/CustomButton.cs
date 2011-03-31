/****
 * © 2010-2011 mOcean Mobile. A subsidiary of Mojiva, Inc. All Rights Reserved.
 * */

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MojivaPhone
{
	internal class CustomButton : Button
	{
		private enum STATE
		{
			FREE,
			PRESSED
		}
		private STATE state = STATE.FREE;

		#region "Variables"
		private string imageSource = null;
		private string imagePressedSource = null;
		private string text = null;
		private Color textColor = Colors.Black;
		private Color bgColor = Colors.LightGray;
		private Image image = null;
		private Image imagePressed = null;
		private TextBlock textBlock = null;
		private byte transparency = Byte.MaxValue;
		private Size size = Size.Empty;
		#endregion

		#region "Properties"
		public string ImageSource
		{
			get { return imageSource; }
			set { imageSource = value; }
		}
		public string ImagePressedSource
		{
			get { return imagePressedSource; }
			set { imagePressedSource = value; }
		}
		public string Text
		{
			get { return text; }
			set { text = value; }
		}
		public Color TextColor
		{
			get { return textColor; }
			set { textColor = value; }
		}
		public Color BackgroundColor
		{
			get { return bgColor; }
			set { bgColor = value; }
		}
		public byte Transparency
		{
			get { return transparency; }
			set { transparency = value; }
		}
		public Size Size
		{
			get { return size; }
			set { size = value; }
		}
		#endregion

		public CustomButton()
		{
			this.HorizontalContentAlignment = HorizontalAlignment.Center;
			this.VerticalContentAlignment = VerticalAlignment.Center;
			this.BorderThickness = new Thickness(0);
		}

		public void Init()
		{
			try
			{
				image = new Image();
				image.Source = new BitmapImage(new System.Uri(imageSource, System.UriKind.RelativeOrAbsolute));
				image.HorizontalAlignment = HorizontalAlignment.Center;
				image.VerticalAlignment = VerticalAlignment.Center;
			}
			catch (System.Exception /*ex*/)
			{}

			try
			{
				imagePressed = new Image();
				imagePressed.Source = new BitmapImage(new System.Uri(imagePressedSource, System.UriKind.RelativeOrAbsolute));
				imagePressed.HorizontalAlignment = HorizontalAlignment.Center;
				imagePressed.VerticalAlignment = VerticalAlignment.Center;
			}
			catch (System.Exception /*ex*/)
			{}

			if (!String.IsNullOrEmpty(text))
			{
				textBlock = new TextBlock();
				textBlock.Text = text;
				textBlock.HorizontalAlignment = HorizontalAlignment.Center;
				textBlock.VerticalAlignment = VerticalAlignment.Center;
				if (textColor != null)
				{
					textBlock.Foreground = new SolidColorBrush(textColor);
				}
			}

			InitContent();
		}

		private void InitContent()
		{
			if (bgColor != null)
			{
				this.Background = new System.Windows.Media.SolidColorBrush(bgColor);
			}

			try
			{
				((Grid)this.Content).Children.Clear();
			}
			catch (System.Exception /*ex*/)
			{}

			Grid content = new Grid();
			content.Width = size.Width;
			content.Height = size.Height;

			if (state == STATE.FREE)
			{
				if (image != null)
				{
					image.Stretch = Stretch.Fill;
					content.Children.Add(image);
				}
			} 
			else if (state == STATE.PRESSED)
			{
				if (imagePressed != null)
				{
					imagePressed.Stretch = Stretch.Fill;
					content.Children.Add(imagePressed);
				}
			}

			if (textBlock != null)
			{
				content.Children.Add(textBlock);
			}

			if (content.Children.Count > 0)
			{
				this.Content = content;
			}
		}

		protected override void OnIsPressedChanged(DependencyPropertyChangedEventArgs e)
		{
			if (this.IsPressed)
			{
				state = STATE.PRESSED;
				
			}
			else
			{
				state = STATE.FREE;
			}

			InitContent();
		}
	}
}
