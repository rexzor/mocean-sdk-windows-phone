/****
 * © 2010-2011 mOcean Mobile. A subsidiary of Mojiva, Inc. All Rights Reserved.
 * */

using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.Info;
using System.IO.IsolatedStorage;
using System.Net;

namespace MojivaPhone
{
	public delegate void FireEventDelegate(string eventName, string[] args);

	public class AdView
	{
		private enum AD_STATE
		{
			INIT,
			DOWNLOAD_PAGE,
			DOWNLOAD_IMAGE,
			READY,
			PRESSED,
			NULL
		}

		private const String SETTINGS_IS_FIRST_APP_LAUNCH = "isFirstAppLaunch";
		private const String FIRST_APP_LAUNCH_URL = "http://www.moceanmobile.com/appconversion.php";
		private const int AD_UPDATE_PERIOD = 120000;

		private const int DEFAULT_SITE_ID = 8061;
		private const int DEFAULT_ZONE_ID = 20249;

		#region "Variables"
		private string site = "8061";
		private string zone = "20249";

		private int key = 1;
		private int adstype = 2;
		private int count = 1;
		private string version = "2.0";
		private bool sizeRequired = false;

		private bool test = false;
		private string logging;
		private int updateTime = AD_UPDATE_PERIOD;
		private int premium;
		private string keywords;
		private int minSizeX = 0, minSizeY = 0, maxSizeX = 0, maxSizeY = 0;

		private string backgroundColor;// paramBG;
		private string textColor;// paramLINK;
		private string adServerUrl = "http://ads.mocean.mobi/ad";
		private string latitude;
		private string longitude;
		private string country;
		private string region;
		private string city;
		private string area;
		private string metro;
		private string zip;
		private string userAgent;
		private string carrier;
		private string advertiserId;
		private string groupCode;
		private string customParameters;

		private Game game;
		private Thread contentThread = null;
		private SpriteBatch spriteBatch = null;
		private string adHref = null;
		private Texture2D adImage = null;
		private Texture2D defaultImage = null;
		private Point position = Point.Zero;
		private Rectangle adRect = Rectangle.Empty;
		private Timer updateTimer = null;
		private volatile AD_STATE adState = AD_STATE.NULL;
		#endregion

		#region "Public methods"
		public AdView(int site, int zone)
		{
			Site = site;
			Zone = zone;
		}

		public AdView(int site, int zone, Game game)
			: this(site, zone)
		{
			this.game = game;
		}

		public void Run()
		{
			adState = AD_STATE.INIT;
			Init();

			try
			{
				spriteBatch = new SpriteBatch(game.GraphicsDevice);
			}
			catch (System.Exception /*ex*/)
			{
				return;
			}

			Thread firstAppLaunchThread = new Thread(new ThreadStart(FirstAppLaunchProc));
			firstAppLaunchThread.Start();

			contentThread = new Thread(new ThreadStart(ContentThreadProc));
			contentThread.Start();
		}

		public void Update(GameTime gameTime)
		{
			switch (adState)
			{
			case AD_STATE.READY:
				InputProcess();
				break;
			}

			CThirdPartyManager.Update(gameTime);
		}

		public void Draw(GameTime gameTime)
		{
			if (adState != AD_STATE.NULL && adState != AD_STATE.INIT)
			{
				spriteBatch.Begin();

				Vector2 spritePosition = new Vector2((float)adRect.X, (float)adRect.Y);
				switch (adState)
				{
					case AD_STATE.READY:
					case AD_STATE.PRESSED:
						spriteBatch.Draw(adImage, spritePosition, Color.White);
						break;
					default:
						if (defaultImage != null)
						{
							spriteBatch.Draw(defaultImage, spritePosition, Color.White);
						}
						break;
				}

				CThirdPartyManager.Draw(gameTime);

				spriteBatch.End();
			}
		}

		~AdView()
		{
			try
			{
				contentThread.Abort();
				contentThread.Join();
			}
			catch (System.Exception /*ex*/)
			{ }
			finally
			{
				contentThread = null;
			}

			AdserverRequest.Release();
			CThirdPartyManager.Release();
			System.Diagnostics.Debug.WriteLine("~AdView !!! ");
		}
		#endregion

		#region "Private methods"
		private void Init()
		{
			updateTimer = new Timer(new TimerCallback(UpdateTimerTick));
		}

		private void ContentThreadProc()
		{
			InitAdServerRequest();
			string url = AdserverRequest.Instance.createURL();
			System.Diagnostics.Debug.WriteLine("url:" + url);

			adState = AD_STATE.DOWNLOAD_PAGE;
			OnAdDownloadBegin();
			string adContent = DataRequest.ReadStringData(url);
			//string adContent = "<a href=\"proof\"><img src=\"http://www.google.ru/intl/ru_ALL/images/logos/images_logo_lg.gif\"></a><a href=\"href_page2\"><img src=\"picture2\"></a>";

/*
			adContent = "<!-- client_side_external_campaign " +
						"<external_campaign version=\"1.0\"> " +
							"<campaign_id>17413</campaign_id> " +
							"<type>Millennial</type> " +
							"<external_params> " +
								"<param name=\"id\">36672</param> " +
								"<param name=\"adType\">MMBannerAdTop</param> " +
								"<param name=\"zip\">424038</param> " +
								"<param name=\"long\">37.6156</param> " +
								"<param name=\"lat\">55.752197</param> " +
								"<param name=\"education\">PhD</param> " +
								"<param name=\"ethnicity\">Japanese</param> " +
								"<param name=\"marital\">Single</param> " +
							"</external_params> " +
							"<track_url>http://ads1.mocean.mobi/img/13fca890-3a68-11e0-ae75-001d096a03fe</track_url> " +
						"</external_campaign> " +
						"--> ";
//*/

			if (!String.IsNullOrEmpty(adContent))
			{
				OnAdDownloadEnd();

				if (CThirdPartyManager.ContainExternalCampaign(adContent))
				{
					CThirdPartyManager.Create(game.GraphicsDevice, new Vector2(adRect.X, adRect.Y), adContent);
				}

				adState = AD_STATE.DOWNLOAD_IMAGE;
				string linkHref = null;
				string linkImgSrc = null;
				bool finded = TryFindImgLinks(adContent, out linkHref, out linkImgSrc);

				if (finded)
				{
					System.Diagnostics.Debug.WriteLine("linkHref: " + linkHref);
					System.Diagnostics.Debug.WriteLine("linkImgSrc: " + linkImgSrc);
					adHref = linkHref;

					try
					{
						byte[] imgBytes = DataRequest.ReadByteData(linkImgSrc);
						MemoryStream imgMemStream = new MemoryStream();
						imgMemStream.Write(imgBytes, 0, imgBytes.Length);
						adImage = Texture2D.FromStream(spriteBatch.GraphicsDevice, imgMemStream);

						adRect.Width = adImage.Width;
						adRect.Height = adImage.Height;

						defaultImage = adImage;

						adState = AD_STATE.READY;
					}
					catch (System.Exception /*ex*/)
					{ }
				}
			}
			else
			{
				OnAdDownloadError("error downloading ad");
				updateTime = Timeout.Infinite;
			}
			SetUpdateTimer(updateTime);
		}

		private void FirstAppLaunchProc()
		{

			if (!String.IsNullOrEmpty(advertiserId) &&
				!String.IsNullOrEmpty(groupCode))
			{
				try
				{
					object propertyValue = null;

					String deviceuid = (DeviceExtendedProperties.TryGetValue("DeviceUniqueID", out propertyValue) ? (propertyValue as String) : String.Empty);

					if (String.IsNullOrEmpty(deviceuid))
					{
						try
						{
							deviceuid = IsolatedStorageSettings.ApplicationSettings["DeviceID"].ToString();
						}
						catch (System.Exception /*ex*/)
						{
							deviceuid = null;
						}

						if (String.IsNullOrEmpty(deviceuid))
						{
							deviceuid = Guid.NewGuid().ToString();
							IsolatedStorageSettings.ApplicationSettings["DeviceID"] = deviceuid;
						}
					}

					bool isFirstAppLaunch = true;
					if (IsolatedStorageSettings.ApplicationSettings.Contains(SETTINGS_IS_FIRST_APP_LAUNCH))
					{
						isFirstAppLaunch = false;
					}

					if (isFirstAppLaunch)
					{
						String request = String.Empty;

						if (!String.IsNullOrEmpty(deviceuid))
						{
							string udid = MD5Core.GetHashString(deviceuid);
							request += "?udid=" + HttpUtility.UrlEncode(udid);
							request += "&advertiser_id=" + HttpUtility.UrlEncode(advertiserId);
							request += "&group_code=" + HttpUtility.UrlEncode(groupCode);
						}

						string url = FIRST_APP_LAUNCH_URL + request;
						DataRequest.SendGetRequest(url, String.Empty);

						IsolatedStorageSettings.ApplicationSettings[SETTINGS_IS_FIRST_APP_LAUNCH] = true;
					}
				}
				catch (System.Exception /*ex*/)
				{ }
			}
		}

		private void InitAdServerRequest()
		{
			AdserverRequest.Instance.setSite(site);
			AdserverRequest.Instance.setUa(userAgent);
			AdserverRequest.Instance.setKeywords(keywords);
			AdserverRequest.Instance.setPremium(Convert.ToInt32(premium));
			AdserverRequest.Instance.setZone(zone);
			AdserverRequest.Instance.setTestModeEnabled(test);
			AdserverRequest.Instance.setCount(count);
			AdserverRequest.Instance.setCountry(country);
			AdserverRequest.Instance.setRegion(region);
			AdserverRequest.Instance.setCity(city);
			AdserverRequest.Instance.setArea(area);
			AdserverRequest.Instance.setMetro(metro);
			AdserverRequest.Instance.setZip(zip);
			AdserverRequest.Instance.SizeRequired = sizeRequired;
			AdserverRequest.Instance.setAdstype(Convert.ToInt32(adstype));
			AdserverRequest.Instance.setLatitude(latitude);
			AdserverRequest.Instance.setLongitude(longitude);
 			AdserverRequest.Instance.setParamBG(backgroundColor);
 			AdserverRequest.Instance.setParamLINK(textColor);
			AdserverRequest.Instance.setCarrier(carrier);
			AdserverRequest.Instance.setImageMinWidth(minSizeX);
			AdserverRequest.Instance.setImageMinHeight(minSizeY);
			AdserverRequest.Instance.setImageMaxWidth(maxSizeX);
			AdserverRequest.Instance.setImageMaxHeight(maxSizeY);
			AdserverRequest.Instance.setAdserverURL(adServerUrl);
			AdserverRequest.Instance.setOutputFormat((int)key);
			AdserverRequest.Instance.SetCustomParameters(customParameters);
			AdserverRequest.Instance.setVersion(version);
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
			TouchCollection touchCollection = TouchPanel.GetState();
			foreach (TouchLocation touchLocation in touchCollection)
			{
				if (touchLocation.State != TouchLocationState.Invalid)
				{
					if (adRect.Contains(new Point((int)touchLocation.Position.X, (int)touchLocation.Position.Y)))
					{
						//System.Diagnostics.Debug.WriteLine("ACHTUNG !!! " + touchLocation.Position);
						adState = AD_STATE.PRESSED;
						OnAdClick();
					}
				}
			}
		}

		private void OnAdClick()
		{
			if (!String.IsNullOrEmpty(adHref))
			{
				OnAdNavigateBanner(adHref);
				WebBrowserTask task = new WebBrowserTask();
				task.URL = adHref;
				task.Show();
			}
		}

		private void SetUpdateTimer(int updateTime)
		{
			updateTimer.Change(updateTime, updateTime);
		}

		private void UpdateTimerTick(Object stateInfo)
		{
			contentThread = new Thread(new ThreadStart(ContentThreadProc));
			contentThread.Start();
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
		#endregion

		#region "Properties"

		/// <summary>
		/// Game object that contain AdView, required
		/// </summary>
		public Game Game
		{
			get { return game; }
			set { game = value; }
		}

		/// <summary>
		/// AdView Locaion
		/// </summary>
		public virtual Point Location
		{
			get { return adRect.Location; }
			set { adRect.Location = value; }
		}

		/// <summary>
		/// The id of the publisher site.
		/// </summary>
		public int Site
		{
			get
			{
				try
				{
					return Int32.Parse(site);
				}
				catch (System.Exception /*ex*/)
				{}
				return DEFAULT_SITE_ID;
			}
			set { site = value.ToString(); }
		}

		/// <summary>
		/// The id of the zone of publisher site.
		/// </summary>
		public int Zone
		{
			get
			{
				try
				{
					return Int32.Parse(zone);
				}
				catch (System.Exception /*ex*/)
				{ }
				return DEFAULT_ZONE_ID;
			}
			set { zone = value.ToString(); }
		}

		/// <summary>
		/// To retrieve test ads/for testing
		/// </summary>
		public bool Test
		{
			get { return test; }
			set { test = value; }
		}

		public string Logging
		{
			get { return logging; }
			set { logging = value; }
		}

		public int UpdateTime
		{
			get
			{
				return updateTime/1000;
			}
			set
			{
				if (value == 0)
				{
					updateTime = Timeout.Infinite;
				} 
				else
				{
					updateTime = value*1000;
				}
			}
		}

		public int Premium
		{
			get { return premium; }
			set { premium = value; }
		}

		public string Keywords
		{
			get { return keywords; }
			set { keywords = value; }
		}

		public int MinSizeX
		{
			get { return minSizeX; }
			set { minSizeX = value; }
		}

		public int MinSizeY
		{
			get { return minSizeY; }
			set { minSizeY = value; }
		}

		public int MaxSizeX
		{
			get { return maxSizeX; }
			set { maxSizeX = value; }
		}

		public int MaxSizeY
		{
			get { return maxSizeY; }
			set { maxSizeY = value; }
		}

		public Color BackgroundColor
		{
			get { return ColorFromRgb(backgroundColor); }
			set { backgroundColor = ColorToRgb(value); }
		}

		public Color TextColor
		{
			get { return ColorFromRgb(textColor); }
			set { textColor = ColorToRgb(value); }
		}

		public string AdServerUrl
		{
			get { return adServerUrl; }
			set { adServerUrl = value; }
		}
		public string Latitude
		{
			get { return latitude; }
			set { latitude = value; }
		}
		public string Longitude
		{
			get { return longitude; }
			set { longitude = value; }
		}
		public string Country
		{
			get { return country; }
			set { country = value; }
		}
		public string Region
		{
			get { return region; }
			set { region = value; }
		}
		public string City
		{
			get { return city; }
			set { city = value; }
		}
		public string Area
		{
			get { return area; }
			set { area = value; }
		}
		public string Metro
		{
			get { return metro; }
			set { metro = value; }
		}
		public string Zip
		{
			get { return zip; }
			set { zip = value; }
		}
		public string UserAgent
		{
			get { return userAgent; }
			set { userAgent = value; }
		}
		public string Carrier
		{
			get { return carrier; }
			set { carrier = value; }
		}
		public string AdvertiserId
		{
			get { return advertiserId; }
			set { advertiserId = value; }
		}
		public string GroupCode
		{
			get { return groupCode; }
			set { groupCode = value; }
		}
		public string CustomParameters
		{
			get { return customParameters; }
			set { customParameters = value; }
		}
		public Texture2D DefaultImage
		{
			get { return defaultImage; }
			set { defaultImage = value; }
		}
		#endregion

		#region "Events"
		private event EventHandler AdDownloadBeginEvent;
		private event EventHandler AdDownloadEndEvent;
		private event EventHandler<StringEventArgs> AdDownloadErrorEvent;
		private event EventHandler<StringEventArgs> AdNavigateBannerEvent;

		private void OnAdDownloadBegin()
		{
			if (AdDownloadBeginEvent != null)
				AdDownloadBeginEvent(this, EventArgs.Empty);
		}
		private void OnAdDownloadEnd()
		{
			if (AdDownloadEndEvent != null)
				AdDownloadEndEvent(this, EventArgs.Empty);
		}
		private void OnAdDownloadError(string error)
		{
			if (AdDownloadErrorEvent != null)
			{
				AdDownloadErrorEvent(this, new StringEventArgs(error));
			}
		}
		private void OnAdNavigateBanner(string url)
		{
			if (AdNavigateBannerEvent != null)
			{
				AdNavigateBannerEvent(this, new StringEventArgs(url));
			}
		}

		/// <summary>
		/// This event fired after user clicks the banner (only if InternalBrowser property has been set to True).
		/// </summary>
		public event EventHandler<StringEventArgs> AdNavigateBanner
		{
			add
			{
				AdNavigateBannerEvent += value;
			}
			remove
			{
				AdNavigateBannerEvent -= value;
			}
		}

		/// <summary>
		/// Event that is raised when the control starts AD download
		/// </summary>
		public event EventHandler AdDownloadBegin
		{
			add { AdDownloadBeginEvent += value; }
			remove { AdDownloadBeginEvent -= value; }
		}

		/// <summary>
		/// Event that is raised when the control finished AD download
		/// </summary>
		public event EventHandler AdDownloadEnd
		{
			add { AdDownloadEndEvent += value; }
			remove { AdDownloadEndEvent -= value; }
		}

		/// <summary>
		/// This event is fired after fail to download content.
		/// </summary>
		public event EventHandler<StringEventArgs> AdDownloadError
		{
			add { AdDownloadErrorEvent += value; }
			remove { AdDownloadErrorEvent -= value; }
		}
		#endregion
	}
}
