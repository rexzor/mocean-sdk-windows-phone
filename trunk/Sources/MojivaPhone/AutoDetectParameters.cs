/****
 * © 2010-2011 mOcean Mobile. A subsidiary of Mojiva, Inc. All Rights Reserved.
 * */

using System;
using System.Collections.Generic;
using System.Text;
using System.Device.Location;
using System.Globalization;
using System.Threading;

namespace MojivaPhone
{
    /// <summary>
    /// Singleton class for detect parameters
    /// </summary>
    internal sealed class AutoDetectParameters
    {
        static readonly object padlock = new object();
        static AutoDetectParameters instance = null;
        private bool _haveGps = false;
        IGeoPositionWatcher<GeoCoordinate> _watcher;
        GeoPosition<GeoCoordinate> _position;
		//private CNetwork network;

        private String _networkOperatorName;
        private String _country;
        private String _ipaddress;
        private Thread _autoDetectThread;
        private static int _references = 0;

        private event EventHandler GetLatitudeEvent;
        private event EventHandler GetLongitudeEvent;
		private event EventHandler GetCourseEvent;
        private event EventHandler GetIpEvent;
        private event EventHandler GetCarrierEvent;
        private event EventHandler GetCountryEvent;
		private event EventHandler GetNetworkEvent;

        private void StartLocationService()
        {
            if (_watcher == null)
            {
                _watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.Default);
                _watcher.StatusChanged += new EventHandler<GeoPositionStatusChangedEventArgs>(watcher_StatusChanged);
                _watcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(watcher_PositionChanged);
            }
           
            _watcher.Start();
        }

        private void OnGetLatitude(EventArgs e)
        {
			if (GetLatitudeEvent != null)
				GetLatitudeEvent(this, e);
        }

        private void OnGetLongitude(EventArgs e)
        {
            if (GetLongitudeEvent != null)
                GetLongitudeEvent(this, e);
        }

		private void OnGetCourse(EventArgs e)
		{
			if (GetCourseEvent != null)
			{
				GetCourseEvent(this, e);
			}
		}

        private void OnGetIp(EventArgs e)
        {
            if (GetIpEvent != null)
                GetIpEvent(this, e);
        }

        private void OnGetCarrier(EventArgs e)
        {
            if (GetCarrierEvent != null)
                GetCarrierEvent(this, e);
        }

        private void OnGetCountry(EventArgs e)
        {
            if (GetCountryEvent != null)
                GetCountryEvent(this, e);
        }

		private void OnNetworkChange(EventArgs e)
		{
			if (GetNetworkEvent != null)
			{
				GetNetworkEvent(this, e);
			}
		}

        public static void AddRef() { _references++; }
        public static void Release()
        {
            _references--;
            if (_references == 0)
            {
                if (instance != null)
                {
                    instance = null;
                }
            }
        }

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static AutoDetectParameters()
        {
        }

        AutoDetectParameters()
        {
            StartLocationService();

			CNetwork.Instance.NetWorkChange += new EventHandler(NetWorkChanged);

            _autoDetectThread = new Thread(new ThreadStart(AutoDetectParametersThreadProc));
            _autoDetectThread.Start();
        }

        ~AutoDetectParameters()
        {
			CNetwork.Release();

			try
			{
				_autoDetectThread.Abort();
				_autoDetectThread.Join();
			}
			catch (System.Exception /*ex*/)
			{}
			finally
			{
				_autoDetectThread = null;
			}
        }

        public static AutoDetectParameters Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new AutoDetectParameters();
                    }
                    return instance;
                }
            }
        }

        public bool HaveGps
        {
            get
            {
                return _haveGps;
            }
        }

		public double Latitude
		{
			get
			{
				if (_position != null) return _position.Location.Latitude;

				return Double.NaN;
			}
		}

        public double Longitude
        {
            get
            {
                if (_position != null) return _position.Location.Longitude;

				return Double.NaN;
            }
        }

		public double Course
		{
			get
			{
				if (_position != null) return _position.Location.Course;

				return 0;
			}
		}

		public double Accuracy
		{
			get
			{
				if (_position != null) return _position.Location.HorizontalAccuracy;

				return 0;
			}
		}

        public String Carrier
        {
            get
            {
                return _networkOperatorName;
            }
        }

        public String Country
        {
            get
            {
                return _country;
            }
        }

        public String IP
        {
            get
            {
                return _ipaddress;
            }
        }

		public String NetworkType
		{
			get 
			{
				return CNetwork.Instance.GetStatus();
			}
		}

		private void watcher_StatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
            if (e.Status == GeoPositionStatus.Disabled)
                _haveGps = false;
            else _haveGps = true;
        }

		private void watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            _position = e.Position;
            OnGetLatitude(EventArgs.Empty);
            OnGetLongitude(EventArgs.Empty);
			OnGetCourse(EventArgs.Empty);
        }

		private void NetWorkChanged(object sender, EventArgs e)
		{
			OnNetworkChange(EventArgs.Empty);
		}

        private void AutoDetectParametersThreadProc()
        {
            String networkOperatorName = ""; // = Microsoft.WindowsMobile.Status.SystemState.PhoneOperatorName;
            if (!String.IsNullOrEmpty(networkOperatorName))
            {
                _networkOperatorName = networkOperatorName;
                OnGetCarrier(EventArgs.Empty);
            }

            RegionInfo region = new RegionInfo(CultureInfo.CurrentCulture.Name);
            if ((region != null) && !String.IsNullOrEmpty(region.EnglishName))
            {
                _country = region.TwoLetterISORegionName;
                OnGetCountry(EventArgs.Empty);
            }

            try
            {
				String ip = DataRequest.ReadStringData("http://www.whatismyip.com/automation/n09230945.asp").Trim();
                if (!String.IsNullOrEmpty(ip))
                {
                    _ipaddress = ip;
                    OnGetIp(EventArgs.Empty);
                }
            }
            catch (Exception)
            {
            }
        }

        public event EventHandler GetLatitude
        {
            add
            {
                GetLatitudeEvent += value;
            }
            remove
            {
                GetLatitudeEvent -= value;
            }
        }

        public event EventHandler GetLongitude
        {
            add
            {
                GetLongitudeEvent += value;
            }
            remove
            {
                GetLongitudeEvent -= value;
            }
        }

		public event EventHandler GetCourse
		{
			add
			{
				GetCourseEvent += value;
			}
			remove
			{
				GetCourseEvent -= value;
			}
		}

        public event EventHandler GetIp
        {
            add
            {
                GetIpEvent += value;
            }
            remove
            {
                GetIpEvent -= value;
            }
        }

        public event EventHandler GetCarrier
        {
            add
            {
                GetCarrierEvent += value;
            }
            remove
            {
                GetCarrierEvent -= value;
            }
        }

        public event EventHandler GetCountry
        {
            add
            {
                GetCountryEvent += value;
            }
            remove
            {
                GetCountryEvent -= value;
            }
        }

		public event EventHandler GetNetwork
		{
			add
			{
				GetNetworkEvent += value;
			}
			remove
			{
				GetNetworkEvent -= value;
			}
		}
    }
}

