/****
 * © 2010-2011 mOcean Mobile. A subsidiary of Mojiva, Inc. All Rights Reserved.
 * */

using System;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;

namespace mOceanWindowsPhone
{
	internal class AdViewer
	{
		#region "Variables"
		private WebBrowser browser = null;
		private string userAgent = String.Empty;
		private Uri uri = null;
		private string contentToLoad = null;
		private bool navigating = false;
		private bool enabled = true;
		private bool needLoadCompletedRaise = false;
		#endregion

		#region "Properties"
		public double Width
		{
			get { return browser.Width; }
			set { browser.Width = value; }
		}
		public double Height
		{
			get { return browser.Height; }
			set { browser.Height = value; }
		}
		public WebBrowser Control
		{
			get { return browser; }
			private set { browser = value; }
		}
		public bool Enabled
		{
			get { return enabled; }
			private set { enabled = value; }
		}
		#endregion

		#region "Public methods"
		public AdViewer()
		{
			browser = new WebBrowser();
			browser.Base = "";
			browser.IsScriptEnabled = true;
			browser.Visibility = Visibility.Collapsed;
			browser.Margin = new Thickness(0);
			browser.HorizontalAlignment = HorizontalAlignment.Left;
			browser.VerticalAlignment = VerticalAlignment.Top;
			browser.Loaded += new RoutedEventHandler(Browser_Loaded);
			browser.Navigating += new EventHandler<NavigatingEventArgs>(Browser_Navigating);
			browser.Navigated += new EventHandler<NavigationEventArgs>(Browser_Navigated);
			browser.LoadCompleted += new LoadCompletedEventHandler(Browser_LoadCompleted);
			browser.ScriptNotify += new EventHandler<NotifyEventArgs>(Browser_ScriptNotify);
		}

		public AdViewer(Uri uri)
			: this()
		{
			this.uri = uri;
		}

		public AdViewer(string contentToLoad)
			: this()
		{
			this.contentToLoad = contentToLoad;
		}

		public void ShowAd(string adContent)
		{
			if (browser.Visibility == Visibility.Collapsed)
			{
				browser.Visibility = Visibility.Visible;
			}

			navigating = true;
			try
			{
				browser.NavigateToString(adContent);
			}
			catch (System.Exception)
			{
				OnDisplayAdError();
			}
		}

		public void ShowCachedAd(string uri)
		{
			if (browser.Visibility == Visibility.Collapsed)
			{
				browser.Visibility = Visibility.Visible;
			}

			navigating = true;
			try
			{
				browser.Navigate(new Uri(uri, UriKind.Relative));
			}
			catch (System.Exception)
			{
				OnDisplayAdError();
			}
		}

		public void Navigate(Uri uri)
		{
			browser.Navigate(uri);
		}

		public void ExecuteScript(string script)
		{
			try
			{
				if (browser.Visibility == Visibility.Visible)
				{
					browser.InvokeScript("execScript", script);

					string s = browser.SaveToString();
				}
			}
			catch (System.Exception)
			{}
		}

		public void Disable()
		{
			enabled = false;
			browser.Visibility = Visibility.Collapsed;
		}

		public void Enable()
		{
			enabled = true;
			browser.Visibility = Visibility.Visible;
		}
		#endregion

		#region "Private methods"
		private void Browser_Loaded(object sender, RoutedEventArgs e)
		{
			if (String.IsNullOrEmpty(userAgent) && String.IsNullOrEmpty(contentToLoad) && uri == null)	// ad
			{
				StartInitUserAgent();
			}
			else		// for expanded
			{
				if (browser.Visibility == Visibility.Collapsed)
				{
					browser.Visibility = Visibility.Visible;
				}

				if (uri != null)
				{
					browser.Navigate(uri);
				}
				else if (!String.IsNullOrEmpty(contentToLoad))
				{
					ShowAd(contentToLoad);
				}
			}
		}

		private void StartInitUserAgent()
		{
			browser.NavigateToString("<html><head><script type=\"text/javascript\">function getUA(){return navigator.userAgent;}</script></head></html>");
		}

		private void Browser_Navigating(object sender, NavigatingEventArgs e)
		{
			if (!navigating)
			{
				if (NavigatingEvent != null)
				{
					NavigatingEvent(sender, e);
				}
			}
			else
			{
				navigating = false;
			}
		}

		private void Browser_Navigated(object sender, NavigationEventArgs e)
		{
			if (String.IsNullOrEmpty(userAgent))
			{
				try
				{
					userAgent = (String)browser.InvokeScript("getUA");
					if (UserAgentInitedEvent != null)
					{
						UserAgentInitedEvent(userAgent);
					}
				}
				catch (System.Exception)
				{}
			}
			else
			{}
		}

		private void Browser_LoadCompleted(object sender, NavigationEventArgs e)
		{
			if (needLoadCompletedRaise)
			{
				if (LoadCompletedEvent != null)
				{
					LoadCompletedEvent(this, e);
				}
			}
			else
			{
				needLoadCompletedRaise = true;
			}
		}

		private void Browser_ScriptNotify(object sender, NotifyEventArgs e)
		{
			if (enabled && ScriptNotifyEvent != null)
			{
				ScriptNotifyEvent(e.Value);
			}
		}
		#endregion

		#region "Events"
		public delegate void UserAgentInitedEventHandler(string userAgent);
		private event UserAgentInitedEventHandler UserAgentInitedEvent = null;
		public event UserAgentInitedEventHandler UserAgentInited
		{
			add { UserAgentInitedEvent += value; }
			remove { UserAgentInitedEvent -= value; }
		}

		private event EventHandler<NavigatingEventArgs> NavigatingEvent;
		public event EventHandler<NavigatingEventArgs> Navigating
		{
			add { NavigatingEvent += value; }
			remove { NavigatingEvent -= value; }
		}

		private event EventHandler LoadCompletedEvent = null;
		public event EventHandler LoadCompleted
		{
			add { LoadCompletedEvent += value; }
			remove { LoadCompletedEvent -= value; }
		}

		public delegate void ScriptNotifyEventHandler(string notify);
		private event ScriptNotifyEventHandler ScriptNotifyEvent = null;
		public event ScriptNotifyEventHandler ScriptNotify
		{
			add { ScriptNotifyEvent += value; }
			remove { ScriptNotifyEvent -= value; }
		}

		private event EventHandler DisplayAdErrorEvent = null;
		private void OnDisplayAdError()
		{
			if (DisplayAdErrorEvent != null)
			{
				DisplayAdErrorEvent(this, EventArgs.Empty);
			}
		}
		internal event EventHandler DisplayAdError
		{
			add { DisplayAdErrorEvent += value; }
			remove { DisplayAdErrorEvent -= value; }
		}
		#endregion
	}
}
