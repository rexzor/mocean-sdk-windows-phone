/****
 * © 2010-2011 mOcean Mobile. A subsidiary of Mojiva, Inc. All Rights Reserved.
 * */

using System;
using System.Device.Location;
using System.Threading;
using Microsoft.Phone.Info;
using Microsoft.Phone.Net.NetworkInformation;

namespace mOceanWindowsPhone
{
	internal class AutoDetectParameters
	{
		private const double LOCATION_CHANGE_DIFFERENCE = 0.1;
		#region "Variables"
		private static readonly object padlock = new object();
		private static AutoDetectParameters instance = null;
		private IGeoPositionWatcher<GeoCoordinate> geoWatcher;
		private GeoCoordinate prevLocation = null;
		private GeoCoordinate location = null;
		private int course = -1;
		private NetworkInterfaceType prevNetworkType = NetworkInterfaceType.None;
		private NetworkInterfaceType currentNetworkType = NetworkInterfaceType.None;

		private volatile string country = null;
		private volatile string deviceId = null;
		private Thread networkDetectThread = null;
		private Thread initHardwareParamsThread = null;
		private static volatile int references = 0;
		private const int NETWORK_CHECK_PERIOD = 100;
		private volatile bool needExit = false;
		#endregion

		#region "Public methods"
		public static void AddReferense()
		{
			references++;
		}

		public static void Release()
		{
			references--;
			if (references <= 0)
			{
				if (instance != null)
				{
					instance = null;
				}
			}
		}
		#endregion

		#region "Properties"
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

		public double Latitude
		{
			get
			{
				if (location != null)
				{
					return location.Latitude;
				}

				return Double.NaN;
			}
		}

		public double Longitude
		{
			get
			{
				if (location != null)
				{
					return location.Longitude;
				}

				return Double.NaN;
			}
		}

		public string Country
		{
			get
			{
				return country;
			}
		}

		public string ConnectionSpeed
		{
			get
			{
				switch (currentNetworkType)
				{
					case NetworkInterfaceType.Wireless80211:
					case NetworkInterfaceType.MobileBroadbandCdma:
						return "1";
					default:
						return "0";
				}
			}
		}

		public string DeviceId
		{
			get { return deviceId; }
			set { deviceId = value; }
		}
		#endregion

		#region "Events"
		private event EventHandler GetDeviceIdEvent = null;

		public delegate void NetworkChangedEventHandler(NetworkInterfaceType networkType);
		private event NetworkChangedEventHandler NetworkChangedEvent = null;

		public delegate void LocationChangedEventHandler(double latitude, double longitude, double accuracy);
		private event LocationChangedEventHandler LocationChangedEvent = null;

		public delegate void CourseChangedEventHandler(int course);
		private event CourseChangedEventHandler CourseChangedEvent = null;

		private event EventHandler GpsSensorDisabledEvent = null;

		protected virtual void OnNetworkChanged()
		{
			if (NetworkChangedEvent != null)
			{
				NetworkChangedEvent(currentNetworkType);
			}
		}
		protected virtual void OnGetDeviceId()
		{
			if (GetDeviceIdEvent != null)
			{
				GetDeviceIdEvent(this, EventArgs.Empty);
			}
		}
		protected virtual void OnLocationChanged()
		{
			if (LocationChangedEvent != null && location != null)
			{
				LocationChangedEvent(location.Latitude, location.Longitude, location.HorizontalAccuracy);
			}
		}
		protected virtual void OnCourseChanged()
		{
			if (CourseChangedEvent != null)
			{
				CourseChangedEvent(course);
			}
		}
		protected virtual void OnGpsSensorDisabled()
		{
			if (GpsSensorDisabledEvent != null)
			{
				GpsSensorDisabledEvent(null, EventArgs.Empty);
			}
		}

		public event NetworkChangedEventHandler NetworkChanged
		{
			add { NetworkChangedEvent += value; }
			remove { NetworkChangedEvent -= value; }
		}
		public event EventHandler GetDeviceId
		{
			add { GetDeviceIdEvent += value; }
			remove { GetDeviceIdEvent -= value; }
		}
		public event LocationChangedEventHandler LocationChanged
		{
			add { LocationChangedEvent += value; }
			remove { LocationChangedEvent -= value; }
		}
		public event CourseChangedEventHandler CourseChanged
		{
			add { CourseChangedEvent += value; }
			remove { CourseChangedEvent -= value; }
		}
		public event EventHandler GpsSensorDisabled
		{
			add { GpsSensorDisabledEvent += value; }
			remove { GpsSensorDisabledEvent -= value; }
		}
		#endregion

		#region "Private methods"
		// Explicit static constructor to tell C# compiler not to mark type as beforefieldinit
		static AutoDetectParameters()
		{}

		AutoDetectParameters()
		{
			StartLocationService();

			initHardwareParamsThread = new Thread(new ThreadStart(InitHardwareParametersThreadProc));
			initHardwareParamsThread.Name = "InitHardwareParametersThreadProc";
			initHardwareParamsThread.Start();

			networkDetectThread = new Thread(new ThreadStart(NetworkDetectThreadProc));
			networkDetectThread.Name = "NetworkDetectThreadProc";
			networkDetectThread.Start();
		}

		~AutoDetectParameters()
		{
			needExit = true;
			geoWatcher = null;
			initHardwareParamsThread.Join();
			initHardwareParamsThread = null;

			networkDetectThread.Join();
			networkDetectThread = null;
		}

		private void StartLocationService()
		{
			if (geoWatcher == null)
			{
				geoWatcher = new GeoCoordinateWatcher(GeoPositionAccuracy.Default);
				if (geoWatcher.Status == GeoPositionStatus.Disabled)
				{
					OnGpsSensorDisabled();
				}
				geoWatcher.StatusChanged += new EventHandler<GeoPositionStatusChangedEventArgs>(GeoWatcher_StatusChanged);
				geoWatcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(GeoWatcher_PositionChanged);
			}

			geoWatcher.Start();
		}

		private void GeoWatcher_StatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
		{
			if (e.Status == GeoPositionStatus.Disabled)
			{
				OnGpsSensorDisabled();
			}
		}

		private void NetworkDetectThreadProc()
		{
			while (true)
			{
				currentNetworkType = NetworkInterface.NetworkInterfaceType;		// ~20 seconds

				if (currentNetworkType != prevNetworkType)
				{
					OnNetworkChanged();
				}

				prevNetworkType = currentNetworkType;

				if (needExit)
				{
					return;
				}

				Thread.Sleep(NETWORK_CHECK_PERIOD);
			}
		}

		private void InitHardwareParametersThreadProc()
		{
			if (needExit)
			{
				return;
			}

			try
			{
				object outParam = null;
				if (DeviceExtendedProperties.TryGetValue("DeviceUniqueId", out outParam))
				{
					byte[] uidData = outParam as byte[];
					deviceId = MD5Core.GetHashString(System.Text.Encoding.UTF8.GetString(uidData, 0, uidData.Length));
				}
			}
			catch (System.Exception)
			{
				deviceId = null;
			}
			finally
			{
				OnGetDeviceId();
			}
		}

		private void GeoWatcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
		{
			GeoPosition<GeoCoordinate> position = e.Position;

			location = position.Location;

			if (prevLocation == null)
			{
				prevLocation = location;
				OnLocationChanged();
			}
			else if (Math.Abs(location.Latitude - prevLocation.Latitude) >= LOCATION_CHANGE_DIFFERENCE ||
					Math.Abs(location.Longitude - prevLocation.Longitude) >= LOCATION_CHANGE_DIFFERENCE)
			{
				OnLocationChanged();
				prevLocation = location;
			}

			if (!Double.IsNaN(location.Course))
			{
				course = (int)location.Course;
				OnCourseChanged();
			}
		}
		#endregion
	}
}

