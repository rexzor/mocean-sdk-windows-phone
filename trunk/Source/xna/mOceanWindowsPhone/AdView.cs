/****
 * © 2010-2011 mOcean Mobile. A subsidiary of Mojiva, Inc. All Rights Reserved.
 * */

using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Phone.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using System.Diagnostics;

namespace mOceanWindowsPhone
{
	public class AdView
	{
		#region "Constants"
		protected enum AD_STATE
		{
			NULL,
			INIT,
			READY,
			UPDATE,
			SHOW,
			PRESSED,
			CLOSED
		}

		private const string SDK_VERSION = "2.2";
		private const string USER_AGENT = "Mozilla/4.0 (compatible; MSIE 7.0; Windows Phone OS 7.0; Trident/3.1; IEMobile/7.0)";
		private const string SETTINGS_IS_FIRST_APP_LAUNCH = "isFirstAppLaunch";
		private const string FIRST_APP_LAUNCH_URL = "http://www.moceanmobile.com/appconversion.php";
		private const string DEFAULT_AD_SERVER = "http://ads.mocean.mobi/ad";
		private const string CHECK_NEW_VERSION_URL = "http://www.moceanmoble.com/sdk_version.php?platform=wp7&version=" + SDK_VERSION;
		private const string NEW_VERSION_MESSAGE = "NEW VERSION MESSAGE";
		private const int DEFAULT_UPDATE_TIME = 120; // seconds
		private const string INVALID_PARAMETERS_RESPONSE = "<!-- invalid params -->";
		private const string INVALID_PARAMETERS_MESSAGE = "INVALID PARAMETERS MESSAGE";
		#endregion

		#region "Variables"
		internal static volatile int adViewsCount = 0;
		private int viewId = 0;
		protected Game game = null;

		private int site = 0;
		private int zone = 0;
		private string deviceId = null;
		private int advertiserId = 0;
		private string groupCode = null;
		protected Texture2D defaultImage = null;

		private bool firstLaunch = true;
		protected bool firstUpdate = true;

		private Thread checkNewVersionThread = null;

		private Thread firstAppLaunchThread = null;
		private Thread updateContentThread = null;
		private AdserverRequest adserverRequest = null;
		private HttpWebRequest currentAdHttpRequest = null;
		private event EventHandler<DataRequest.AdEventArgs> adRequestedEvent = null;
		private int requestId = 0;

		private Timer updateTimer = null;
		private int updateTime = DEFAULT_UPDATE_TIME;

		protected SpriteBatch spriteBatch = null;
		private string adHref = null;
		private Texture2D adImage = null;
		private Point position = Point.Zero;
		protected Rectangle adRect = Rectangle.Empty;
		protected volatile AD_STATE adState = AD_STATE.NULL;

		private ManualResetEvent deviceIdInited = new ManualResetEvent(false);
		
		private AdCampaignsManager adCampaignsManager = null;

		private Logger logger = null;
		private Logger.LogLevel logLevel = Logger.LogLevel.ErrorsOnly;

		protected TouchCollection touchCollection;
		#endregion

		#region "Public methods"
		public AdView(int site, int zone, Game game)
		{
			adState = AD_STATE.NULL;

			this.site = site;
			this.zone = zone;
			this.game = game;

			viewId = adViewsCount++;
			if (game != null)
			{
				spriteBatch = new SpriteBatch(game.GraphicsDevice);
				adCampaignsManager = new AdCampaignsManager(spriteBatch);
				adCampaignsManager.AddExCampaign += AddExCampaign;
			}

			adserverRequest = new AdserverRequest();
		}

		public void Update()
		{
			if (adState == AD_STATE.READY || adState == AD_STATE.SHOW || adState == AD_STATE.UPDATE)
			{
				adState = AD_STATE.UPDATE;

				if (firstLaunch)
				{
					firstAppLaunchThread = new Thread(new ThreadStart(FirstAppLaunchProc));
					firstAppLaunchThread.Name = "FirstAppLaunchThread";
					firstAppLaunchThread.Start();
				}

				updateContentThread = new Thread(new ThreadStart(UpdateAdThreadProc));
				updateContentThread.Name = "UpdateContentThreadProc";
				updateContentThread.Start();
			}
		}

		public virtual void Update(GameTime gameTime)
		{
			touchCollection = TouchPanel.GetState();

			UpdateAd();
			UpdateAdCampaignManager(gameTime);
		}

		public virtual void Draw(GameTime gameTime)
		{
			
			if (adState != AD_STATE.NULL && adState != AD_STATE.INIT)
			{
				spriteBatch.Begin();

				Vector2 spritePosition = new Vector2((float)adRect.X, (float)adRect.Y);
				switch (adState)
				{
					case AD_STATE.SHOW:
					case AD_STATE.PRESSED:
						spriteBatch.Draw(adImage, spritePosition, Color.White);
						break;
					case AD_STATE.CLOSED:
						break;
					default:
						if (defaultImage != null)
						{
							spriteBatch.Draw(defaultImage, spritePosition, Color.White);
						}
						break;
				}

				spriteBatch.End();
			}

			if (adCampaignsManager != null)
			{
				adCampaignsManager.Draw(gameTime);
			}
		}

		public Texture2D GetAdTexture()
		{
			return adImage;
		}
		#endregion

		#region "Protected methods"
		protected void UpdateAd()
		{
			switch (adState)
			{
				case AD_STATE.NULL:
					Init();
					break;
				case AD_STATE.READY:
					Update();
					break;
				case AD_STATE.SHOW:
					InputProcess();
					break;
				default:
					break;
			}
		}

		protected void UpdateAdCampaignManager(GameTime gameTime)
		{
			if (adCampaignsManager != null)
			{
				adCampaignsManager.Update(gameTime);
			}
		}
		#endregion

		#region "Private methods"
		~AdView()
		{
			game = null;
			logger = null;

			adCampaignsManager.AddExCampaign -= AddExCampaign;
			adCampaignsManager = null;

			adImage = null;
			defaultImage = null;
			adserverRequest = null;
			adRequestedEvent -= AdRequested;

			AutoDetectParameters.Release();

			if (currentAdHttpRequest != null)
			{
				currentAdHttpRequest = null;
			}

			if (updateContentThread != null)
			{
				updateContentThread.Join();
				updateContentThread = null;
			}
		}

		private void Init()
		{
			adState = AD_STATE.INIT;

			if (Microsoft.Devices.Environment.DeviceType == Microsoft.Devices.DeviceType.Emulator)
			{
				CheckNewVersion();
			}

			adserverRequest.SetAdserverURL(DEFAULT_AD_SERVER);
			adserverRequest.SetAdsType(3);
			adserverRequest.SetUserAgent(USER_AGENT);
			adserverRequest.SetDeviceId(deviceId);
			adserverRequest.SetVersion(SDK_VERSION);
			adserverRequest.SetSizeRequired(true);

			AutoDetectParameters.AddReferense();
			if (AutoDetectParameters.Instance.DeviceId == null)
			{
				AutoDetectParameters.Instance.LocationChanged += new AutoDetectParameters.LocationChangedEventHandler(GpsCoordinatesDetected);
				AutoDetectParameters.Instance.GetDeviceId += OnGetDeviceId;
				AutoDetectParameters.Instance.GpsSensorDisabled += GpsSensorDisabled;
			}
			else
			{
				OnGetDeviceId(this, EventArgs.Empty);
			}

			updateTimer = new Timer(new TimerCallback(UpdateTimerTick));
			adRequestedEvent += AdRequested;
		}

		private void CheckNewVersion()
		{
			checkNewVersionThread = new Thread(new ThreadStart(() =>
			{
				string result = DataRequest.RequestStringDataSync(CHECK_NEW_VERSION_URL);

				if (result != SDK_VERSION)
				{}
			}));

			checkNewVersionThread.Start();
		}

		private void OnGetDeviceId(object sender, EventArgs e)
		{
			AutoDetectParameters.Instance.GetDeviceId -= OnGetDeviceId;

			deviceId = AutoDetectParameters.Instance.DeviceId;
			adserverRequest.SetDeviceId(deviceId);
			deviceIdInited.Set();
			WriteLog(Logger.LogLevel.All, "AutoDetectParameters", Logger.UA_DETECTED, deviceId);

			adState = AD_STATE.READY;
		}

		private void GpsCoordinatesDetected(double latitude, double longitude, double accuracy)
		{
			WriteLog(Logger.LogLevel.All, "AutoDetectParameters", Logger.GPS_COORDINATES_DETECTED, latitude.ToString("F1") + "," + longitude.ToString("F1"));
		}

		private void FirstAppLaunchProc()
		{
			deviceIdInited.WaitOne();

			try
			{
				if (String.IsNullOrEmpty(deviceId))
				{
					try
					{
						deviceId = IsolatedStorageSettings.ApplicationSettings["DeviceID"].ToString();
					}
					catch (System.Exception)
					{
						deviceId = null;
					}

					if (String.IsNullOrEmpty(deviceId))
					{
						deviceId = Guid.NewGuid().ToString();
						IsolatedStorageSettings.ApplicationSettings["DeviceID"] = deviceId;
					}
				}

				if (IsolatedStorageSettings.ApplicationSettings.Contains(SETTINGS_IS_FIRST_APP_LAUNCH))
				{
					firstLaunch = false;
				}

				if (firstLaunch)
				{
					String request = String.Empty;

					if (!String.IsNullOrEmpty(deviceId) &&
						advertiserId > 0 &&
						!String.IsNullOrEmpty(groupCode))
					{
						request += "?udid=" + HttpUtility.UrlEncode(deviceId);
						request += "&advertiser_id=" + HttpUtility.UrlEncode(advertiserId.ToString());
						request += "&group_code=" + HttpUtility.UrlEncode(groupCode);
					}

					if (!String.IsNullOrEmpty(request))
					{
						DataRequest.SendGetRequest(FIRST_APP_LAUNCH_URL + request, String.Empty);
						IsolatedStorageSettings.ApplicationSettings[SETTINGS_IS_FIRST_APP_LAUNCH] = true;
						firstLaunch = false;
					}
				}
			}
			catch (System.Exception)
			{}

			try
			{
				IsolatedStorageSettings.ApplicationSettings.Save();
			}
			catch (System.Exception)
			{}
		}

		private void UpdateAdThreadProc()
		{
			deviceIdInited.WaitOne();

			PauseUpdateTimer();

			if (site > 0 && zone > 0)
			{
				adserverRequest.SetSite(site);
				adserverRequest.SetZone(zone);

				string url = adserverRequest.ToString();

				WriteLog(Logger.LogLevel.All, Logger.START_AD_DOWNLOAD, url);
				OnAdDownloadBegin();

				if (currentAdHttpRequest != null)
				{
					currentAdHttpRequest.Abort();
				}

				currentAdHttpRequest = DataRequest.RequestAdAsync(url, adRequestedEvent, ++requestId);
			}
		}

		private void AdRequested(object sender, DataRequest.AdEventArgs e)
		{
			if (e.AdRequestId == requestId)
			{
				WriteLog(Logger.LogLevel.All, Logger.GET_SERVER_RESPONSE, null);

				string adContent = e.Content;

				if (adContent == INVALID_PARAMETERS_RESPONSE)
				{
					OnAdDownloadError(INVALID_PARAMETERS_MESSAGE);
				}
				else if (e.Type == DataRequest.AD_TYPE.ERROR)
				{
					OnAdDownloadError(e.Content);
				}
				else
				{
					ShowAd(adContent);

					if (adCampaignsManager != null)
					{
						adCampaignsManager.TryAddView(adContent);
					}
				}
			}
		}

		private void PauseUpdateTimer()
		{
			updateTimer.Change(Timeout.Infinite, 0);
		}

		private void ResumeUpdateTimer()
		{
			if (updateTime == 0)
			{
				updateTimer.Change(Timeout.Infinite, Timeout.Infinite);
			}
			else
			{
				updateTimer.Change(updateTime * 1000, 0);
			}
		}

		private void ShowAd(string adContent)
		{
			string linkHref = null;
			string linkImgSrc = null;
			bool finded = TryFindImgLinks(adContent, out linkHref, out linkImgSrc);

			if (finded)
			{
				adHref = linkHref;

				bool adImageLoaded = false;
				try
				{
					using (IsolatedStorageFile appStorage = IsolatedStorageFile.GetUserStoreForApplication())
					{
						using (IsolatedStorageFileStream file = appStorage.OpenFile(linkImgSrc, FileMode.Open))
						{
							adImage = Texture2D.FromStream(spriteBatch.GraphicsDevice, file);
							OnAdDownloadEnd();
							adImageLoaded = true;
						}
					}
				}
				catch (System.Exception)
				{}

				if (!adImageLoaded)
				{
					try
					{
						byte[] imgBytes = DataRequest.RequestByteDataSync(linkImgSrc);
						MemoryStream imgMemStream = new MemoryStream();
						imgMemStream.Write(imgBytes, 0, imgBytes.Length);
						adImage = Texture2D.FromStream(spriteBatch.GraphicsDevice, imgMemStream);
						OnAdDownloadEnd();
					}
					catch (System.Exception)
					{ }
				}

				if (adImage != null)
				{
					adRect.Width = adImage.Width;
					adRect.Height = adImage.Height;

					defaultImage = adImage;
					adState = AD_STATE.SHOW;

					WriteLog(Logger.LogLevel.All, Logger.START_RENDER_AD, null);
					WriteLog(Logger.LogLevel.All, Logger.AD_DISPLAYED, null);
				}
			}

			ResumeUpdateTimer();
		}

		private bool TryFindImgLinks(string adContent, out string linkHref, out string linkImgSrc)
		{
			linkHref = null;
			linkImgSrc = null;

			string adPattern = "<a\\s*(.*?)>(.*?)</a>";
			Regex adRegex = new Regex(adPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
			MatchCollection adMatches = adRegex.Matches(adContent);
			foreach (Match currMatch in adMatches)
			{
				string linkAttrs = currMatch.Groups[1].Value;
				string linkContent = currMatch.Groups[2].Value;

				if (String.IsNullOrEmpty(linkHref))
				{
					linkHref = GetAttributeValue(linkAttrs, "href");
				}

				string imgPattern = "<img\\s*(.*?)>";
				Regex imgRegex = new Regex(imgPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
				MatchCollection imgMatches = imgRegex.Matches(linkContent);
				foreach (Match currImgMatch in imgMatches)
				{
					string imgAttrs = currImgMatch.Groups[1].Value;

					if (String.IsNullOrEmpty(linkImgSrc))
					{
						linkImgSrc = GetAttributeValue(imgAttrs, "src");
					}
				}

				if (!String.IsNullOrEmpty(linkHref) && !String.IsNullOrEmpty(linkImgSrc))
				{
					return true;
				}
				else
				{
					linkHref = null;
					linkImgSrc = null;
				}
			}

			return false;
		}

		private string GetAttributeValue(string strToParse, string attribute)
		{
			string attrPattern = "(?<attr>[^\\s]*)=[\"\'](?<value>[^\"\']*)[\"\']";
			Regex attrRegex = new Regex(attrPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
			MatchCollection attrMatches = attrRegex.Matches(strToParse);
			foreach (Match currAttrMatch in attrMatches)
			{
				if (currAttrMatch.Groups["attr"].Value == attribute)
				{
					return currAttrMatch.Groups["value"].Value;
				}
			}

			return null;
		}

		private void InputProcess()
		{
			foreach (TouchLocation touchLocation in touchCollection)
			{
				if (touchLocation.State == TouchLocationState.Pressed && adRect.Contains((int)touchLocation.Position.X, (int)touchLocation.Position.Y))
				{
					adState = AD_STATE.PRESSED;
					OnAdClick();
					return;
				}
				else
				{
					TouchLocation prevLocation;
					if (touchLocation.TryGetPreviousLocation(out prevLocation))
					{
						if (prevLocation.State == TouchLocationState.Pressed && adRect.Contains((int)prevLocation.Position.X, (int)prevLocation.Position.Y))
						{
							adState = AD_STATE.PRESSED;
							OnAdClick();
							return;
						}
					}
				}
			}
		}

		private void OnAdClick()
		{
			if (!String.IsNullOrEmpty(adHref))
			{
				PauseUpdateTimer();
				OnAdNavigateBanner(adHref);
				WebBrowserTask task = new WebBrowserTask();
				task.URL = adHref;
				task.Show();
			}
		}

		private void UpdateTimerTick(Object stateInfo)
		{
			Update();
		}

		private Color ColorFromRgb(String rgb)
		{
			if (rgb.Length >= 7)
			{
				try
				{
					byte r = byte.Parse(rgb.Substring(1, 2));
					byte g = byte.Parse(rgb.Substring(3, 2));
					byte b = byte.Parse(rgb.Substring(5, 2));
					return new Color(r, g, b, 255);
				}
				catch (Exception)
				{ }
			}
			return new Color(0, 0, 0, 0);
		}

		private String ColorToRgb(Color color)
		{
			string clr = color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
			return "#" + clr;
		}

		private void AddExCampaign(object sender, ExCampaignEventArgs e)
		{
			adserverRequest.AddExternalCampaign(e.CampaignId);
			Update();
		}

		private void GpsSensorDisabled(object sender, EventArgs e)
		{
			AutoDetectParameters.Instance.GpsSensorDisabled -= GpsSensorDisabled;
		}

		private void WriteLog(Logger.LogLevel logLevel, string message, string parameter)
		{
			WriteLog(logLevel, "AdView", message, parameter); ;
		}

		private void WriteLog(Logger.LogLevel logLevel, string className, string message, string parameter)
		{
#if DEBUG
			if (logger != null)
			{
 				if (this.logLevel >= logLevel)
 				{
					logger.WriteLine(logLevel, className, viewId, message, parameter);
 				}
			}
#endif
		}
		#endregion

		#region "Setting parameters"
		public void SetLocation(Point location)
		{
			adRect.Location = location;
		}
		public void SetSite(int site)
		{
			if (site > 0)
			{
				this.site = site;
				adserverRequest.SetSite(site);
			}
		}
		public void SetZone(int zone)
		{
			if (zone > 0)
			{
				this.zone = zone;
				adserverRequest.SetZone(zone);
			}
		}
		public void SetTest(bool test)
		{
			adserverRequest.SetTest(test);
		}
		public void SetPremium(int premium)
		{
			if (premium >= 0 && premium <= 2)
			{
				adserverRequest.SetPremium(premium);
			}
		}
		public void SetKeywords(string keywords)
		{
			if (!String.IsNullOrEmpty(keywords))
			{
				adserverRequest.SetKeywords(keywords);
			}
		}
		public void SetAdsType(int adsType)
		{
			if (adsType == 1 || adsType == 2 || adsType == 3 || adsType == 6)
			{
				adserverRequest.SetAdsType(adsType);
			}
		}
		public void SetMinSizeX(int size)
		{
			if (size > 0)
			{
				adserverRequest.SetMinSizeX(size);
			}
		}
		public void SetMinSizeY(int size)
		{
			if (size > 0)
			{
				adserverRequest.SetMinSizeY(size);
			}
		}
		public void SetMaxSizeX(int size)
		{
			if (size > 0)
			{
				adserverRequest.SetMaxSizeX(size);
			}
		}
		public void SetMaxSizeY(int size)
		{
			if (size > 0)
			{
				adserverRequest.SetMaxSizeY(size);
			}
		}
		public void SetBackgroundColor(Color backgroundColor)
		{
			adserverRequest.SetBackgroundColor(ColorToRgb(backgroundColor));
		}
		public void SetTextColor(Color textColor)
		{
			adserverRequest.SetTextColor(ColorToRgb(textColor));
		}
		public void SetCustomParameters(string customParameters)
		{
			if (!String.IsNullOrEmpty(customParameters))
			{
				adserverRequest.SetCustomParameters(customParameters);
			}
		}
		public void SetAdserverURL(string adServerUrl)
		{
			if (!String.IsNullOrEmpty(adServerUrl))
			{
				adserverRequest.SetAdserverURL(adServerUrl);
			}
		}
		public void SetDefaultImage(Texture2D defaultImage)
		{
			this.defaultImage = defaultImage;
		}
		public void SetAdvertiserId(int advertiserId)
		{
			if (advertiserId > 0)
			{
				this.advertiserId = advertiserId;
			}
		}
		public void SetGroupCode(string groupCode)
		{
			if (!String.IsNullOrEmpty(groupCode))
			{
				this.groupCode = groupCode;
			}
		}
		public void SetUpdateTime(int updateTime)
		{
			this.updateTime = updateTime;
		}
		public void SetLatitude(string latitude)
		{
			adserverRequest.SetLatitude(latitude);
		}
		public void SetLongitude(string longitude)
		{
			adserverRequest.SetLongitude(longitude);
		}
		public void SetCountry(string country)
		{
			if (!String.IsNullOrEmpty(country))
			{
				adserverRequest.SetCountry(country);
			}
		}
		public void SetRegion(string region)
		{
			if (!String.IsNullOrEmpty(region))
			{
				adserverRequest.SetRegion(region);
			}
		}
		public void SetCity(string city)
		{
			if (!String.IsNullOrEmpty(city))
			{
				adserverRequest.SetCity(city);
			}
		}
		public void SetArea(string area)
		{
			if (!String.IsNullOrEmpty(area))
			{
				adserverRequest.SetArea(area);
			}
		}
		public void SetMetro(string metro)
		{
			if (!String.IsNullOrEmpty(metro))
			{
				adserverRequest.SetMetro(metro);
			}
		}
		public void SetZip(string zip)
		{
			if (!String.IsNullOrEmpty(zip))
			{
				adserverRequest.SetZip(zip);
			}
		}
		public void SetCarrier(string carrier)
		{
			if (!String.IsNullOrEmpty(carrier))
			{
				adserverRequest.SetCarrier(carrier);
			}
		}
		public void SetLogger(Logger logger)
		{
			if (logger != null)
			{
				this.logger = logger;
			}
			else
			{
				WriteLog(Logger.LogLevel.ErrorsOnly, Logger.WRONG_PARAMETER, "logger");
			}
		}
		public void SetLogLevel(Logger.LogLevel logLevel)
		{
			this.logLevel = logLevel;
		}
		#endregion

		#region "Events"
		private event AdNavigateEventHandler AdNavigateBannerEvent;
		protected virtual bool OnAdNavigateBanner(string url)
		{
			if (AdNavigateBannerEvent != null)
			{
				try
				{
					return AdNavigateBannerEvent(url);
				}
				catch (System.Exception)
				{ }
			}

			return false;
		}
		public event AdNavigateEventHandler AdNavigateBanner
		{
			add { AdNavigateBannerEvent += value; }
			remove { AdNavigateBannerEvent -= value; }
		}

		private event EventHandler AdDownloadBeginEvent = null;
		protected virtual void OnAdDownloadBegin()
		{
			if (AdDownloadBeginEvent != null)
			{
				try
				{
					AdDownloadBeginEvent(this, EventArgs.Empty);
				}
				catch (System.Exception)
				{ }
			}
		}
		public event EventHandler AdDownloadBegin
		{
			add { AdDownloadBeginEvent += value; }
			remove { AdDownloadBeginEvent -= value; }
		}

		private event EventHandler AdDownloadEndEvent = null;
		protected virtual void OnAdDownloadEnd()
		{
			WriteLog(Logger.LogLevel.All, Logger.FINISH_AD_DOWNLOAD, null);
			if (AdDownloadEndEvent != null)
			{
				try
				{
					AdDownloadEndEvent(this, EventArgs.Empty);
				}
				catch (System.Exception)
				{ }
			}
		}
		public event EventHandler AdDownloadEnd
		{
			add { AdDownloadEndEvent += value; }
			remove { AdDownloadEndEvent -= value; }
		}

		private event AdDownloadErrorEventHandler AdDownloadErrorEvent = null;
		protected virtual void OnAdDownloadError(string error)
		{
			WriteLog(Logger.LogLevel.ErrorsOnly, Logger.FAIL_AD_DOWNLOAD, error);
			if (AdDownloadErrorEvent != null)
			{
				try
				{
					AdDownloadErrorEvent(error);
				}
				catch (System.Exception)
				{ }
			}
		}
		public event AdDownloadErrorEventHandler AdDownloadError
		{
			add { AdDownloadErrorEvent += value; }
			remove { AdDownloadErrorEvent -= value; }
		}
		#endregion
	}

	#region "Event Handlers"
	public delegate bool AdNavigateEventHandler(string url);
	public delegate void AdDownloadErrorEventHandler(string url);
	public delegate void AdWebViewClosingEventHandler(string url);
	#endregion
}
