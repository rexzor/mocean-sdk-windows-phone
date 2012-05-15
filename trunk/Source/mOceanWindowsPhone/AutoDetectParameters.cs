/****
 * © 2010-2011 mOcean Mobile. A subsidiary of Mojiva, Inc. All Rights Reserved.
 * */

using System;
using System.Device.Location;
using System.Threading;
using Microsoft.Phone.Info;
using Microsoft.Phone.Net.NetworkInformation;
using System.Diagnostics;

namespace mOceanWindowsPhone
{
	internal class AutoDetectParameters
	{
		private static readonly object detectorLocker = new object();
		private static AutoDetectParameters instance = null;
		private static volatile int references = 0;

		public static string GetDeviceId()
		{
			string deviceUniqueId = null;
			try
			{
				object outParam = null;
				if (DeviceExtendedProperties.TryGetValue("DeviceUniqueId", out outParam))
				{
					byte[] uidData = outParam as byte[];
					deviceUniqueId = System.Text.Encoding.UTF8.GetString(uidData, 0, uidData.Length);
				}
			}
			catch (Exception)
			{ }

			return deviceUniqueId;
		}

		#region Instance
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

		public static AutoDetectParameters Instance
		{
			get
			{
				lock (detectorLocker)
				{
					if (instance == null)
					{
						instance = new AutoDetectParameters();
					}
					return instance;
				}
			}
		}

		// Explicit static constructor to tell C# compiler not to mark type as beforefieldinit
		static AutoDetectParameters()
		{ }

		AutoDetectParameters()
		{
			//StartLocationService();
			StartNetworkService();
		}

		~AutoDetectParameters()
		{
			StopLocationService();
			StopNetworkService();
		}
		#endregion

		#region Location
		private static IGeoPositionWatcher<GeoCoordinate> geoWatcher = null;
		private static GeoCoordinate prevLocation = null;
		private static GeoCoordinate location = null;
		private static int course = -1;

        private bool locationDetection = false;
        public bool LocationDetection
        {
            get { return locationDetection; }
            set
            {
                locationDetection = value;
                if ((locationDetection == true) && (geoWatcher == null))
                {
                    StartLocationService();
                }
                else if (locationDetection == false)
                {
                    StopLocationService();
                }
            }
        }

        private double locationMinMoveMeters = 1000.0; // old value was 0.1, leading to constant updates
        public double LocationMinMoveMeters
        {
            get { return locationMinMoveMeters; }
            set { locationMinMoveMeters = value; }
        }

		private void StartLocationService()
		{
            Debug.WriteLine("Location services enabled");
			geoWatcher = new GeoCoordinateWatcher(GeoPositionAccuracy.Default);
			if (geoWatcher.Status == GeoPositionStatus.Disabled)
			{
				OnGpsSensorDisabled();
			}
			geoWatcher.StatusChanged += new EventHandler<GeoPositionStatusChangedEventArgs>(GeoWatcher_StatusChanged);
			geoWatcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(GeoWatcher_PositionChanged);

			geoWatcher.Start();
		}

		private void StopLocationService()
		{
            Debug.WriteLine("Location services disabled");
			geoWatcher = null;
		}

		private void GeoWatcher_StatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
		{
			if (e.Status == GeoPositionStatus.Disabled)
			{
				OnGpsSensorDisabled();
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
            else if (Math.Abs(location.Latitude - prevLocation.Latitude) >= LocationMinMoveMeters ||
                     Math.Abs(location.Longitude - prevLocation.Longitude) >= LocationMinMoveMeters)
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

		public delegate void LocationChangedEventHandler(double latitude, double longitude, double accuracy);
		public event LocationChangedEventHandler LocationChanged = null;
		protected virtual void OnLocationChanged()
		{
			LocationChangedEventHandler handler = LocationChanged;
			if (handler != null)
			{
				try
				{
					handler(location.Latitude, location.Longitude, location.HorizontalAccuracy);
				}
				catch (Exception)
				{ }
			}
		}

		public delegate void CourseChangedEventHandler(int course);
		public event CourseChangedEventHandler CourseChanged = null;
		protected virtual void OnCourseChanged()
		{
			CourseChangedEventHandler handler = CourseChanged;
			if (handler != null)
			{
				try
				{
					handler(course);
				}
				catch (Exception)
				{ }
			}
		}

 		public event EventHandler GpsSensorDisabled = null;
		protected virtual void OnGpsSensorDisabled()
		{
			EventHandler handler = GpsSensorDisabled;
			if (handler != null)
			{
				try
				{
					handler(Instance, EventArgs.Empty);
				}
				catch (Exception)
				{ }
			}
		}

		public double Latitude
		{
			get { return location != null ? location.Latitude : Double.NaN; }
		}

		public double Longitude
		{
			get { return location != null ? location.Longitude : Double.NaN; }
		}
		#endregion

		#region Network
		private const int NETWORK_CHECK_PERIOD = 100;
		private Thread networkServiceThread = null;
		private NetworkInterfaceType prevNetworkType = NetworkInterfaceType.None;
		private NetworkInterfaceType currentNetworkType = NetworkInterfaceType.None;
		private volatile bool stopNetworkService = false;

		private void StartNetworkService()
		{
			networkServiceThread = new Thread(new ThreadStart(NetworkServiceThreadProc));
			networkServiceThread.Name = "NetworkDetectThreadProc";
			networkServiceThread.Start();
		}

		private void StopNetworkService()
		{
			stopNetworkService = true;
			networkServiceThread.Join();
			networkServiceThread = null;
		}

		private void NetworkServiceThreadProc()
		{
			while (true)
			{
				currentNetworkType = NetworkInterface.NetworkInterfaceType;		// ~20 seconds

				if (currentNetworkType != prevNetworkType)
				{
					OnNetworkChanged();
				}

				prevNetworkType = currentNetworkType;

				if (stopNetworkService)
				{
					return;
				}

				Thread.Sleep(NETWORK_CHECK_PERIOD);
			}
		}

		public delegate void NetworkChangedEventHandler(NetworkInterfaceType networkType);
		public event NetworkChangedEventHandler NetworkChanged = null;
		protected virtual void OnNetworkChanged()
		{
			NetworkChangedEventHandler handler = NetworkChanged;
			if (handler != null)
			{
				try
				{
					handler(currentNetworkType);
				}
				catch (Exception)
				{ }
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

		public NetworkInterfaceType CurrentNetworkType
		{
			get { return currentNetworkType; }
			private set { }
		}
		#endregion
	}
}

