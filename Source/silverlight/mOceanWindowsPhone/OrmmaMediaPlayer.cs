/****
 * © 2010-2011 mOcean Mobile. A subsidiary of Mojiva, Inc. All Rights Reserved.
 * */

using System.Runtime.Serialization;
using System.Windows;
using Microsoft.Phone.Controls;
using System.Windows.Media;
using System.Windows.Controls;
using System;
using System.Diagnostics;
using System.Windows.Media.Imaging;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;

namespace mOceanWindowsPhone
{
	internal class OrmmaMediaPlayer : PopupWindow
	{
		#region "Variables"
		private Properties properties = null;
		private bool isFullScreen = true;
		#endregion

		#region "Properties"
		public new UIElement Window
		{
			get
			{
				if (!isFullScreen)
				{
					return control;
				}
				else
				{
					return null;
				}
			}
			private set { }
		}
		#endregion

		#region "Public methods"
		public OrmmaMediaPlayer(PhoneApplicationPage page, Size size, string propertiesString)
		{
			this.page = page;
			this.size = size;

			InitProperties(propertiesString);
			Init();
		}

		public override void Close()
		{
			Stop();
			base.Close();
		}

		public void Expand(PageOrientation orientation)
		{
			TransformGroup transformGroup = new TransformGroup();
			RotateTransform rotateTransform = new RotateTransform();
			TranslateTransform translateTransform = new TranslateTransform();

			switch (orientation)
			{
				case PageOrientation.Landscape:
				case PageOrientation.LandscapeLeft:
				case PageOrientation.LandscapeRight:
					if (orientation == PageOrientation.LandscapeLeft)
					{
						rotateTransform.Angle = 90;
						translateTransform.X = size.Width;
					}
					else if (orientation == PageOrientation.LandscapeRight)
					{
						rotateTransform.Angle = -90;
						translateTransform.Y = size.Height;
					}

					transformGroup.Children.Add(rotateTransform);
					transformGroup.Children.Add(translateTransform);

					popup.Child.RenderTransform = transformGroup;
					SetSize(new Size(size.Height, size.Width));
					break;
				case PageOrientation.Portrait:
				case PageOrientation.PortraitDown:
				case PageOrientation.PortraitUp:
				default:
					transformGroup.Children.Add(rotateTransform);
					transformGroup.Children.Add(translateTransform);

					popup.Child.RenderTransform = transformGroup;
					SetSize(size);
					break;
			}
		}

		public void Play(string url)
		{
			((OrmmaMediaPlayerControl)control).mediaElement.AutoPlay = false;
			((OrmmaMediaPlayerControl)control).mediaElement.Source = new Uri(url, UriKind.RelativeOrAbsolute);

			if (properties != null)
			{
				bool autoplay = (properties.autoplay == "autoplay");
				((OrmmaMediaPlayerControl)control).playPauseButton.IsChecked = autoplay;
				((OrmmaMediaPlayerControl)control).mediaElement.AutoPlay = autoplay;
				((OrmmaMediaPlayerControl)control).mediaElement.IsMuted = (properties.audio == "muted");

				if (!isFullScreen)
				{
					if (properties.position != null)
					{
						((OrmmaMediaPlayerControl)control).Margin = new Thickness(properties.position.left, properties.position.top, 0, 0);
					}

					((OrmmaMediaPlayerControl)control).HorizontalAlignment = HorizontalAlignment.Left;
					((OrmmaMediaPlayerControl)control).VerticalAlignment = VerticalAlignment.Top;
					((OrmmaMediaPlayerControl)control).Width = properties.width;
					((OrmmaMediaPlayerControl)control).Height = properties.height;
				}
			}

			if (isFullScreen)
			{
				Show();
			}
		}
		#endregion

		#region "Private methods"
		protected override void Init()
		{
			control = new OrmmaMediaPlayerControl();

			if (properties != null)
			{
				if (properties.startStyle == "fullscreen")
				{
					isFullScreen = true;
				}
				else
				{
					isFullScreen = false;
				}
			}

			((OrmmaMediaPlayerControl)control).mediaElement.MediaOpened += MediaElement_MediaOpened;
			((OrmmaMediaPlayerControl)control).mediaElement.MediaEnded += MediaElement_MediaEnded;
			((OrmmaMediaPlayerControl)control).mediaElement.MediaFailed += MediaElement_MediaFailed;
			((OrmmaMediaPlayerControl)control).mediaElement.CurrentStateChanged += new RoutedEventHandler(MediaElement_CurrentStateChanged);

			((OrmmaMediaPlayerControl)control).playPauseButton.Checked += new RoutedEventHandler(PlayPauseButton_Checked);
			((OrmmaMediaPlayerControl)control).playPauseButton.Unchecked += new RoutedEventHandler(PlayPauseButton_Unchecked);
			((OrmmaMediaPlayerControl)control).stopButton.Click += new RoutedEventHandler(StopButton_Click);

			if (isFullScreen)
			{
				BG_COLOR = Colors.Black;
				base.Init();

				if (page != null)
				{
					Expand(page.Orientation);
				}
			}
		}

		private void InitProperties(string propertiesString)
		{
			properties = null;

			try
			{
				using (var memoryStream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(propertiesString)))
				{
					var serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(Properties));
					properties = serializer.ReadObject(memoryStream) as Properties;
				}
			}
			catch (System.Exception)
			{
				properties = null;
			}
		}

		private void Pause()
		{
			try
			{
				((OrmmaMediaPlayerControl)control).mediaElement.Pause();
			}
			catch (System.Exception)
			{}
		}

		private void Play()
		{
			try
			{
				((OrmmaMediaPlayerControl)control).mediaElement.Play();
			}
			catch (System.Exception)
			{}
		}

		private void Stop()
		{
			try
			{
				((OrmmaMediaPlayerControl)control).mediaElement.Stop();
			}
			catch (System.Exception)
			{}
		}

		private void PlayPauseButton_Checked(object sender, RoutedEventArgs e)
		{
			Play();
		}

		private void PlayPauseButton_Unchecked(object sender, RoutedEventArgs e)
		{
			Pause();
		}

		private void StopButton_Click(object sender, RoutedEventArgs e)
		{
			((OrmmaMediaPlayerControl)control).playPauseButton.IsChecked = false;

			if (isFullScreen)
			{
				if (properties != null && properties.stopStyle == "exit")
				{
					Close();
				}
				else
				{
					Stop();
				}
			}
			else
			{
				if (properties != null && properties.stopStyle == "exit")
				{
					Stop();
					((Grid)control.Parent).Children.Remove(control);
				}
				else
				{
					Stop();
				}
			}
		}

		private void MediaElement_CurrentStateChanged(object sender, RoutedEventArgs e)
		{
			try
			{
				MediaElementState state = ((MediaElement)sender).CurrentState;
			}
			catch (System.Exception)
			{}

			((OrmmaMediaPlayerControl)control).ControlsContainer.IsHitTestVisible = true;
		}

		private void MediaElement_MediaOpened(object sender, RoutedEventArgs e)
		{
		}

		private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
		{
			if (properties != null)
			{
				if (properties.loop == "loop")
				{
					Play();
				}
				else if (properties.stopStyle == "exit")
				{
					if (isFullScreen)
					{
						Close();
					}
					else
					{
						Stop();
						try
						{
							((Grid)control.Parent).Children.Remove(control);
						}
						catch (System.Exception)
						{ }
					}
				}
			}
			else
			{
				if (isFullScreen)
				{
					Close();
				}
				else
				{
					Stop();
					try
					{
						((Grid)control.Parent).Children.Remove(control);
					}
					catch (System.Exception)
					{}
				}
			}
		}

		private void MediaElement_MediaFailed(object sender, RoutedEventArgs e)
		{
			try
			{
				((Grid)control.Parent).Children.Remove(control);
			}
			catch (System.Exception)
			{ }
			Close();
		}
		#endregion

		[DataContract]
		public class Properties
		{
			[DataMember]
			public string audio = null;

			[DataMember]
			public string autoplay = null;

			[DataMember]
			public string controls = null;

			[DataMember]
			public string loop = null;

			[DataMember]
			public string startStyle = null;

			[DataMember]
			public string stopStyle = null;

			[DataMember]
			public double width = 0;

			[DataMember]
			public double height = 0;

			[DataMember]
			public Position position = null;

			[DataContract]
			public class Position
			{
				[DataMember]
				public double top = 0;

				[DataMember]
				public double left = 0;

				public Position()
				{}
			}

			public Properties()
			{ }
		}
	}
}
