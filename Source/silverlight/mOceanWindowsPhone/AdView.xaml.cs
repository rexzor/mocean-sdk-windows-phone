/****
 * © 2010-2011 mOcean Mobile. A subsidiary of Mojiva, Inc. All Rights Reserved.
 * */

using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;

namespace mOceanWindowsPhone
{
	public partial class AdView : UserControl
	{
		#region "Constants"
		private const string SDK_VERSION = "2.2";
		private const string SETTINGS_IS_FIRST_APP_LAUNCH = "isFirstAppLaunch";
		private const string FIRST_APP_LAUNCH_URL = "http://www.moceanmobile.com/appconversion.php";
		private const string DEFAULT_AD_SERVER = "http://ads.mocean.mobi/ad";
		private const string CHECK_NEW_VERSION_URL = "http://www.moceanmobile.com/sdk_version.php?platform=wp7&version=" + SDK_VERSION;
		private const string NEW_VERSION_MESSAGE = "NEW VERSION MESSAGE";
		private const int DEFAULT_UPDATE_TIME = 120; // seconds
		private const string NO_SITE_ZONE_PARAMETERS_MESSAGE = "NO SITE ZONE PARAMETERS MESSAGE";
		private const string INVALID_PARAMETERS_RESPONSE = "<!-- invalid params -->";
		private const string INVALID_PARAMETERS_MESSAGE = "INVALID PARAMETERS MESSAGE";
		private const string EMPTY_CONTENT_MESSAGE = "Empty ad content";
		#endregion

		#region "Variables"
		internal static volatile int adViewsCount = 0;
		private int viewId = 0;
		private int site = 0;
		private int zone = 0;
		private string userAgent = null;
		private string deviceId = null;
		private int advertiserId = 0;
		private string groupCode = null;
		private Image defaultImage = null;

		private AdViewer adViewer = new AdViewer();
		private AdViewer expandedViewer = new AdViewer();
		private ManualResetEvent userAgentInited = new ManualResetEvent(false);
		private ManualResetEvent deviceIdInited = new ManualResetEvent(false);

		private bool firstLaunch = true;
		protected bool firstUpdate = true;

		private Thread checkNewVersionThread = null;

		private Thread firstAppLaunchThread = null;
		private Thread updateContentThread = null;
		private AdserverRequest adserverRequest = new AdserverRequest();
		private HttpWebRequest currentAdHttpRequest = null;
		private event EventHandler<DataRequest.AdEventArgs> adRequestedEvent = null;
		private int requestId = 0;

		private Timer updateTimer = null;
		private int updateTime = DEFAULT_UPDATE_TIME;
		private bool internalBrowser = false;
		private volatile string prevAdContent = String.Empty;
		private volatile string newAdContent = String.Empty;

		protected Point adPosition = new Point(0, 0);
		protected Size adSize = new Size(0, 0);
		internal string metaTags = String.Empty;

		private Size screenSize = new Size(0, 0);
		private Point absolutePosition = new Point(0, 0);

		private InternalBrowser internalBrowserControl = null;
		private VideoShower videoShower = new VideoShower();

		private OrmmaController ormmaController = null;
		private OrmmaMap ormmaMap = null;
		private OrmmaMediaPlayer ormmaMediaPlayer = null;
		

		protected PhoneApplicationPage ownerPage = null;
		private SupportedPageOrientation supportedPageOrientation = SupportedPageOrientation.PortraitOrLandscape;

		private Logger logger = null;
		private Logger.LogLevel logLevel = Logger.LogLevel.ErrorsOnly;
		#endregion

		#region "Public methods"
		public AdView()
		{
			viewId = adViewsCount++;
			InitializeComponent();
			//Init();

			if (Microsoft.Devices.Environment.DeviceType == Microsoft.Devices.DeviceType.Emulator)
			{
				CheckNewVersion();
			}
		}

		public AdView(int site, int zone) : this()
		{
			SetSite(site);
			SetZone(zone);
		}

		public void Update()
		{
			if (firstLaunch)
			{
				firstAppLaunchThread = new Thread(new ThreadStart(FirstAppLaunchProc));
				firstAppLaunchThread.Name = "FirstAppLaunchThreadProc";
				firstAppLaunchThread.Start();
			}

			if (firstUpdate)
			{
				firstUpdate = false;

				if (internalBrowser && internalBrowserControl == null)
				{
					internalBrowserControl = new InternalBrowser();
					internalBrowserControl.Closed += new InternalBrowser.ClosedEventHandler(InternalBrowserControl_Closed);
				}

				ShowDefaultImage();
			}

			updateContentThread = null;
			updateContentThread = new Thread(new ThreadStart(UpdateAdThreadProc));
			updateContentThread.Name = "UpdateContentThreadProc";
			updateContentThread.Start();
		}

		public override string ToString()
		{
			double left = 0;
			double top = 0;
			double right = 0;
			double bottom = 0;
			Panel parent = Parent as Panel;

			if (this.HorizontalAlignment == HorizontalAlignment.Left)
			{
				left = this.Margin.Left;
				if (parent != null)
				{
					right = parent.Width - this.Width - left;
				}
			}
			else if (this.HorizontalAlignment == HorizontalAlignment.Right)
			{
				right = this.Margin.Right;
				if (parent != null)
				{
					left = parent.Width - this.Width - right;
				}
			}

			if (this.VerticalAlignment == VerticalAlignment.Top)
			{
				top = this.Margin.Top;
				if (parent != null)
				{
					bottom = parent.Height - this.Height - top;
				}
			}
			else if (this.VerticalAlignment == VerticalAlignment.Bottom)
			{
				bottom = this.Margin.Bottom;
				if (parent != null)
				{
					top = parent.Height - this.Height - bottom;
				}
			}

			return "{Left: " + left + "; " + "Top: " + top + "; Right: " + right + "; Bottom: " + bottom + "}";
		}

		public override int GetHashCode()
		{
			return ToString().GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj is AdView)
			{
				return this.Width.Equals(((UserControl)obj).Width) && this.Height.Equals(((UserControl)obj).Height) && this.Margin.Equals(((AdView)obj).Margin);
			}

			return false;
		}
		#endregion

		#region "Properties"
		internal PhoneApplicationPage OwnerPage
		{
			get { return ownerPage; }
			set { ownerPage = value; }
		}
		#endregion

		#region "Setting parameters"
		public void SetSite(int site)
		{
			if (site > 0)
			{
				this.site = site;
				adserverRequest.SetSite(site);
			}
			else
			{
				WriteLog(Logger.LogLevel.ErrorsOnly, Logger.WRONG_PARAMETER, "site");
			}
		}
		public void SetZone(int zone)
		{
			if (zone > 0)
			{
				this.zone = zone;
				adserverRequest.SetZone(zone);
			}
			else
			{
				WriteLog(Logger.LogLevel.ErrorsOnly, Logger.WRONG_PARAMETER, "zone");
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
			else
			{
				WriteLog(Logger.LogLevel.ErrorsOnly, Logger.WRONG_PARAMETER, "premium");
			}
		}
		public void SetKeywords(string keywords)
		{
			if (!String.IsNullOrEmpty(keywords))
			{
				adserverRequest.SetKeywords(keywords);
			}
			else
			{
				WriteLog(Logger.LogLevel.ErrorsOnly, Logger.WRONG_PARAMETER, "keywords");
			}
		}
		public void SetAdsType(int adsType)
		{
			if (adsType == 1 || adsType == 2 || adsType == 3 || adsType == 6)
			{
				adserverRequest.SetAdsType(adsType);
			}
			else
			{
				WriteLog(Logger.LogLevel.ErrorsOnly, Logger.WRONG_PARAMETER, "ads type");
			}
		}
		public void SetMinSizeX(int size)
		{
			if (size > 0)
			{
				adserverRequest.SetMinSizeX(size);
			}
			else
			{
				WriteLog(Logger.LogLevel.ErrorsOnly, Logger.WRONG_PARAMETER, "min size x");
			}
		}
		public void SetMinSizeY(int size)
		{
			if (size > 0)
			{
				adserverRequest.SetMinSizeY(size);
			}
			else
			{
				WriteLog(Logger.LogLevel.ErrorsOnly, Logger.WRONG_PARAMETER, "min size y");
			}
		}
		public void SetMaxSizeX(int size)
		{
			if (size > 0)
			{
				adserverRequest.SetMaxSizeX(size);
			}
			else
			{
				WriteLog(Logger.LogLevel.ErrorsOnly, Logger.WRONG_PARAMETER, "max size x");
			}
		}
		public void SetMaxSizeY(int size)
		{
			if (size > 0)
			{
				adserverRequest.SetMaxSizeY(size);
			}
			else
			{
				WriteLog(Logger.LogLevel.ErrorsOnly, Logger.WRONG_PARAMETER, "max size y");
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
			else
			{
				WriteLog(Logger.LogLevel.ErrorsOnly, Logger.WRONG_PARAMETER, "custom parameters");
			}
		}
		public void SetAdserverURL(string adServerUrl)
		{
			if (!String.IsNullOrEmpty(adServerUrl))
			{
				adserverRequest.SetAdserverURL(adServerUrl);
			}
			else
			{
				WriteLog(Logger.LogLevel.ErrorsOnly, Logger.WRONG_PARAMETER, "adserver url");
			}
		}
		public void SetDefaultImage(Image defaultImage)
		{
			if (defaultImage != null)
			{
				this.defaultImage = defaultImage;
			}
			else
			{
				WriteLog(Logger.LogLevel.ErrorsOnly, Logger.WRONG_PARAMETER, "default image");
			}
		}
		public void SetInternalBrowser(bool internalBrowser)
		{
			this.internalBrowser = internalBrowser;
		}
		public void SetAdvertiserId(int advertiserId)
		{
			if (advertiserId > 0)
			{
				this.advertiserId = advertiserId;
			}
			else
			{
				WriteLog(Logger.LogLevel.ErrorsOnly, Logger.WRONG_PARAMETER, "advertiser id");
			}
		}
		public void SetGroupCode(string groupCode)
		{
			if (!String.IsNullOrEmpty(groupCode))
			{
				this.groupCode = groupCode;
			}
			else
			{
				WriteLog(Logger.LogLevel.ErrorsOnly, Logger.WRONG_PARAMETER, "group code");
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
			else
			{
				WriteLog(Logger.LogLevel.ErrorsOnly, Logger.WRONG_PARAMETER, "country");
			}
		}
		public void SetRegion(string region)
		{
			if (!String.IsNullOrEmpty(region))
			{
				adserverRequest.SetRegion(region);
			}
			else
			{
				WriteLog(Logger.LogLevel.ErrorsOnly, Logger.WRONG_PARAMETER, "region");
			}
		}
		public void SetCity(string city)
		{
			if (!String.IsNullOrEmpty(city))
			{
				adserverRequest.SetCity(city);
			}
			else
			{
				WriteLog(Logger.LogLevel.ErrorsOnly, Logger.WRONG_PARAMETER, "city");
			}
		}
		public void SetArea(string area)
		{
			if (!String.IsNullOrEmpty(area))
			{
				adserverRequest.SetArea(area);
			}
			else
			{
				WriteLog(Logger.LogLevel.ErrorsOnly, Logger.WRONG_PARAMETER, "area");
			}
		}
		public void SetMetro(string metro)
		{
			if (!String.IsNullOrEmpty(metro))
			{
				adserverRequest.SetMetro(metro);
			}
			else
			{
				WriteLog(Logger.LogLevel.ErrorsOnly, Logger.WRONG_PARAMETER, "metro");
			}
		}
		public void SetZip(string zip)
		{
			if (!String.IsNullOrEmpty(zip))
			{
				adserverRequest.SetZip(zip);
			}
			else
			{
				WriteLog(Logger.LogLevel.ErrorsOnly, Logger.WRONG_PARAMETER, "zip");
			}
		}
		public void SetCarrier(string carrier)
		{
			if (!String.IsNullOrEmpty(carrier))
			{
				adserverRequest.SetCarrier(carrier);
			}
			else
			{
				WriteLog(Logger.LogLevel.ErrorsOnly, Logger.WRONG_PARAMETER, "carrier");
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

/*
		public int Site { get; set; }
		public int Zone { get; set; }
		public bool Test { get; set; }
		public int Premium { get; set; }
		public string Keywords { get; set; }
		public int AdsType { get; set; }
		public int MinSizeX { get; set; }
		public int MinSizeY { get; set; }
		public int MaxSizeX { get; set; }
		public int MaxSizeY { get; set; }
		public Color BackgroundColor { get; set; }
		public Color TextColor { get; set; }
		public string CustomParameters { get; set; }
		public string AdserverURL { get; set; }
		public Image DefaultImage { get; set; }
		public bool InternalBrowser { get; set; }
		public int AdvertiserId { get; set; }
		public string GroupCode { get; set; }
		public int UpdateTime { get; set; }
		public string Latitude { get; set; }
		public string Longitude { get; set; }
		public string Country { get; set; }
		public string Region { get; set; }
		public string City { get; set; }
		public string Area { get; set; }
		public string Metro { get; set; }
		public string Zip { get; set; }
		public string Carrier { get; set; }
		public Logger Logger { get; set; }
		public Logger.LogLevel LogLevel { get; set; }
*/
		#endregion

		#region "Private, protected, internal methods"
		~AdView()
		{
			updateTimer.Dispose();

			AutoDetectParameters.Release();

#if THIRDPARTY
			AdCampaignsManager.Release();
#endif
			ormmaController = null;

			userAgentInited.Close();
			deviceIdInited.Close();

			if (checkNewVersionThread != null)
			{
				checkNewVersionThread.Join();
				checkNewVersionThread = null;
			}

			if (firstAppLaunchThread != null)
			{
				firstAppLaunchThread.Join();
				firstAppLaunchThread = null;
			}

			if (updateContentThread != null)
			{
				updateContentThread.Join();
				updateContentThread = null;
			}
		}

		private void CheckNewVersion()
		{
			checkNewVersionThread = new Thread(new ThreadStart(() => {
				string result = DataRequest.RequestStringDataSync(CHECK_NEW_VERSION_URL);

				if (result != SDK_VERSION)
				{
					//WriteLog(Logger.LogLevel.All, NEW_VERSION_MESSAGE);
				}
			}));

			checkNewVersionThread.Start();
		}

		private void Init()
		{
			//adserverRequest = new AdserverRequest();
			adserverRequest.SetAdserverURL(DEFAULT_AD_SERVER);
			adserverRequest.SetAdsType(3);
			adserverRequest.SetUserAgent(userAgent);
			adserverRequest.SetDeviceId(deviceId);
			adserverRequest.SetVersion(SDK_VERSION);
			adserverRequest.SetSizeRequired(true);
			
			//adViewer = new AdViewer();
			adViewer.UserAgentInited += AdViewer_UserAgentInited;
			adViewer.Navigating += AdViewer_Navigating;
			adViewer.LoadCompleted += AdViewer_LoadCompleted;
			adViewer.DisplayAdError += AdViewer_DisplayAdError;

			AutoDetectParameters.AddReferense();
#if THIRDPARTY
			AdCampaignsManager.AddReferense();
#endif

			if (AutoDetectParameters.Instance.DeviceId == null)
			{
				AutoDetectParameters.Instance.GetDeviceId += new EventHandler(OnGetDeviceId);
				AutoDetectParameters.Instance.LocationChanged += new AutoDetectParameters.LocationChangedEventHandler(GpsCoordinatesDetected);
				AutoDetectParameters.Instance.GpsSensorDisabled += new EventHandler(GpsSensorDisabled);
			}
			else 
			{
				OnGetDeviceId(this, EventArgs.Empty);
			}

			//expandedViewer = new AdViewer();
			expandedViewer.Control.Visibility = Visibility.Collapsed;
			ormmaController = new OrmmaController(adViewer, expandedViewer);
			ormmaController.ChangeVisibility += OrmmaChangeVisibility;
			ormmaController.Resize += OrmmaResize;
			ormmaController.Expand += OrmmaExpand;
			ormmaController.Close += OrmmaClose;
			ormmaController.OpenUrl += OrmmaOpenUrl;
			ormmaController.OpenMap += OrmmaOpenMap;
			ormmaController.PlayMedia += OrmmaPlayMedia;

			//videoShower = new VideoShower();
// 			videoShower.Opened += new EventHandler(VideoShower_Opened);
// 			videoShower.Click += new EventHandler(VideoShower_Click);

			updateTimer = new Timer(new TimerCallback(UpdateTimerTick));
		}

		private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
		{
			if (!LayoutRoot.Children.Contains(adViewer.Control))
			{
				LayoutRoot.Children.Add(adViewer.Control);
			}

			if (!LayoutRoot.Children.Contains(expandedViewer.Control))
			{
				LayoutRoot.Children.Add(expandedViewer.Control);
			}

			if (!LayoutRoot.Children.Contains(videoShower.Control))
			{
				LayoutRoot.Children.Add(videoShower.Control);
			}

			LayoutRoot.Width = this.Width;
			LayoutRoot.Height = this.Height;
			adViewer.Width = this.Width;
			adViewer.Height = this.Height;

			videoShower.Width = this.Width;
			videoShower.Height = this.Height;


			adPosition.X = this.Margin.Left;
			adPosition.Y = this.Margin.Top;
			adSize.Width = this.Width;
			adSize.Height = this.Height;

			metaTags = "<meta name=\"viewport\" content=\"width=" + this.Width.ToString("F0") + ", height=" + this.Height.ToString("F0") + ", user-scalable=yes\"/>";

			PhoneApplicationFrame frame = Application.Current.RootVisual as PhoneApplicationFrame;
			if (frame != null)
			{
				ownerPage = frame.Content as PhoneApplicationPage;
				screenSize = frame.RenderSize;
			}

			if (ownerPage != null)
			{
				OnAdViewLoaded();
				ownerPage.BackKeyPress += new EventHandler<System.ComponentModel.CancelEventArgs>(BackKeyPress);
				ownerPage.OrientationChanged += new EventHandler<OrientationChangedEventArgs>(OnOrientationChanged);
				supportedPageOrientation = ownerPage.SupportedOrientations;
			}

			ormmaController.SetDefaultPosition(this.Margin.Left, this.Margin.Top, this.RenderSize);
			ormmaController.SetMaxSize(Application.Current.RootVisual.RenderSize);

			ResumeUpdateTimer();

			Update();

			//OrmmaPlayMedia(null, null);

			//OrmmaOpenMap(null, null);
		}

		private void LayoutRoot_Unloaded(object sender, RoutedEventArgs e)
		{
			PauseUpdateTimer();
		}

		private void AdViewer_UserAgentInited(string userAgent)
		{
			this.userAgent = userAgent;
			adserverRequest.SetUserAgent(userAgent);
			userAgentInited.Set();
		}

		private void OnGetDeviceId(object sender, EventArgs e)
		{
			deviceId = AutoDetectParameters.Instance.DeviceId;
			adserverRequest.SetDeviceId(deviceId);
			deviceIdInited.Set();
			WriteLog(Logger.LogLevel.All, "AutoDetectParameters", Logger.UA_DETECTED, deviceId);
		}

		private void GpsCoordinatesDetected(double latitude, double longitude, double accuracy)
		{
			WriteLog(Logger.LogLevel.All, "AutoDetectParameters", Logger.GPS_COORDINATES_DETECTED, latitude.ToString("F1") + "," + longitude.ToString("F1"));
		}

		private void GpsSensorDisabled(object sender, EventArgs e)
		{}

		private void FirstAppLaunchProc()
		{
			deviceIdInited.WaitOne();

			try
			{
				if (String.IsNullOrEmpty(deviceId))
				{
					string savedDeviceId = null;
					try
					{
						savedDeviceId = IsolatedStorageSettings.ApplicationSettings["DeviceID"].ToString();
					}
					catch (System.Exception)
					{}

					if (String.IsNullOrEmpty(savedDeviceId))
					{
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
			catch (Exception)
			{ }
		}
		
		private void UpdateAdThreadProc()
		{
			userAgentInited.WaitOne();
			deviceIdInited.WaitOne();

			PauseUpdateTimer();

			if (site > 0 && zone > 0)
			{
				string url = adserverRequest.ToString();

				WriteLog(Logger.LogLevel.All, Logger.START_AD_DOWNLOAD, url);
				OnAdDownloadBegin();

				if (currentAdHttpRequest != null)
				{
					currentAdHttpRequest.Abort();
				}

				adRequestedEvent += AdRequested;
				currentAdHttpRequest = DataRequest.RequestAdAsync(url, adRequestedEvent, ++requestId);
			}
			else
			{
				if (String.IsNullOrEmpty(prevAdContent))
				{
					CloseDefaultImage();
				}

				prevAdContent = NO_SITE_ZONE_PARAMETERS_MESSAGE;

				ShowAd();
			}
		}

		private void AdRequested(object sender, DataRequest.AdEventArgs e)
		{
			if (e.AdRequestId == requestId)
			{
				currentAdHttpRequest = null;

				WriteLog(Logger.LogLevel.All, Logger.GET_SERVER_RESPONSE);

				//videoShower.Hide();

				newAdContent = e.Content;

				if (newAdContent == INVALID_PARAMETERS_RESPONSE)
				{
					OnAdDownloadError(INVALID_PARAMETERS_MESSAGE);
				}
				else if (e.Type == DataRequest.AD_TYPE.ERROR)
				{
					OnAdDownloadError(e.Content);
				}
				else
				{
					OnAdDownloadEnd();
					if (e.Type == DataRequest.AD_TYPE.VIDEO)
					{
						videoShower.Ready.WaitOne();

						WriteLog(Logger.LogLevel.All, Logger.START_RENDER_AD);
						videoShower.Play(e.VideoSrc, e.VideoHref);

						//videoShower.Play("http://www.jhepple.com/SampleMovies/niceday.wmv", e.VideoHref);
					}
					else if (newAdContent.Length > 0)
					{
						bool isThirdPartyAd = false;
#if THIRDPARTY
						isThirdPartyAd = SearchThirdParty(newAdContent);
#endif

						if (!isThirdPartyAd)
						{
#if THIRDPARTY
							DeleteThirdParty();
#endif
							if (String.IsNullOrEmpty(prevAdContent))
							{
								CloseDefaultImage();
							}

							prevAdContent = newAdContent;

							ShowAd();
						}
					}
					else // Content Length == 0
					{
						WriteLog(Logger.LogLevel.ErrorsAndInfo, "Empty content downloaded");
					}
				}
			}
		}

		internal void ShowAd()
		{
			if (!(String.IsNullOrEmpty(prevAdContent)))
			{
				WriteLog(Logger.LogLevel.All, Logger.START_RENDER_AD);

				Deployment.Current.Dispatcher.BeginInvoke(() =>
				{
					if (!LayoutRoot.Children.Contains(adViewer.Control))
					{
						LayoutRoot.Children.Add(adViewer.Control);
					}

				});

				string fullAdSource = "<html><head>" +
					metaTags +
					ormmaController.Library +
					"</head><body style=\"margin: 0px; padding: 0px; width: 100%; height: 100%\">"
					+
					prevAdContent
					+
					"<br><br> <a href=\"\" onclick=\"Ormma.openMap('http://maps.google.ru/maps?f=q&source=s_q&hl=ru&geocode=&q=%D1%80%D0%B5%D1%81%D0%BF%D1%83%D0%B1%D0%BB%D0%B8%D0%BA%D0%B0+%D0%9C%D0%B0%D1%80%D0%B8%D0%B9+%D0%AD%D0%BB,+%D0%99%D0%BE%D1%88%D0%BA%D0%B0%D1%80-%D0%9E%D0%BB%D0%B0&aq=0&sll=55.178868,49.658203&sspn=25.499525,80.683594&ie=UTF8&hq=&hnear=%D0%B3%D0%BE%D1%80%D0%BE%D0%B4+%D0%99%D0%BE%D1%88%D0%BA%D0%B0%D1%80-%D0%9E%D0%BB%D0%B0,+%D1%80%D0%B5%D1%81%D0%BF%D1%83%D0%B1%D0%BB%D0%B8%D0%BA%D0%B0+%D0%9C%D0%B0%D1%80%D0%B8%D0%B9+%D0%AD%D0%BB&ll=56.642637,47.891464&spn=0.19029,0.630341&t=k&z=11&iwloc=A', true); return false;\">OPEN MAP</a>" +

					"<br><br> <a href=\"\" onclick=\"Ormma.playVideo('http://www.jhepple.com/SampleMovies/niceday.wmv', { \'audio\' : \'muted\', \'autoplay\': \'autoplay\', \'controls\': \'controls\', \'loop\': \'loop\', \'startStyle\': \'fullscreen\', \'stopStyle\': \'exit\'}); return false;\">VIDEO FULSCREEN</a>" +
					"<br><br> <a href=\"\" onclick=\"Ormma.playVideo('http://www.jhepple.com/support/SampleMovies/WindowsMedia.wmv', {\'autoplay\': \'noautoplay\', \'controls\': \'controls\', \'loop\': \'loop\', \'startStyle\': \'normal\', \'stopStyle\': \'exit\', \'width\': 150, \'height\': 150, \'position\': {\'top\': 10, \'left\':20}}); return false;\">VIDEO INLINE</a>" +

					"</body></html>";

				string fileName = "adview" + viewId.ToString() + ".html";
				bool saved = false;

				if (DataRequest.USE_CACHE)
				{
					saved = TrySaveFile(fileName, fullAdSource);
				}

				ManualResetEvent showed = new ManualResetEvent(false);
				
				if (saved)
				{
					Deployment.Current.Dispatcher.BeginInvoke(() => 
					{
						adViewer.ShowCachedAd(fileName);
						showed.Set();
					});
				}
				else
				{
					Deployment.Current.Dispatcher.BeginInvoke(() => adViewer.ShowAd(fullAdSource));
				}

				showed.WaitOne();
			}
		}

		private bool TrySaveFile(string fileName, string ad)
		{
			bool saved = false;

			try
			{
				using (IsolatedStorageFile appStorage = IsolatedStorageFile.GetUserStoreForApplication())
				{
					if (appStorage.FileExists(fileName))
					{
						appStorage.DeleteFile(fileName);
					}

					using (IsolatedStorageFileStream file = appStorage.OpenFile(fileName, FileMode.Create))
					{
						using (StreamWriter writer = new StreamWriter(file))
						{
							writer.Write(ad);
							saved = true;
						}
					}
				}
			}
			catch (System.Exception)
			{}

			return saved;
		}

		private void UpdateTimerTick(Object stateInfo)
		{
			Deployment.Current.Dispatcher.BeginInvoke(() => 
				Update()
				);
		}

		private void AdViewer_Navigating(object sender, NavigatingEventArgs e)
		{
			if (e.Uri != null && !String.IsNullOrEmpty(e.Uri.ToString()))
			{
				if (internalBrowser && internalBrowserControl != null)
				{
					e.Cancel = true;
					if (!OnAdNavigateBanner(e.Uri.ToString()))
					{
						PauseUpdateTimer();
						WriteLog(Logger.LogLevel.All, Logger.OPEN_INTERNAL_BROWSER);
						internalBrowserControl.Show(e.Uri.ToString());
						if (ownerPage != null)
						{
							internalBrowserControl.ChangeOrientation(ownerPage.Orientation);
						}
					}
				}
				else
				{
					PauseUpdateTimer();
					try
					{
						WebBrowserTask task = new WebBrowserTask();
						task.URL = e.Uri.ToString();
						task.Show();
					}
					catch (System.Exception)
					{}
				}
			}
		}

		private void AdViewer_LoadCompleted(object sender, EventArgs e)
		{
			WriteLog(Logger.LogLevel.All, "AdViewer", Logger.AD_DISPLAYED);

			ResumeUpdateTimer();
		}

		private void AdViewer_DisplayAdError(object sender, EventArgs e)
		{
			WriteLog(Logger.LogLevel.ErrorsOnly, "AdViewer", Logger.AD_DISPLAY_ERROR);
		}

		protected virtual void ORMMAReady()
		{
		}

		private void ShowDefaultImage()
		{
			if (defaultImage != null && !LayoutRoot.Children.Contains(defaultImage))
			{
				WriteLog(Logger.LogLevel.All, Logger.DISPLAY_DEFAULT_IMAGE);

				try
				{
					LayoutRoot.Children.Add(defaultImage);
				}
				catch (System.Exception)
				{ }
			}
		}

		private void CloseDefaultImage()
		{
			Deployment.Current.Dispatcher.BeginInvoke(() =>
			{
				try
				{
					defaultImage.Visibility = Visibility.Collapsed;
					//LayoutRoot.Children.Remove(defaultImage);
				}
				catch (System.Exception)
				{ }
			});
		}

		internal void PauseUpdateTimer()
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

		private void BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (ormmaMediaPlayer != null && ormmaMediaPlayer.IsOpen)
			{
				e.Cancel = true;
				ormmaMediaPlayer.Close();
			}

			if (ormmaMap != null && ormmaMap.IsOpen)
			{
				e.Cancel = true;
				ormmaMap.Close();
			}

			if (internalBrowser && internalBrowserControl != null && internalBrowserControl.IsOpen)
			{
				e.Cancel = true;
				internalBrowserControl.Close();
				ResumeUpdateTimer();

				if (videoShower != null && videoShower.State == VideoShower.PLAYING_STATE.PAUSED)
				{
					videoShower.Resume();
				}
			}
		}

		protected virtual void OnOrientationChanged(object sender, OrientationChangedEventArgs e)
		{
			Size currentScreenSize = GetCurrentScreenSize();

			if (internalBrowser && internalBrowserControl != null && internalBrowserControl.IsOpen)
			{
				internalBrowserControl.ChangeOrientation(e.Orientation);
			}

			if (ormmaMap != null && ormmaMap.IsOpen)
			{
				ormmaMap.SetSize(currentScreenSize);
			}

			if (ormmaMediaPlayer != null && ormmaMediaPlayer.IsOpen)
			{
				ormmaMediaPlayer.Expand(e.Orientation);
			}

			ormmaController.ChangeOrientation(e.Orientation);
		}

		private void InternalBrowserControl_Closed(string url)
		{
			WriteLog(Logger.LogLevel.All, Logger.CLOSE_INTERNAL_BROWSER);

			OnAdWebViewClosing(url);

			if (videoShower != null && videoShower.State == VideoShower.PLAYING_STATE.PAUSED)
			{
				videoShower.Resume();
			}
		}

		internal void Resize(double width, double height)
		{
			this.Width = width;
			this.Height = height;
			LayoutRoot.Width = width;
			LayoutRoot.Height = height;
			adViewer.Width = width;
			adViewer.Height = height;
		}

		private void OrmmaChangeVisibility(object sender, OrmmaController.VisibilityEventArgs e)
		{
			//if (expandedViewer == null || expandedViewer.Control.Visibility == Visibility.Collapsed)
			{
				if (e.Visible)
				{
					this.Visibility = Visibility.Visible;
				}
				else
				{
					this.Visibility = Visibility.Collapsed;
				}
			}
		}

		private void OrmmaResize(object sender, OrmmaController.ResizeEventArgs e)
		{
			if (expandedViewer.Control.Visibility == Visibility.Collapsed)
			{
				Resize(e.Width, e.Height);

				ormmaController.ChangeSize(e.Width, e.Height);
			}
		}

		private void OrmmaExpand(object sender, OrmmaController.ExpandEventArgs e)
		{
			if (expandedViewer.Control.Visibility == Visibility.Collapsed)
			{
				PauseUpdateTimer();

				Size size = Application.Current.RootVisual.RenderSize;
				size.Width = Math.Max(size.Width, size.Height);
				size.Height = size.Width;

				this.Width = size.Width;
				this.Height = size.Height;
				this.Margin = new Thickness(0);
				LayoutRoot.Width = size.Width;
				LayoutRoot.Height = size.Height;
				adViewer.Control.Margin = new Thickness(adPosition.X, adPosition.Y, 0, 0);
				adViewer.Control.IsHitTestVisible = false;

				expandedViewer.Control.Visibility = Visibility.Visible;
				expandedViewer.Control.Margin = new Thickness(e.dimensions.x, e.dimensions.y, 0, 0);
				expandedViewer.Width = e.dimensions.width;
				expandedViewer.Height = e.dimensions.height;

				if (e.properties.useBackground)
				{
					expandedViewer.Control.Opacity = e.properties.backgroundOpacity;
				}

				if (e.properties.lockOrientation)
				{
					if (ownerPage != null)
					{
						switch (ownerPage.Orientation)
						{
							case PageOrientation.Portrait:
							case PageOrientation.PortraitDown:
							case PageOrientation.PortraitUp:
								ownerPage.SupportedOrientations = SupportedPageOrientation.Portrait;
								break;
							case PageOrientation.Landscape:
							case PageOrientation.LandscapeLeft:
							case PageOrientation.LandscapeRight:
								ownerPage.SupportedOrientations = SupportedPageOrientation.Landscape;
								break;
							default:
								break;
						}
					}
				}

				if (String.IsNullOrEmpty(e.Url))
				{
					string fileName = "expandedview" + viewId.ToString() + ".html";
					bool saved = false;

					if (DataRequest.USE_CACHE)
					{
						saved = TrySaveFile(fileName, adViewer.Control.SaveToString());
					}

					if (saved)
					{
						expandedViewer.ShowCachedAd(fileName);
					} 
					else
					{
						expandedViewer.ShowAd(adViewer.Control.SaveToString());
					}
				}
				else
				{
					expandedViewer.Navigate(new Uri(e.Url, UriKind.RelativeOrAbsolute));
				}

				ormmaController.ChangeSize(e.dimensions.width, e.dimensions.height);
				ormmaController.SetExpandedState();
			}
		}

		private void OrmmaClose(object sender, EventArgs e)
		{
			if (expandedViewer.Control.Visibility == Visibility.Visible)
			{
				OrmmaCloseExpanded();
				adViewer.Control.IsHitTestVisible = true;

				ResumeUpdateTimer();
			}
			else if (!adSize.Equals(new Size(adViewer.Width, adViewer.Height)))
			{
				Resize(adSize.Width, adSize.Height);
				ormmaController.ChangeSize(adSize.Width, adSize.Height);

				adViewer.Control.IsHitTestVisible = true;

				ResumeUpdateTimer();
			}
		}

		private void OrmmaCloseExpanded()
		{
			expandedViewer.Control.Visibility = Visibility.Collapsed;

			if (adSize.Equals(new Size(adViewer.Width, adViewer.Height)))	// not resized
			{
				OrmmaCloseExpandedToDefault();
			}
			else	// resized
			{
				OrmmaCloseExpandedToResized();
			}

			ownerPage.SupportedOrientations = supportedPageOrientation;
		}

		private void OrmmaCloseExpandedToDefault()
		{
			double d = 0;

			this.Width = adSize.Width + d;
			this.Height = adSize.Height + d;
			this.Margin = new Thickness(adPosition.X, adPosition.Y, 0, 0);
			LayoutRoot.Width = this.Width;
			LayoutRoot.Height = this.Height;
			adViewer.Control.Margin = new Thickness(d / 2);
			ormmaController.ChangeSize(adSize.Width, adSize.Height);
		}

		private void OrmmaCloseExpandedToResized()
		{
			double d = 0;

			this.Width = adViewer.Width + d;
			this.Height = adViewer.Height + d;
			this.Margin = new Thickness(adPosition.X, adPosition.Y, 0, 0);
			LayoutRoot.Width = this.Width;
			LayoutRoot.Height = this.Height;
			adViewer.Control.Margin = new Thickness(d / 2);

			ormmaController.ChangeSize(adViewer.Width, adViewer.Height);
		}

		private void OrmmaOpenUrl(object sender, OrmmaController.OpenUrlEventArgs e)
		{
			if (!internalBrowser)
			{
				internalBrowser = true;
				internalBrowserControl = new InternalBrowser();
			}

			PauseUpdateTimer();
			internalBrowserControl.Show(e.Url);
			if (ownerPage != null)
			{
				internalBrowserControl.ChangeOrientation(ownerPage.Orientation);
			}
		}

		private void OrmmaOpenMap(object sender, OrmmaController.OpenMapEventArgs e)
		{
			if (e != null && !String.IsNullOrEmpty(e.POI))
			{
				if (e.FullScreen)
				{
					if (ormmaMap == null)
					{
						Point position = GetCurrentAbsolutePosition();
						Size screenSize = GetCurrentScreenSize();
						ormmaMap = new OrmmaMap(ownerPage, position, screenSize);
						LayoutRoot.Children.Add(ormmaMap.Window);
					}
					
					ormmaMap.Show();
					ormmaMap.OpenPoi(e.POI);
				}
				else
				{
					try
					{
						LayoutRoot.Children.Remove(ormmaMap.MapControl);
					}
					catch (System.Exception)
					{}

					ormmaMap = new OrmmaMap();
					LayoutRoot.Children.Add(ormmaMap.MapControl);
					ormmaMap.OpenPoi(e.POI);
				}
			}
		}

		private void OrmmaPlayMedia(object sender, OrmmaController.PlayMediaEventArgs e)
		{
			try
			{
				ormmaMediaPlayer.Close();
			}
			catch (System.Exception)
			{}

			ormmaMediaPlayer = null;

			ormmaMediaPlayer = new OrmmaMediaPlayer(ownerPage, screenSize, e.Properties);

			if (ormmaMediaPlayer.Window != null)
			{
				LayoutRoot.Children.Add(ormmaMediaPlayer.Window);
			}

//			string url = null;
// #if DEBUG
// 			url = "http://www.jhepple.com/SampleMovies/niceday.wmv";
// #else
// 			url = "http://mobile.mojiva.com/5_SILclip_iphone.mp4";
// #endif

			ormmaMediaPlayer.Play(e.Url);
		}

		private void VideoShower_Opened(object sender, EventArgs e)
		{
			WriteLog(Logger.LogLevel.All, "VideoShower", Logger.AD_DISPLAYED);
			CloseDefaultImage();
		}

		private void VideoShower_Click(object sender, EventArgs e)
		{
			string url = videoShower.Href;

			if (!String.IsNullOrEmpty(url))
			{
				if (internalBrowser && internalBrowserControl != null)
				{
					if (!OnAdNavigateBanner(url))
					{
						PauseUpdateTimer();
						internalBrowserControl.Show(url);
						if (ownerPage != null)
						{
							internalBrowserControl.ChangeOrientation(ownerPage.Orientation);
						}
					}
				}
				else
				{
					PauseUpdateTimer();
					WebBrowserTask task = new WebBrowserTask();
					task.URL = url;
					task.Show();
				}
			}
		}

		internal static Color ColorFromRgb(string rgb)
		{
			if (rgb.Length >= 7)
			{
				try
				{
					byte r = byte.Parse(rgb.Substring(1, 2));
					byte g = byte.Parse(rgb.Substring(3, 2));
					byte b = byte.Parse(rgb.Substring(5, 2));
					return Color.FromArgb(255, r, g, b);
				}
				catch (Exception)
				{ }
			}
			return Color.FromArgb(0, 0, 0, 0);
		}

		internal static string ColorToRgb(Color color)
		{
			return "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
		}

		private Point GetCurrentAbsolutePosition()
		{
			Point position = new Point(0, 0);

			try
			{
				position = this.TransformToVisual(ownerPage).Transform(new Point(0, 0));
			}
			catch (System.Exception)
			{}

			return position;
		}

		private Size GetCurrentScreenSize()
		{
			if (ownerPage != null)
			{
				switch (ownerPage.Orientation)
				{
					case PageOrientation.Portrait:
					case PageOrientation.PortraitUp:
					case PageOrientation.PortraitDown:
						return screenSize;
					case PageOrientation.Landscape:
					case PageOrientation.LandscapeLeft:
					case PageOrientation.LandscapeRight:
					default:
						return new Size(screenSize.Height, screenSize.Width);
				}
			}

			return screenSize;
		}

		private void WriteLog(Logger.LogLevel logLevel, string message, string parameter = null)
		{
			WriteLog(logLevel, "AdView", message, parameter);
		}

		private void WriteLog(Logger.LogLevel logLevel, string className, string message, string parameter = null)
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

		#region "Events"
		private event AdNavigateEventHandler AdNavigateBannerEvent = null;
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
			WriteLog(Logger.LogLevel.All, Logger.FINISH_AD_DOWNLOAD);
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
			if (String.IsNullOrEmpty(error))
			{
				error = EMPTY_CONTENT_MESSAGE;
			}

			WriteLog(Logger.LogLevel.ErrorsOnly, Logger.FAIL_AD_DOWNLOAD, error);
			if (AdDownloadErrorEvent != null)
			{
				try
				{
					AdDownloadErrorEvent(error);
				}
				catch (System.Exception)
				{}
			}
		}
		public event AdDownloadErrorEventHandler AdDownloadError
		{
			add { AdDownloadErrorEvent += value; }
			remove { AdDownloadErrorEvent -= value; }
		}

		private event AdWebViewClosingEventHandler AdWebViewClosingEvent = null;
		protected virtual void OnAdWebViewClosing(string url)
		{
			if (AdWebViewClosingEvent != null)
			{
				try
				{
					AdWebViewClosingEvent(url);
				}
				catch (System.Exception)
				{}
			}
		}
		public event AdWebViewClosingEventHandler AdWebViewClosing
		{
			add { AdWebViewClosingEvent += value; }
			remove { AdWebViewClosingEvent -= value; }
		}
		#endregion

		#region "Internal events"
		private event EventHandler AdViewLoadedEvent = null;
		private void OnAdViewLoaded()
		{
			Init();

			if (AdViewLoadedEvent != null)
			{
				AdViewLoadedEvent(this, EventArgs.Empty);
			}
		}
		internal event EventHandler AdViewLoaded
		{
			add { AdViewLoadedEvent += value; }
			remove { AdViewLoadedEvent -= value; }
		}
		#endregion

#if THIRDPARTY
		private AdCampaignsManager.ThirdPartyAdView thirdPartyAdView = null;

		private bool SearchThirdParty(string adContent)
		{
			ManualResetEvent searched = new ManualResetEvent(false);
			bool added = false;

			thirdPartyAdView = AdCampaignsManager.Instance.GetInitThirdPartyAdView(viewId, adContent);

			Deployment.Current.Dispatcher.BeginInvoke(() =>
			{
				if (thirdPartyAdView != null)
				{
					thirdPartyAdView.ExCampaign += new EventHandler<AdCampaignsManager.ThirdPartyAdView.ExCampaignEventArgs>(AdCampaignsManagerAddExCampaign);

					LayoutRoot.Children.Clear();
					LayoutRoot.Children.Add(thirdPartyAdView.View);
					thirdPartyAdView.Run();
					added = true;
				}
				else
				{
					added = false;
				}

				searched.Set();
			});

			searched.WaitOne();

			return added;
		}

		private void AdCampaignsManagerAddExCampaign(object sender, AdCampaignsManager.ThirdPartyAdView.ExCampaignEventArgs e)
		{
			WriteLog(Logger.LogLevel.ErrorsAndInfo, "External campaign " + e.CampaignId + " added");

			adserverRequest.AddExternalCampaign(e.CampaignId);

			Update();
		}

		private void DeleteThirdParty()
		{
			if (thirdPartyAdView != null)
			{
				if (AdCampaignsManager.Instance.TryDeleteThirdPartyAdView(viewId))
				{
					Deployment.Current.Dispatcher.BeginInvoke(() =>
					{
						try
						{
							LayoutRoot.Children.Remove(thirdPartyAdView.View);
						}
						catch (System.Exception)
						{}

						thirdPartyAdView = null;
					});
				}
			}
		}
#endif
	}

	#region "Event Handlers"
	public delegate bool AdNavigateEventHandler(string url);
	public delegate void AdDownloadErrorEventHandler(string error);
	public delegate void AdWebViewClosingEventHandler(string url);
	#endregion
}
