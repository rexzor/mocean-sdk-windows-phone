/****
 * © 2010-2011 mOcean Mobile. A subsidiary of Mojiva, Inc. All Rights Reserved.
 * */

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace MojivaPhone
{
	internal class CVideoShower
	{
		private const string VIDEO_TAG = "video";
		private const int DEFAULT_VIDEO_WIDTH = 480;
		private const int DEFAULT_VIDEO_HEIGHT = 800;

		private System.Windows.Controls.Primitives.Popup popup = null;
		private MediaElement mediaElement = new MediaElement();
		private bool inited = false;
		private string navigateUri = String.Empty;
		private event EventHandler<StringEventArgs> AdNavigateEvent;
		public event EventHandler<StringEventArgs> AdNavigate
		{
			add { AdNavigateEvent += value; }
			remove { AdNavigateEvent -= value; }
		}

		private class CVideo
		{
			public string src;
			public int width;
			public int height;
			public string href;

			public CVideo()
			{
				src = String.Empty;
				width = DEFAULT_VIDEO_WIDTH;
				height = DEFAULT_VIDEO_HEIGHT;
				href = String.Empty;
			}
		}
		private List<CVideo> videoLinks = new List<CVideo>();

		public CVideoShower()
		{
		}

		private void Init()
		{
			popup = new System.Windows.Controls.Primitives.Popup();
			popup.Width = DEFAULT_VIDEO_WIDTH;
			popup.Height = DEFAULT_VIDEO_HEIGHT;
			popup.HorizontalOffset = 0;
			popup.VerticalOffset = 0;

			mediaElement.Width = DEFAULT_VIDEO_WIDTH;
			mediaElement.Height = DEFAULT_VIDEO_HEIGHT;
			mediaElement.MediaOpened += new System.Windows.RoutedEventHandler(MediaElement_MediaOpened);
			mediaElement.MediaEnded += new System.Windows.RoutedEventHandler(MediaElement_MediaEnded);
			mediaElement.MediaFailed += new EventHandler<System.Windows.ExceptionRoutedEventArgs>(MediaElement_MediaFailed);

			Grid g = new Grid();
			g.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 0, 0));
			g.Children.Add(mediaElement);
			popup.Child = g;

			inited = true;
		}

		private void MediaElement_MediaOpened(object sender, System.Windows.RoutedEventArgs e)
		{
			if (videoLinks.Count > 0)
			{
				popup.Width = mediaElement.NaturalVideoWidth;
				popup.Height = mediaElement.NaturalVideoHeight;

				((Grid)popup.Child).Width = mediaElement.NaturalVideoWidth;
				((Grid)popup.Child).Height = mediaElement.NaturalVideoHeight;

				mediaElement.Width = mediaElement.NaturalVideoWidth;
				mediaElement.Height = mediaElement.NaturalVideoHeight;

 				popup.Visibility = System.Windows.Visibility.Visible;
				popup.Child.Visibility = System.Windows.Visibility.Visible;
			}
		}

		private void Hide()
		{
			if (inited)
			{
				popup.IsOpen = false;
			}
		}

		private void MediaElement_MediaEnded(object sender, System.Windows.RoutedEventArgs e)
		{
			if (videoLinks.Count > 0)
			{
				videoLinks.RemoveAt(0);
				System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() => ShowNextVideo());
			}
		}

		private void MediaElement_MediaFailed(object sender, System.Windows.ExceptionRoutedEventArgs e)
		{
			if (inited)
			{
				popup.IsOpen = false;
			}
		}

		public void SearchPage(string pageContent)
		{
			videoLinks.Clear();

			Dictionary<string, string> parsedPage = ParseHTMLTag("a", pageContent);

			string href = String.Empty;
			string content = String.Empty;

/*
			if (parsedPage.ContainsKey("href"))
			{
				href = parsedPage["href"];
			}
			if (parsedPage.ContainsKey("content"))
			{
				content = parsedPage["content"];
			}

			Dictionary<string, string> parsedContent = ParseHTMLTag(VIDEO_TAG, content);
			string src = String.Empty;
			if (parsedContent.ContainsKey("src"))
			{
				src = parsedContent["src"];
			}

			CVideo newVideo = new CVideo();
			newVideo.src = src;
			newVideo.href = href;
			videoLinks.Add(newVideo);

			System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() => ShowNextVideo());
//*/
		}

		private Dictionary<string, string> ParseHTMLTag(string tagName, string tag)
		{
			var result = new Dictionary<string, string>();

			string pagePattern = String.Format("<{0}\\s*(.*?)>(.*?)</{0}>", tagName);
			Regex pageRegex = new Regex(pagePattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
			MatchCollection pageMatches = pageRegex.Matches(tag);
			foreach (Match currMatch in pageMatches)
			{
 				string parameters = currMatch.Groups[1].Value;
 				string content = currMatch.Groups[2].Value;
// 				result.Add("content", content);

// 				string attrPattern = "(?<attr>[^\\s]*)=[\"\'](?<value>[^\"\']*)[\"\']";
// 				Regex attrRegex = new Regex(attrPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
// 				MatchCollection attrMatches = attrRegex.Matches(parameters);
// 				foreach (Match currAttrMatch in attrMatches)
// 				{
// 					string attr = currAttrMatch.Groups["attr"].Value;
// 					string value = currAttrMatch.Groups["value"].Value;
// 
// 					result.Add(attr, value);
// 				}
			}

			return result;
		}

		private void ShowNextVideo()
		{
			if (videoLinks.Count > 0)
			{
				if (!inited)
				{
					Init();
				}

				string currVideo = videoLinks[0].src;

				Uri uri = new Uri(videoLinks[0].src, UriKind.RelativeOrAbsolute);
				mediaElement.Source = uri;
				mediaElement.Play();

				popup.Child.Visibility = System.Windows.Visibility.Collapsed;
				popup.Visibility = System.Windows.Visibility.Collapsed;
				popup.IsOpen = true;
			}
			else
			{
				Hide();
			}
		}

		private void OnNavigateTo(string uri)
		{
			Hide();

			if (AdNavigateEvent != null)
			{
				AdNavigateEvent(this, new StringEventArgs(uri));
			}
		}
	}
}
