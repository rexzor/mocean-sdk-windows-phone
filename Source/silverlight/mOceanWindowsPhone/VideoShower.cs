/****
 * © 2010-2011 mOcean Mobile. A subsidiary of Mojiva, Inc. All Rights Reserved.
 * */

using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace mOceanWindowsPhone
{
	internal class VideoShower
	{
		#region "Constants"
		public enum PLAYING_STATE { READY, PLAYING, PAUSED }
		#endregion

		#region "Variables"
		private PLAYING_STATE state = PLAYING_STATE.READY;
		private MediaElement mediaElement = null;
		#endregion

		#region "Properties"
		public ManualResetEvent Ready = new ManualResetEvent(false);
		public ManualResetEvent Done = new ManualResetEvent(false);
		public PLAYING_STATE State
		{
			get { return state; }
			private set { state = value; }
		}
		public MediaElement Control
		{
			get { return mediaElement; }
			private set { mediaElement = value; }
		}
		public double Width
		{
			get { return mediaElement.Width; }
			set { mediaElement.Width = value; }
		}
		public double Height
		{
			get { return mediaElement.Height; }
			set { mediaElement.Height = value; }
		}
		public string Href
		{
			get;
			private set;
		}
		#endregion

		#region "Public methods"
		public VideoShower()
		{
			mediaElement = new MediaElement();
			mediaElement.Volume = 0.9;
			mediaElement.Visibility = Visibility.Collapsed;
			mediaElement.Stretch = Stretch.Fill;
			mediaElement.Loaded += new RoutedEventHandler(MediaElement_Loaded);
			mediaElement.MediaOpened += new RoutedEventHandler(MediaElement_MediaOpened);
			mediaElement.MediaEnded += new RoutedEventHandler(MediaElement_MediaEnded);
			mediaElement.MediaFailed += new EventHandler<ExceptionRoutedEventArgs>(MediaElement_MediaFailed);
			mediaElement.MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(MediaElement_MouseLeftButtonUp);
		}

		public void Play(string uriString, string href)
		{
			System.Diagnostics.Debug.WriteLine(uriString);

			Href = href;
			Deployment.Current.Dispatcher.BeginInvoke(() => {
				switch (state)
				{
					case PLAYING_STATE.READY:
						try
						{
							using (var appStorage = IsolatedStorageFile.GetUserStoreForApplication())
							{
								using (var file = new IsolatedStorageFileStream(uriString, FileMode.Open, appStorage))
								{
									mediaElement.SetSource(file);
								}
							}
						}
						catch (System.Exception)
						{
							Uri uri = new Uri(uriString, UriKind.RelativeOrAbsolute);
							mediaElement.Source = uri;
						}
						mediaElement.Play();
						break;
					case PLAYING_STATE.PLAYING:
						break;
					default:
						break;
				}
			});
		}

		public void Stop()
		{
			Deployment.Current.Dispatcher.BeginInvoke(() =>
			{
				mediaElement.Stop();
				State = PLAYING_STATE.READY;
			});
		}

		public void Pause()
		{
			Deployment.Current.Dispatcher.BeginInvoke(() =>
			{
				if (mediaElement.CanPause && mediaElement.CurrentState == MediaElementState.Playing)
				{
					mediaElement.Pause();
					State = PLAYING_STATE.PAUSED;
				}
			});
		}

		public void Resume()
		{
			Deployment.Current.Dispatcher.BeginInvoke(() =>
			{
				if (mediaElement.CurrentState == MediaElementState.Paused)
				{
					mediaElement.Play();
					State = PLAYING_STATE.PLAYING;
				}
			});
		}

		public void Hide()
		{
			Deployment.Current.Dispatcher.BeginInvoke(() =>
			{
				mediaElement.Visibility = Visibility.Collapsed;
				State = PLAYING_STATE.READY;
			});
		}
		#endregion

		#region "Private methods"
		private void MediaElement_Loaded(object sender, RoutedEventArgs e)
		{
			Ready.Set();
		}

		private void MediaElement_MediaOpened(object sender, RoutedEventArgs e)
		{
			State = PLAYING_STATE.PLAYING;
			mediaElement.Visibility = Visibility.Visible;
			OnOpened();
		}

		private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
		{
			mediaElement.Play();
		}

		private void MediaElement_MediaFailed(object sender, ExceptionRoutedEventArgs e)
		{
			mediaElement.Visibility = Visibility.Collapsed;
		}

		private void MediaElement_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			Pause();
			OnClick();
		}
		#endregion

		#region "Events"
		private event EventHandler OpenedEvent = null;
		protected virtual void OnOpened()
		{
			if (OpenedEvent != null)
			{
				OpenedEvent(this, EventArgs.Empty);
			}
		}
		public event EventHandler Opened
		{
			add { OpenedEvent += value; }
			remove { OpenedEvent -= value; }
		}

		private event EventHandler ClickEvent;
		protected virtual void OnClick()
		{
			if (ClickEvent != null)
			{
				ClickEvent(this, EventArgs.Empty);
			}
		}
		public event EventHandler Click
		{
			add { ClickEvent += value; }
			remove { ClickEvent -= value; }
		}
		#endregion
	}
}
