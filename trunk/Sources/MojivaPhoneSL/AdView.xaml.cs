/****
 * © 2010-2011 mOcean Mobile. A subsidiary of Mojiva, Inc. All Rights Reserved.
 * */

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Resources;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Info;
using Microsoft.Phone.Tasks;
using System.Diagnostics;

namespace MojivaPhone
{
	internal delegate void FireEventDelegate(string eventName, string[] args);

	public partial class AdView : UserControl
	{
		#region "Type definitions"
		internal enum ADS_MODE
		{
			/**
			 * The first-load ads counter is sent only. Ads is not shown.
			 */
			MODE_COUNTER_ONLY = 1,
			/**
			 * Ads is shown only. The first-load ads counter is not sent.
			 */
			MODE_ADS_ONLY = 2,
			/**
			 * The first-load ads counter is sent and ads is shown.
			 */
			MODE_COUNTER_AND_ADS = 3
		};

		internal enum ADS_TYPE
		{
			ADS_TYPE_UNDEFINED = 0,
			/**
			 * Type of advertisement: text only.
			 */
			ADS_TYPE_TEXT_ONLY = 1,
			/**
			 * Type of advertisement: image only.
			 */
			ADS_TYPE_IMAGES_ONLY = 2,
			/**
			 * Type of advertisement: image and text.
			 */
			ADS_TYPE_TEXT_AND_IMAGES = 3,
			/**
			 * Type of advertisement: SMS ad. SMS will be returned in XML.
			 */
			ADS_TYPE_SMS = 4
		};

		internal enum OVER_18_TYPE
		{
			OVER_18_TYPE_UNDEFINED = 0,
			/**
			 * Type of over 18 content: deny over 18 content.
			 */
			OVER_18_TYPE_DENY = 1,
			/**
			 * Type of over 18 content: only over 18 content.
			 */
			OVER_18_TYPE_ONLY = 2,
			/**
			 * Type of over 18 content: allow all ads including over 18 content.
			 */
			OVER_18_TYPE_ALL = 3
		};

		internal enum PREMIUM_TYPE
		{
			PREMIUM_STATUS_UNDEFINED = 0,
			/**
			 * Premium type: premium and non-premium.
			 */
			PREMIUM_STATUS_BOTH = 3,
			/**
			 * Premium type: premium only.
			 */
			PREMIUM_STATUS_PREMIUM = 2,
			/**
			 * Premium type: non-premium.
			 */
			PREMIUM_STATUS_NON_PREMIUM = 1
		};

		internal enum OUTPUT_FORMAT
		{
			OUTPUT_FORMAT_UNDEFINED = 0,
			/**
			 * Type of output format: normal.
			 */
			OUTPUT_FORMAT_NORMAL = 1,
			/**
			 * Type of output format: XML.
			 */
			OUTPUT_FORMAT_XML = 3,
			/**
			 * Type of output format: JSON.
			 */
			OUTPUT_FORMAT_JSON = 5
		};

		internal enum TARGET_TYPE
		{
			TARGET_UNDEFINED = 0,
			/**
			 * Type of target attribute for HTML link element: open the linked document in a new window.
			 */
			TARGET_BLANK = 1,
			/**
			 * Type of target attribute for HTML link element: open the linked document in the parent frameset.
			 */
			TARGET_PARENT = 2,
			/**
			 * Type of target attribute for HTML link element: open the linked document in the same frame.
			 */
			TARGET_SELF = 3,
			/**
			 * Type of target attribute for HTML link element: open the linked document in the full body of the window.
			 */
			TARGET_TOP = 4
		};

		internal enum EOrmmaState
		{
			UNKNOWN,
			HIDDEN,
			DEFAULT,
			EXPANDED,
			RESIZED,
			COUNT
		}

		internal enum EOrmmaEvents
		{
			ERROR,
			HEADING_CHANGE,
			KEYBOARD_CHANGE,
			LOCATION_CHANGE,
			NETWORK_CHANGE,
			ORIENTATION_CHANGE,
			READY,
			RESPONSE,
			SCREEN_CHANGE,
			SHAKE,
			SIZE_CHANGE,
			STATE_CHANGE,
			TILT_CHANGE,
			COUNT
		}
		#endregion

		#region "Static variables"
		/**
		 * Type of target attribute for HTML link element: open the linked document in a new window.
		 */
		private static String TARGET_BLANK_CONST = "_blank";
		/**
		 * Type of target attribute for HTML link element: open the linked document in the parent frameset.
		 */
		private static String TARGET_PARENT_CONST = "_parent";
		/**
		 * Type of target attribute for HTML link element: open the linked document in the same frame.
		 */
		private static String TARGET_SELF_CONST = "_self";
		/**
		 * Type of target attribute for HTML link element: open the linked document in the full body of the window.
		 */
		private static String TARGET_TOP_CONST = "_top";

		private static String SETTINGS_IS_FIRST_APP_LAUNCH = "isFirstAppLaunch";
		private static String FIRST_APP_LAUNCH_URL = "http://www.moceanmobile.com/appconversion.php";
		private static int AD_RELOAD_PERIOD = 120000; //in milliseconds

		private const string ASSETS_ROOT_DIR = "assets";
		private const string ASSETS_INDEX_FILENAME = "index.htm";

		private const int DEFAULT_SITE_ID = 8061;
		private const int DEFAULT_ZONE_ID = 20249;
		#endregion

		#region "Variables"
		private ManualResetEvent threadStop = new ManualResetEvent(false);
		private Thread contentThread;
		private Timer reloadTimer;
		private AdserverRequest adserverRequest = null;
#if USE_CACHE
		internal bool _useCache = false;
#endif
		internal int _viewId = 1;
		internal bool _internalBrowser = false;
		internal Image _defaultImage = null;
		internal int _reloadPeriod = AD_RELOAD_PERIOD;
		internal String _appId;
		internal String _campaign;
		internal ADS_MODE _mode = ADS_MODE.MODE_ADS_ONLY;
		internal String _site;
		internal String _zone;
		internal String _ip;
		internal String _keywords;
		internal ADS_TYPE _adstype = ADS_TYPE.ADS_TYPE_TEXT_AND_IMAGES;
		internal OVER_18_TYPE _over18 = OVER_18_TYPE.OVER_18_TYPE_UNDEFINED;
		internal String _latitude, _longitude, _course, _ua;
		internal PREMIUM_TYPE _premium = PREMIUM_TYPE.PREMIUM_STATUS_UNDEFINED;
		internal OUTPUT_FORMAT _key = OUTPUT_FORMAT.OUTPUT_FORMAT_NORMAL;
		internal bool _testModeEnabled;
		internal int _count = 1;
		internal String _country;
		internal String _region;
		internal String _city;
		internal String _area;
		internal String _metro;
		internal String _zip;
		internal bool _sizeRequired;
		internal bool _textBorderEnabled;
		internal String _paramBorder;
		internal String _paramBG;
		internal String _paramLink;
		internal int _imageMinWidth = 0, _imageMinHeight = 0, _imageMaxWidth = 0, _imageMaxHeight = 0;
		internal String _carrier;
		internal String _target;
		internal String _url;
		internal bool _pixelModeEnabled;
		internal bool _navigating = true;
		internal bool _allowNavi = false;
		internal bool _first = true;
		internal string _customParameters;
		internal string _adServerUrl = "http://ads.mocean.mobi/ad";
		internal string _advertiserId;
		internal string _groupCode;

		internal ManualResetEvent _firstEvent = new ManualResetEvent(false);

		private string ormmaLibText = "";
		private FireEventDelegate fireEventDelegate;
		private CExpandedView expandedView = null;
		private CAccelerometer accelerometer = null;
		private CNativeAppManager nativeAppManager = null;
		private CAssetManager assetManager = null;
		private CVideoShower videoShower = null;
		private CInternalBrowser internalBrowserController = null;
		private CThirdPartyManager thirdPartyManager = null;

		#endregion

		#region "Properties"
		
#if USE_CACHE
		/// <summary>
		/// Use cache for Ad or not.
		/// </summary>
		public bool UseCache
		{
			get { return _useCache;
			}
			set { _useCache = value;
			}
		}
#endif

		/// <summary>
		/// Required.
		/// View id (default 1)
		/// </summary>
		[Obsolete("This property is obsolete. Don't use it")]
		public int ViewId
		{
			get { return _viewId; }
			set { _viewId = value; }
		}

		/// <summary>
		/// The flag which operates advertising opening. False - Ad opens in an external browser. 
		/// True - Ad opening at internal form.
		/// (default = false)
		/// </summary>
		public bool InternalBrowser
		{
			get { return _internalBrowser; }
			set { _internalBrowser = value; }
		}

		/// <summary>
		/// Image which will be shown during advertising loading if there is no advertising in a cache.
		/// (default = none)
		/// </summary>
		public Image DefaultImage
		{
			get { return _defaultImage; }
			set { _defaultImage = value; }
		}

		/// <summary>
		/// The period of an automatic reload of advertising (in milliseconds).
		/// (default = 120000)
		/// </summary>
		public int UpdateTime
		{
			get { return _reloadPeriod/1000; }
			set
			{
				if (value == 0)
				{
					_reloadPeriod = Timeout.Infinite;
				} 
				else
				{
					_reloadPeriod = value * 1000;
				}
			}
		}

		/// <summary>
		/// Required.
		/// The id of the application.
		/// </summary>
		[Obsolete("This property is obsolete. Don't use it")]
		public String AppId
		{
			get { return _appId; }
			set { _appId = value; }
		}

		/// <summary>
		/// Campaign name
		/// </summary>
		[Obsolete("This property is obsolete. Don't use it")]
		public String Campaign
		{
			get { return _campaign; }
			set { _campaign = value; }
		}

		/// <summary>
		/// Required.
		/// Mode of viewer of advertising (use 1 - counter only, 2 - ads only, 3 - counter and ads)
		/// (default = 2)
		/// </summary>
		[Obsolete("This property is obsolete. Don't use it")]
		public int Mode
		{
			get { return (int)_mode; }
			set { _mode = (ADS_MODE)value; }
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
					return Int32.Parse(_site);
				}
				catch (System.Exception /*ex*/)
				{}
				return DEFAULT_SITE_ID;
			}
			set { _site = value.ToString(); }
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
					return Int32.Parse(_zone);
				}
				catch (System.Exception /*ex*/)
				{}
				return DEFAULT_ZONE_ID;
			}
			set { _zone = value.ToString(); }
		}

		/// <summary>
		/// The IP address of the carrier gateway over which the device is connecting.
		/// Autodetected if not set.
		/// </summary>
		[Obsolete("This property is obsolete. Don't use it")]
		public String Ip
		{
			get { return _ip; }
			set { _ip = value; }
		}

		/// <summary>
		/// Keywords to search ad delimited by commas.
		/// </summary>
		public String Keywords
		{
			get { return _keywords; }
			set { _keywords = value; }
		}

		/// <summary>
		/// Required.
		/// Type of advertisement (0 - disabled, 1 - text only, 2 - image only, 3 - image and text, 4 - SMS ad). SMS will be returned in XML.
		/// (default = 3)
		/// </summary>
		[Obsolete("This property is obsolete. Don't use it")]
		public int Type
		{
			get { return (int)_adstype; }
			set { _adstype = (ADS_TYPE)value; }
		}

		/// <summary>
		/// Filter by ad over 18 content (0 - disabled, 1 - deny over 18 content , 2 - only over 18 content, 3 - allow all ads including over 18 content).
		/// (default = 0)
		/// </summary>
		[Obsolete("This property is obsolete. Don't use it")]
		public int Over18
		{
			get { return (int)_over18; }
			set { _over18 = (OVER_18_TYPE)value; }
		}

		/// <summary>
		/// Latitude. 
		/// Autodetected if not set.
		/// </summary>
		public String Latitude
		{
			get { return _latitude; }
			set { _latitude = value; }
		}

		/// <summary>
		/// Longitude.
		/// Autodetected if not set.
		/// </summary>
		public String Longitude
		{
			get { return _longitude; }
			set { _longitude = value; }
		}

		/// <summary>
		/// Course.
		/// Autodetected if not set.
		/// </summary>
		public String Course
		{
			get { return _course; }
			set { _course = value; }
		}

		/// <summary>
		/// The browser user agent of the device making the request.
		/// </summary>
		[Obsolete("This property is obsolete and autodetected. Don't use it")]
		public String UserAgent
		{
			get { return _ua; }
			set { _ua = value; }
		}

		/// <summary>
		/// Filter by premium (0 - disabled, 1 - non-premium, 2 - premium only, 3 - both). Can be used only by premium publishers.
		/// (default = 0)
		/// </summary>
		public String Premium
		{
			get { return _premium.ToString(); }
			set
			{
				try
				{
					_premium = (PREMIUM_TYPE)(Enum.Parse(typeof(PREMIUM_TYPE), value, true));
				}
				catch (System.Exception /*ex*/)
				{
					_premium = PREMIUM_TYPE.PREMIUM_STATUS_UNDEFINED;
				}
				
			}
		}

		/// <summary>
		/// Output format. Undefined = 0. Normal format uses key = 1. Parameter key should be set to 2 in order to use XML output and to 3 in order to use JSON output. 
		/// (default = 0)
		/// </summary>
		public int OutputFormat
		{
			get { return (int)_key; }
			set { _key = (OUTPUT_FORMAT)value; }
		}

		/// <summary>
		/// To retrieve test ads/for testing
		/// </summary>
		public bool Test
		{
			get { return _testModeEnabled; }
			set { _testModeEnabled = value; }
		}

		/// <summary>
		/// Quantity of ads, returned by a server. Maximum value is 5. 
		/// (default = 1)
		/// </summary>
		[Obsolete("This property is obsolete. Don't use it")]
		public int Count
		{
			get { return _count; }
			set { _count = value; }
		}

		/// <summary>
		/// Country of visitor (for example: US). 
		/// Autodetected if not set.
		/// </summary>
		public String Country
		{
			get { return _country; }
			set { _country = value; }
		}

		/// <summary>
		/// Region of visitor (for example: NY).
		/// </summary>
		public String Region
		{
			get { return _region; }
			set { _region = value; }
		}

		/// <summary>
		/// City of the device user (with state). For US only.
		/// </summary>
		public String City
		{
			set { _city = value; }
			get { return _city; }
		}

		/// <summary>
		/// Area code of a user. For US only
		/// </summary>
		public String Area
		{
			set { _area = value; }
			get { return _area; }
		}

		/// <summary>
		/// Metro code of a user. For US only
		/// </summary>
		public String Metro
		{
			set { _metro = value; }
			get { return _metro; }
		}

		/// <summary>
		/// Zip/Postal code of user (note: parameter is all caps). For US only.
		/// </summary>
		public String Zip
		{
			set { _zip = value; }
			get { return _zip; }
		}

		/// <summary>
		/// If set to 1, return image size (width and height) in html.
		/// </summary>
		public bool SizeRequired
		{
			get { return _sizeRequired; }
			set { _sizeRequired = value; }
		}

		/// <summary>
		/// Advertiser id (will be provided by mocean)
		/// </summary>
		public String AdvertiserId
		{
			get { return _advertiserId; }
			set { _advertiserId = value; }
		}

		/// <summary>
		/// Group code (will be provided by mocean)
		/// </summary>
		public String GroupCode
		{
			get { return _groupCode; }
			set { _groupCode = value; }
		}

		/// <summary>
		/// Show borders around text ads (false - non-borders, true - show borders). 
		/// (default = false)
		/// </summary>
		[Obsolete("This property is obsolete. Don't use it")]
		public bool TextBorderEnabled
		{
			get { return _textBorderEnabled; }
			set { _textBorderEnabled = value; }
		}

		/// <summary>
		/// Borders color
		/// </summary>
		[Obsolete("This property is obsolete. Don't use it")]
		public Color BorderColor
		{
			get { return ColorFromRgb(_paramBorder); }
			set { _paramBorder = ColorToRgb(value); }
		}

		/// <summary>
		/// Background color in borders
		/// </summary>
		public Color BackgroundColor
		{
			get { return ColorFromRgb(_paramBG); }
			set { _paramBG = ColorToRgb(value); }
		}

		/// <summary>
		/// Text color
		/// </summary>
		public Color TextColor
		{
			get { return ColorFromRgb(_paramLink); }
			set { _paramLink = ColorToRgb( value ); }
		}

		/// <summary>
		/// Carrier name.
		/// Autodetected if not set.
		/// </summary>
		public String Carrier
		{
			get { return _carrier; }
			set { _carrier = value; }
		}

		/// <summary>
		/// Override size detection for banners
		/// </summary>
		public int MinSizeX
		{
			get { return _imageMinWidth; }
			set { _imageMinWidth = value; }
		}

		/// <summary>
		/// Override size detection for banners
		/// </summary>
		public int MinSizeY
		{
			get { return _imageMinHeight; }
			set { _imageMinHeight = value; }
		}

		/// <summary>
		/// Override size detection for banners
		/// </summary>
		public int MaxSizeX
		{
			get { return _imageMaxWidth; }
			set { _imageMaxWidth = value; }
		}

		/// <summary>
		/// Override size detection for banners
		/// </summary>
		public int MaxSizeY
		{
			get { return _imageMaxHeight; }
			set { _imageMaxHeight = value; }
		}

		/// <summary>
		/// Target attribute for HTML link element (0 - undefined, 1 - open the linked document in a new window, 2 - open the linked document in the same frame, 3 - open the linked document in the parent frameset, 4 - open the linked document in the full body of the window). 
		/// (default = 0)
		/// </summary>
		public int Target
		{
			get
			{
				if (_target == TARGET_BLANK_CONST) return 1;
				else if (_target == TARGET_PARENT_CONST) return 2;
				else if (_target == TARGET_SELF_CONST) return 3;
				else if (_target == TARGET_TOP_CONST) return 4;

				return 0;
			}
			set
			{
				switch(value)
				{
					case 1:
						_target = TARGET_BLANK_CONST;
						break;
					case 2:
						_target = TARGET_PARENT_CONST;
						break;
					case 3:
						_target = TARGET_SELF_CONST;
						break;
					case 4:
						_target = TARGET_TOP_CONST;
						break;
					default:
						_target = "";
						break;
				}
			}
		}

		/// <summary>
		/// URL of site for which it is necessary to receive advertising. 
		/// </summary>
		[Obsolete("This property is obsolete. Don't use it")]
		public String SiteUrl
		{
			get { return _url; }
			set { _url = value; }
		}

		/// <summary>
		/// If true, return redirect to image for ad (or tracking pixel 1x1) directly (instead of html response).
		/// (default = false)
		/// </summary>
		[Obsolete("This property is obsolete. Don't use it")]
		public bool PixelModeEnabled
		{
			get { return _pixelModeEnabled; }
			set { _pixelModeEnabled = value; }
		}

		public PhoneApplicationPage Owner
		{
			get;
			set;
		}

		public String AdServerUrl
		{
			set { _adServerUrl = value; }
			get { return _adServerUrl; }
		}

		public String CustomParameters
		{
			set { _customParameters = value; }
			get { return _customParameters; }
		}
		#endregion

		#region "Public methods"
		/// <summary>
		/// Constructor
		/// </summary>
		public AdView()
		{
			InitializeComponent();
			AutoDetectParameters.AddRef();
			reloadTimer = new Timer(new TimerCallback(reloadTimer_Tick));
			m_webBrowser.IsScriptEnabled = true;

 			this.Loaded += new RoutedEventHandler(UserControl_Loaded);
 			this.Unloaded += new RoutedEventHandler(UserControl_Unloaded);
 			m_webBrowser.Navigating += new EventHandler<NavigatingEventArgs>(AdViewControl_Navigating);
 			m_webBrowser.Navigated += new EventHandler<NavigationEventArgs>(m_webBrowser_Navigated);
 			m_webBrowser.ScriptNotify += new EventHandler<NotifyEventArgs>(WebBrowser_ScriptNotify);
			fireEventDelegate = new FireEventDelegate(FireEvent);

			AutoDetectParameters.Instance.GetNetwork += new EventHandler(NetworkChanged);
		}

		/// <summary>
		/// Run the advertisement (should be called to run banner)
		/// </summary>
		public virtual void Run()
		{
			InitOrmmaLibrary();
			//StreamReader reader = new StreamReader(Microsoft.Xna.Framework.TitleContainer.OpenStream("html/test_ad_level1.htm"));
			//m_webBrowser.NavigateToString(reader.ReadToEnd());

			m_webBrowser.NavigateToString("<html><head><script type=\"text/javascript\">function getUA(){return navigator.userAgent;}</script></head></html>");

			threadStop.Reset();
			m_webBrowser.Base = "";
			using (IsolatedStorageFile appStorage = IsolatedStorageFile.GetUserStoreForApplication())
			{
				if (!appStorage.DirectoryExists("Cache"))
				{
					appStorage.CreateDirectory("Cache");
				}
				if (!appStorage.DirectoryExists("Cache\\Images"))
				{
					appStorage.CreateDirectory("Cache\\Images");
				}
			}

			AutoDetectParameters.Instance.GetLatitude += new EventHandler(OnUpdateLocation);
			AutoDetectParameters.Instance.GetLongitude += new EventHandler(OnUpdateLocation);
			AutoDetectParameters.Instance.GetCourse += new EventHandler(OnUpdateCourse);

			bool isShowAds = true;
			bool isSentCounter = true;

			switch (_mode)
			{
				case ADS_MODE.MODE_ADS_ONLY:
					isShowAds = true;
					isSentCounter = false;
					break;
				case ADS_MODE.MODE_COUNTER_ONLY:
					isShowAds = false;
					isSentCounter = true;
					break;
				case ADS_MODE.MODE_COUNTER_AND_ADS:
					isShowAds = true;
					isSentCounter = true;
					break;
			}

			if (isShowAds)
			{
				if (contentThread == null)
				{
					contentThread = new Thread(new ThreadStart(ContentThreadProc));
					if (!contentThread.IsAlive) contentThread.Start();
				}
			}

			//if (isSentCounter)
			{
				Thread firstAppLaunchThread = new Thread(new ThreadStart(FirstAppLaunchProc));
				firstAppLaunchThread.Start();
			}

			if (Owner != null)
			{
				Owner.OrientationChanged += new EventHandler<OrientationChangedEventArgs>(MainPage_OrientationChanged);
				Owner.BackKeyPress += new EventHandler<System.ComponentModel.CancelEventArgs>(Owner_BackKeyPress);
			}

			accelerometer = new CAccelerometer(fireEventDelegate);
			nativeAppManager = new CNativeAppManager();
			assetManager = new CAssetManager(fireEventDelegate, ASSETS_ROOT_DIR);
			videoShower = new CVideoShower();

			if (_internalBrowser)
			{
				internalBrowserController = new CInternalBrowser();
			}
		}

		private void InitAdserverRequest()
		{
			if (adserverRequest == null)
			{
				adserverRequest = new AdserverRequest();
			}

			adserverRequest.setSite(_site);
			//adserverRequest.setIp(_ip);
			adserverRequest.setUa(_ua);
			adserverRequest.setUrl(_url);
			adserverRequest.setKeywords(_keywords);
			adserverRequest.setPremium(Convert.ToInt32(_premium));
			adserverRequest.setZone(_zone);
			adserverRequest.setTestModeEnabled(_testModeEnabled);
			adserverRequest.setCount(_count);
			adserverRequest.setCountry(_country);
			adserverRequest.setRegion(_region);
			adserverRequest.setCity(_city);
			adserverRequest.setArea(_area);
			adserverRequest.setMetro(_metro);
			adserverRequest.setZip(_zip);
			adserverRequest.SizeRequired = _sizeRequired;
			adserverRequest.setAdstype(Convert.ToInt32(_adstype));
			//adserverRequest.setOver18(Convert.ToInt32(_over18));
			adserverRequest.setLatitude(_latitude);
			adserverRequest.setLongitude(_longitude);
			//adserverRequest.setTextborderEnabled(_textBorderEnabled);
			//adserverRequest.setParamBorder(_paramBorder);
			adserverRequest.setParamBG(_paramBG);
			adserverRequest.setParamLINK(_paramLink);
			adserverRequest.setCarrier(_carrier);
			adserverRequest.setImageMinWidth(_imageMinWidth);
			adserverRequest.setImageMinHeight(_imageMinHeight);
			adserverRequest.setImageMaxWidth(_imageMaxWidth);
			adserverRequest.setImageMaxHeight(_imageMaxHeight);
			adserverRequest.setTarget(_target);
			//adserverRequest.setPixelModeEnabled(_pixelModeEnabled);
			adserverRequest.setAdserverURL(_adServerUrl);
			adserverRequest.setOutputFormat((int)_key);
			adserverRequest.SetCustomParameters(_customParameters);
			adserverRequest.setVersion("2.0");
		}

		/// <summary>
		/// Set custom parameters array
		/// </summary>
		[Obsolete("This method is obsolete. Use \"CustomParameters\" property")]
		public void SetCustomParametersArray(String[] value)
		{
			Dictionary<String, String> pars = new Dictionary<String, String>();
			for (int f = 0; f < value.Length; f += 2)
			{
				pars.Add(value[f], value[f + 1]);
			}

			adserverRequest.SetCustomParameters(pars);
		}

		/// <summary>
		/// Set custom parameter
		/// </summary>
		/// <param name="strName">Custom parameter name</param>
		/// <param name="strValue">Custom parameter value</param>
		/// <return></return>
		void SetCustomParameter(String strName, String strValue)
		{
			adserverRequest.SetCustomParameter(strName, strValue);
		}

		/// <summary>
		/// Remove custom parameter
		/// </summary>
		/// <param name="strName">Custom parameter name</param>
		/// <return>TRUE if found and removed, otherwise FALSE</return>
		bool RemoveCustomParameter(String strName)
		{
			return adserverRequest.RemoveCustomParameter(strName);
		}

		/// <summary>
		/// Remove custom parameter
		/// </summary>
		/// <param name="nIdx">Custom parameter index</param>
		/// <return>TRUE if found and removed, otherwise FALSE</return>
		bool RemoveCustomParameter(int nIdx)
		{
			return adserverRequest.RemoveCustomParameter(nIdx);
		}

		/// <summary>
		/// Get custom parameter
		/// </summary>
		/// <param name="strName">Custom parameter name</param>
		/// <return>Custom parameter value (if found, otherwise empty string)</return>
		String GetCustomParameter(String strName)
		{
			return adserverRequest.GetCustomParameter(strName);
		}

		/// <summary>
		/// Get custom parameter
		/// </summary>
		/// <param name="nIdx">Custom parameter index</param>
		/// <return>Custom parameter value (if found, otherwise empty string)</return>
		String GetCustomParameter(int nIdx)
		{
			return adserverRequest.GetCustomParameter(nIdx);
		}

		/// <summary>
		/// Immediately update banner contents
		/// </summary>
		public void Update()
		{
			InitAdserverRequest();
			contentThread = new Thread(new ThreadStart(ContentThreadProc));
			if (!contentThread.IsAlive) contentThread.Start();
		}

		/// <summary>
		/// Dispose object
		/// </summary>
		/// <param name="disposing"></param>
		/// 
		~AdView()
		{
			//thirdPartyContainer.Stop();
			AutoDetectParameters.Release();
			m_webBrowser.Navigating -= new EventHandler<NavigatingEventArgs>(AdViewControl_Navigating);
		}
		#endregion

		#region "Private methods"
		private void FirstAppLaunchProc()
		{
			try
			{
				//if ((_appId != null) && (_appId.Length > 0))
				{
					object temp = null;

					String deviceuid = (DeviceExtendedProperties.TryGetValue("DeviceUniqueID", out temp) ? (temp as String) : String.Empty);

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

						if (!String.IsNullOrEmpty(deviceuid) &&
							!String.IsNullOrEmpty(_advertiserId) &&
							!String.IsNullOrEmpty(_groupCode))
						{
							string udid = MD5Core.GetHashString(deviceuid);
							request += "?udid=" + HttpUtility.UrlEncode(udid);
							request += "&advertiser_id=" + HttpUtility.UrlEncode(_advertiserId);
							request += "&group_code=" + HttpUtility.UrlEncode(_groupCode);
						}

						DataRequest.SendGetRequest(FIRST_APP_LAUNCH_URL + request, String.Empty);

						IsolatedStorageSettings.ApplicationSettings[SETTINGS_IS_FIRST_APP_LAUNCH] = true;
					}
				}
			}
			catch (Exception)
			{}
		}

		private void InitOrmmaLibrary()
		{
//*
			for (int i = 0; i < (int)EOrmmaState.COUNT; i++)
			{
				string stateName = Enum.GetName(typeof(EOrmmaState), i);
				string stateDeclaration = "var ORMMA_STATE_" + stateName.ToUpper() + " = \"" + stateName.ToLower() + "\";\n";
				ormmaLibText += stateDeclaration;
			}

			for (int i = 0; i < (int)EOrmmaEvents.COUNT; i++)
			{
				string eventName = Enum.GetName(typeof(EOrmmaEvents), i);
				string eventDeclaration = "var ORMMA_EVENT_" + eventName.ToUpper();

				// state_name -> stateName
				try
				{
					eventName = eventName.ToLower();
					string next = eventName.Substring(eventName.IndexOf('_') + 1, 1);
					eventName = eventName.Replace("_" + next, next.ToUpper());
				}
				catch (System.Exception ex)
				{
					System.Diagnostics.Debug.WriteLine(ex.Message);
				}

				eventDeclaration += " = \"" + eventName + "\";\n";
				ormmaLibText += eventDeclaration;
			}
//*/
			ormmaLibText += MojivaPhone.Resources.ormmalib;
			string defPosStr = 
			"DEFAULT_POSITION = {x: " + m_webBrowser.Margin.Left +
								", y: " + m_webBrowser.Margin.Top +
								", width: " + m_webBrowser.DesiredSize.Width +
								", height: " + m_webBrowser.DesiredSize.Height +
								"}";

			ormmaLibText = ormmaLibText.Replace("DEFAULT_POSITION = {}", defPosStr);

			ormmaLibText = "<script type=\"text/javascript\">" + ormmaLibText + "</script>";
		}

		private void OnUpdateLocation(object sender, EventArgs e)
		{
			Latitude = adserverRequest.getLatitude();
			Longitude = adserverRequest.getLongitude();
			string accuracy = AutoDetectParameters.Instance.Accuracy.ToString();
			FireEvent("locationChange", new string[] { Latitude, Longitude, accuracy });
		}

		private void OnUpdateCourse(object sender, EventArgs e)
		{
			Course = adserverRequest.getCourse();
			FireEvent("headingChange", new string[] { Course });
		}

		private delegate void SetDocumentPathDelegate(String path);

		private void SetDocumentPath(String path)
		{
			Dispatcher.BeginInvoke(new SetDocumentPathDelegate(SetDocumentPathSync), path);
		}

		private void SetDocumentPathSync(String path)
		{
			if (_allowNavi)
			{
				_navigating = true;
				m_webBrowser.Navigate(new Uri("Cache\\" + path, UriKind.Relative));
			}
		}

		private delegate void SetDefaultImageDelegate();

		private void SetDefaultImage()
		{
			Dispatcher.BeginInvoke(new SetDefaultImageDelegate(SetDefaultImageSync));
		}

		private void SetDefaultImageSync()
		{
			String image = _viewId + "_default.jpg";
			String idCache = this.GetType().Name + "_" + _viewId.ToString() + ".html";

			using (IsolatedStorageFile appStorage = IsolatedStorageFile.GetUserStoreForApplication())
			{
				using (IsolatedStorageFileStream stream = appStorage.CreateFile("Cache\\" + image))
				{
					StreamResourceInfo sri = null;
					Uri uri = new Uri(((BitmapImage)_defaultImage.Source).UriSource.ToString(), UriKind.Relative);
					sri = Application.GetResourceStream(uri);
					// Create a new WriteableBitmap object and set it to the JPEG stream.
					BitmapImage bitmap = new BitmapImage();
					bitmap.SetSource(sri.Stream);

					WriteableBitmap wb = new WriteableBitmap(bitmap);
					wb.SaveJpeg(stream, wb.PixelWidth, wb.PixelHeight, 0, 100);
				}

				String data = "<!DOCTYPE html PUBLIC \"-//WAPFORUM//DTD XHTML Mobile 1.0//EN\" \"http://www.wapforum.org/DTD/xhtml-mobile10.dtd\"><body style=\"margin: 0px; padding: 0px;\"><img src=\"" + image + "\"/></body>";
				using (var file = appStorage.OpenFile("Cache\\" + idCache, FileMode.Create))
				using (var writer = new StreamWriter(file))
				{
					writer.Write(data);
				}
			}
			SetDocumentPath(idCache);

		}

		private delegate bool VisibleDelegate();

		private bool GetVisible()
		{
			return (this.Visibility == System.Windows.Visibility.Visible);
		}

		public bool GetVisibleSync()
		{
			if (System.Windows.Deployment.Current.Dispatcher.CheckAccess())
			{
				return (this.Visibility == System.Windows.Visibility.Visible);
			}
			else
			{
				bool b = false;
				ManualResetEvent e = new ManualResetEvent(false);
				System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() => { b = (this.Visibility == System.Windows.Visibility.Visible); e.Set(); });
				e.WaitOne();
				return b;
			}
		}

		private void ContentThreadProc()
		{
			if (this.threadStop.WaitOne(0))
			{
				contentThread = null;
				return;
			}
			if (GetVisibleSync())
			{
				if (String.IsNullOrEmpty(UserAgent))
				{
					_firstEvent.WaitOne();
				}

				String idCache = this.GetType().Name + "_" + _viewId.ToString() + ".html";
				String data = "";

#if USE_CACHE
				if (_useCache)
				{
					using (IsolatedStorageFile appStorage = IsolatedStorageFile.GetUserStoreForApplication())
					{

						if (appStorage.FileExists("Cache\\" + idCache))
						{
							SetDocumentPath(idCache);
						}
						else
						{
							if (_defaultImage != null)
							{
								SetDefaultImage();
							}
						}
					}
				}
#endif
				//*
				try
				{
					InitAdserverRequest();
					string url = adserverRequest.createURL();
					Debug.WriteLine(url);

					OnAdDownloadBegin();

					data = DataRequest.ReadStringData(url);
					if (data == "<!-- invalid params -->")
					{
						OnAdDownloadError(data);
					}
					else if (data.Length > 0)
					{
						data = replaceImages(data);
						OnAdDownloadEnd();
					}
				}
				catch (Exception /*ex*/)
				{ }
//*/

				int currUpdateTime = _reloadPeriod;
				try
				{
					if ((data != null) && (data.Length > 0))
					{
						if (CThirdPartyManager.ContainExternalCampaign(data))
						{
							Deployment.Current.Dispatcher.BeginInvoke(() => mMAdView.Visibility = System.Windows.Visibility.Visible);
							
							if (thirdPartyManager == null)
							{
								thirdPartyManager = new CThirdPartyManager(mMAdView);
							}

							thirdPartyManager.AddExCampaign += new EventHandler<StringEventArgs>(ThirdPartyManager_AddExCampaign);
							thirdPartyManager.Run(data);
							currUpdateTime = Timeout.Infinite;
						}
						else
						{
							Deployment.Current.Dispatcher.BeginInvoke(() => mMAdView.Visibility = System.Windows.Visibility.Collapsed);
						}

						data = "<!DOCTYPE html PUBLIC \"-//WAPFORUM//DTD XHTML Mobile 1.0//EN\" \"http://www.wapforum.org/DTD/xhtml-mobile10.dtd\"><head>" +
							ormmaLibText +
							"</head><body style=\"margin: 0px; padding: 0px; width: 100%; height: 100%\">" + data + "</body></html>";

						saveCache(idCache, data);
						SetDocumentPath(idCache);
					}
				}
				catch (Exception /*ex*/)
				{ }
				SetReloadTimer(currUpdateTime);
			}
			else
			{
				SetReloadTimer(1000);
			}
		}

		private void ThirdPartyManager_AddExCampaign(object sender, StringEventArgs e)
		{
			adserverRequest.AddExCampaign(e.Value);
			SetReloadTimer(_reloadPeriod);
		}

		private delegate void SetReloadTimerDelegate(int nInterval);
		private void SetReloadTimer(int nInterval)
		{
			Dispatcher.BeginInvoke(new SetReloadTimerDelegate(SetReloadTimerDel), nInterval);
		}

		private void SetReloadTimerDel(int nInterval)
		{
			reloadTimer.Change(nInterval, nInterval);
		}

		private void reloadTimer_Tick(Object stateInfo)
		{
			//reloadTimer.Change(Timeout.Infinite, Timeout.Infinite);
			contentThread = new Thread(new ThreadStart(ContentThreadProc));
			contentThread.Start();
		}

		private String readCache(String id)
		{
			String data;
			using (IsolatedStorageFile appStorage = IsolatedStorageFile.GetUserStoreForApplication())
			using (var file = appStorage.OpenFile("Cache\\" + id, FileMode.Open))
			using (var reader = new StreamReader(file))
			{
				data = reader.ReadToEnd();
			}

			return data;
		}

		private void saveCache(String id, String source)
		{
			using (IsolatedStorageFile appStorage = IsolatedStorageFile.GetUserStoreForApplication())
			{
				if (appStorage.FileExists("Cache\\" + id)) appStorage.DeleteFile("Cache\\" + id);
				using (var file = appStorage.OpenFile("Cache\\" + id, FileMode.Create))
				using (var writer = new StreamWriter(file))
				{
					writer.Write(source);
				}
			}

			return;
		}

		private string GetMD5(string stringToHash)
		{
			// Create a hash value from the array
			byte[] hash = MD5Core.GetHash(stringToHash);

			// Convert a hash to a string
			string s = string.Empty;
			foreach (byte b in hash)
				s += b.ToString("x2");

			return s;
		}

		private String replaceImages(String source)
		{
#if USE_CACHE
			Regex imgPattern = new Regex("<img([^>]*[^/])", RegexOptions.IgnoreCase);

			String srcData;
			//String replacementImgTag;
			Regex srcPattern = new Regex("(src\\s*=\\s*(\"|').*?(\"|'))|(src\\s*=.*?\\s)", RegexOptions.IgnoreCase);

			MatchCollection imgMatcher = imgPattern.Matches(source);
			String newSrc = source;

			foreach (Match imgItem in imgMatcher)
			{
				MatchCollection srcMatcher = srcPattern.Matches(imgItem.Value);

				foreach (Match srcItem in srcMatcher)
				{
					srcData = srcItem.Value.Replace("src", "");
					srcData = srcData.Replace("\"", "");
					srcData = srcData.Replace("'", "");
					srcData = srcData.Replace("=", "");
					srcData = srcData.Trim();

					String imagePath = "";

					byte[] imageBytes = null;
					try
					{
						int idx = srcData.LastIndexOf('.');
						String ext = "";
						if (idx != -1) ext = srcData.Substring(idx, srcData.Length - idx);
						imageBytes = readImageContent(srcData);
						imagePath = GetMD5(srcData);
						imagePath += ext;
						using (IsolatedStorageFile appStorage = IsolatedStorageFile.GetUserStoreForApplication())
						{
							if (appStorage.FileExists("\\Cache\\" + imagePath)) appStorage.DeleteFile("\\Cache\\" + imagePath);
							using (var file = appStorage.OpenFile("\\Cache\\" + imagePath, FileMode.Create))
							using (var writer = new BinaryWriter(file))
							{
								writer.Write(imageBytes);
							}
							using (var file = appStorage.OpenFile("\\Cache\\" + imagePath, FileMode.Open))
							{
								int a = 10;
							}

						}
					}
					catch (Exception)
					{
						imagePath = "";
					}

					newSrc = newSrc.Replace(srcItem.Value, "src=\"" + imagePath + "\"");
				}
			}


			return newSrc;
#else
			return source;
#endif
		}

		private static byte[] readImageContent(String url)
		{
			return DataRequest.ReadByteData(url);
		}

		protected static Color ColorFromRgb(String rgb)
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

		protected static String ColorToRgb(Color color)
		{
			return "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
		}

		private static void OpenExternalUrl(String str)
		{
			WebBrowserTask task = new WebBrowserTask();
			task.URL = str;
			task.Show();
		}

		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			_allowNavi = true;
		}

		private void UserControl_Unloaded(object sender, RoutedEventArgs e)
		{
			_allowNavi = false;
		}

		private void m_webBrowser_Navigated(object sender, NavigationEventArgs e)
		{
			if (_first)
			{
				_first = false;
				Dispatcher.BeginInvoke(new getUserAgentDelegate(getUserAgent));
			}
		}

		private delegate void getUserAgentDelegate();

		private void getUserAgent()
		{
			try
			{
				UserAgent = (String)m_webBrowser.InvokeScript("getUA");
			}
			catch (System.Exception /*ex*/)
			{}
			_firstEvent.Set();
		}

		private void WebBrowser_ScriptNotify(object sender, NotifyEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine("Notify " + e.Value);
			ProcessScriptNotify(e.Value);
		}

		private void ProcessScriptNotify(string msg)
		{
			string[] msgParts = msg.Split(new char[] { '|' });
			List<string> args = null;

			if (msgParts.Length > 1)			// example: Open(url)
			{
				args = new List<string>(msgParts);
				args = args.GetRange(1, args.Count - 1);
			}

			ProcessFunction(msgParts[0].ToLower(), args);
		}

		private void ProcessFunction(string funcName, List<string> args)
		{
			if (funcName == "makecall" ||
				funcName == "sendsms" ||
				funcName == "sendmail")
			{
				nativeAppManager.RunApp(funcName, args);
				return;
			}

			MethodInfo methodInfo = null;
			try
			{
				methodInfo = this.GetType().GetMethod(funcName, BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Instance);
			}
			catch (System.Exception /*ex*/)
			{
				return;
			}

			if (methodInfo != null)
			{
				if (args == null)
				{
					methodInfo.Invoke(this, null);
				}
				else
				{
					try													// try integer arguments
					{
						List<object> intArgs = new List<object>();
						for (int i = 0; i < args.Count; i++)
						{
							intArgs.Add((object)Convert.ToInt32(args[i], 10));
						}
						methodInfo.Invoke(this, intArgs.ToArray());
					}
					catch (System.FormatException /*fmtEx*/)
					{
						try												// try string arguments
						{
							List<object> strArgs = new List<object>();
							for (int i = 0; i < args.Count; i++)
							{
								strArgs.Add((object)args[i]);
							}
							methodInfo.Invoke(this, strArgs.ToArray());
						}
						catch (System.Exception /*ex*/)
						{ }
					}
					catch (System.Exception /*ex*/)
					{ }
				}
			}
		}

		private void FireEvent(string eventName, string[] args)
		{
			//System.Diagnostics.Debug.WriteLine("event " + eventName);
			try
			{
				string[] allArgs = new string[args.Length + 1];
				allArgs[0] = eventName;

				if (eventName == "networkChange")
				{
					string netWorkStatus = args[1];
					switch (netWorkStatus)
					{
						case "offline":
							assetManager.StopAssetStoring();
							assetManager.SaveTextFile(ASSETS_INDEX_FILENAME, m_webBrowser.SaveToString());
							m_webBrowser.Base = ASSETS_ROOT_DIR;
							m_webBrowser.Navigate(new Uri(ASSETS_INDEX_FILENAME, UriKind.Relative));
							break;
						case "wifi":
						case "cell":
							assetManager.StartAssetStoring();
							break;
						default:
							break;
					}
				}

				args.CopyTo(allArgs, 1);

				if (Deployment.Current.Dispatcher.CheckAccess())
				{
					m_webBrowser.InvokeScript("fireEvent", allArgs);
				}
				else
				{
					Deployment.Current.Dispatcher.BeginInvoke(() => m_webBrowser.InvokeScript("fireEvent", allArgs));
				}
			}
			catch (System.Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
			}
		}

		private void MainPage_OrientationChanged(object sender, OrientationChangedEventArgs e)
		{
			FireEvent("orientationChange", new string[] { GetOrientationDegree().ToString() });
		}

		private void GetOrientation()
		{
			m_webBrowser.InvokeScript("setTmpObj", GetOrientationDegree().ToString());
		}

		private int GetOrientationDegree()
		{
			int orientation = -1;

			if (Owner != null)
			{
				switch (Owner.Orientation)
				{
					case PageOrientation.PortraitUp:
						orientation = 0;
						break;
					case PageOrientation.PortraitDown:
						orientation = 180;
						break;
					case PageOrientation.LandscapeLeft:
						orientation = 270;
						break;
					case PageOrientation.LandscapeRight:
						orientation = 90;
						break;
				}
			}

			return orientation;
		}

		private void Open(string url)
		{
			OpenExternalUrl(url);
		}

		private void Show()
		{
			//System.Diagnostics.Debug.WriteLine("show");
			m_webBrowser.Visibility = System.Windows.Visibility.Visible;
		}

		private void Hide()
		{
			//System.Diagnostics.Debug.WriteLine("hide");
			m_webBrowser.Visibility = System.Windows.Visibility.Collapsed;
		}

		private void Close()
		{
			//SetDefaultDimensions();
			if (expandedView != null)
			{
				expandedView.Close();
				expandedView = null;

				accelerometer.StopAccelListener();
			}
		}

		protected void Resize(int width, int height)
		{
			this.Width = width;
			this.Height = height;
			m_webBrowser.Width = width;
			m_webBrowser.Height = height;
		}

		private void Expand(string posStr, string propStr, string url)
		{
			//System.Diagnostics.Debug.WriteLine("expand");
			accelerometer.StartAccelListener();

			Uri uri;
			if (url == "null")
			{
				uri = m_webBrowser.Source;
			}
			else
			{
				uri = new Uri(url, UriKind.RelativeOrAbsolute);
			}

			if (expandedView == null)
			{
				expandedView = new CExpandedView(posStr, propStr, uri);
				expandedView.Show();
			}
		}

		private void GetMaxSize()
		{
			m_webBrowser.InvokeScript("setTmpObj", this.LayoutRoot.Width.ToString(), this.LayoutRoot.Height.ToString());
		}

		private void GetSize()
		{
			m_webBrowser.InvokeScript("setTmpObj", m_webBrowser.DesiredSize.Width.ToString(), m_webBrowser.DesiredSize.Height.ToString());
		}

		private void GetScreenSize()
		{
			m_webBrowser.InvokeScript("setTmpObj", Application.Current.Host.Content.ActualWidth.ToString(), Application.Current.Host.Content.ActualHeight.ToString());
		}

		private void GetNetwork()
		{
			Deployment.Current.Dispatcher.BeginInvoke(() => m_webBrowser.InvokeScript("setTmpObj", AutoDetectParameters.Instance.NetworkType));
		}

		private void NetworkChanged(object sender, EventArgs e)
		{
// 			System.Diagnostics.Debug.WriteLine(Microsoft.Phone.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable().ToString());
// 			System.Diagnostics.Debug.WriteLine(AutoDetectParameters.Instance.NetworkType);

			Deployment.Current.Dispatcher.BeginInvoke(() => FireEvent("networkChange", new string[] { Microsoft.Phone.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable().ToString(), AutoDetectParameters.Instance.NetworkType }));
		}

		private void GetHeading()
		{
			if (!String.IsNullOrEmpty(Course))
			{
				m_webBrowser.InvokeScript("setTmpObj", Course);
			}
			else
			{
				m_webBrowser.InvokeScript("setTmpObj", "-1");
			}
		}

		private void GetLocation()
		{
			m_webBrowser.InvokeScript("setTmpObj", new string[] { Latitude, Longitude, "0" });
		}

		private void GetTilt()
		{
			m_webBrowser.InvokeScript("setTmpObj", accelerometer.TiltToStringArr());
		}

		private void AddAsset(string url, string alias)
		{
			assetManager.AddAsset(url, alias);
		}

		private void RemoveAsset(string alias)
		{
			assetManager.RemoveAsset(alias);
		}

		private void RemoveAllAssets()
		{
			assetManager.RemoveAllAssets();
		}

		private void GetAssetURL(string alias)
		{
			m_webBrowser.InvokeScript("setTmpObj", assetManager.GetAssetURL(alias));
		}

		private void GetCacheRemaining()
		{
			m_webBrowser.InvokeScript("setTmpObj", assetManager.GetCacheRemaining().ToString());
		}

		private void Request(string uri, string display)
		{
			m_webBrowser.InvokeScript("setTmpObj", assetManager.Request(uri, display).ToString());
		}

		private void StorePicture(string url)
		{
			assetManager.StorePicture(url);
		}

		private void SetShakeProperties(int intensity, int interval)
		{
			if (accelerometer != null)
			{
				accelerometer.SetProperties(intensity, interval);
			}
		}

		private void StartShakeListener()
		{
			accelerometer.StartShakeListener();
		}

		private void Owner_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (internalBrowserController != null && internalBrowserController.IsShow)
			{
				internalBrowserController.Closed += new EventHandler<StringEventArgs>(internalBrowserController_Closed);
				internalBrowserController.Close();
				e.Cancel = true;
			}
		}

		private void internalBrowserController_Closed(object sender, StringEventArgs e)
		{
			OnAdWebViewClosing(e.Value);
		}

		#endregion

		#region "Protected methods"

		protected void AdViewControl_Navigating(object sender, Microsoft.Phone.Controls.NavigatingEventArgs e)
		{
			if (!_navigating)
			{
				if (_internalBrowser)
				{
					bool isNaviHandled = OnAdNavigateBanner(e.Uri.ToString());
					if (!isNaviHandled && internalBrowserController != null && Owner != null)
					{
						internalBrowserController.Show(e.Uri.ToString());
					}
					e.Cancel = true;
				}
				else
				{
					OpenExternalUrl(e.Uri.ToString());
				}
			}
			else
			{
				_navigating = false;
			}
		}

		#endregion

		#region "Events"
		public delegate bool AdNavigateBannerDelegate(String url);
		private event AdNavigateBannerDelegate AdNavigateBannerEvent;
		

		private event EventHandler AdDownloadBeginEvent;
		private event EventHandler AdDownloadEndEvent;
		private event EventHandler<StringEventArgs> AdDownloadErrorEvent;
		private event EventHandler<StringEventArgs> AdWebViewClosingEvent;

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

		private void OnAdWebViewClosing(string url)
		{
			if (AdWebViewClosingEvent != null)
			{
				AdWebViewClosingEvent(this, new StringEventArgs(url));
			}
		}

		private bool OnAdNavigateBanner(string url)
		{
			if (AdNavigateBannerEvent != null)
			{
				return AdNavigateBannerEvent(url);
			}

			return false;
		}


		/// <summary>
		/// This event fired after user clicks the banner (only if InternalBrowser property has been set to True).
		/// </summary>
		public event AdNavigateBannerDelegate AdNavigateBanner
		{
			add { AdNavigateBannerEvent += value; }
			remove { AdNavigateBannerEvent -= value; }
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

		/// <summary>
		/// This event fired after in app web view closing
		/// </summary>
		public event EventHandler<StringEventArgs> AdWebViewClosing
		{
			add { AdWebViewClosingEvent += value; }
			remove { AdWebViewClosingEvent -= value; }
		}

		/// <summary>
		/// Event that is raised when GPS latitude found
		/// </summary>
		public event EventHandler GetLatitude
		{
			add { AutoDetectParameters.Instance.GetLatitude += value; }
			remove { AutoDetectParameters.Instance.GetLatitude -= value;}
		}

		/// <summary>
		/// Event that is raised when GPS longitude found
		/// </summary>
		public event EventHandler GetLongitude
		{
			add { AutoDetectParameters.Instance.GetLongitude += value; }
			remove { AutoDetectParameters.Instance.GetLongitude -= value; }
		}

		/// <summary>
		/// Event that is raised when ip address found
		/// </summary>
		public event EventHandler GetIp
		{
			add { AutoDetectParameters.Instance.GetIp += value; }
			remove { AutoDetectParameters.Instance.GetIp -= value; }
		}

		/// <summary>
		/// Event that is raised when carrier found
		/// </summary>
		public event EventHandler GetCarrier
		{
			add { AutoDetectParameters.Instance.GetCarrier += value; }
			remove{ AutoDetectParameters.Instance.GetCarrier -= value; }
		}

		/// <summary>
		/// Event that is raised when country found
		/// </summary>
		public event EventHandler GetCountry
		{
			add { AutoDetectParameters.Instance.GetCountry += value; }
			remove { AutoDetectParameters.Instance.GetCountry -= value; }
		}

		#endregion
	}
}
