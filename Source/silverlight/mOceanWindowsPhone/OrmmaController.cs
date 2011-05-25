/****
 * © 2010-2011 mOcean Mobile. A subsidiary of Mojiva, Inc. All Rights Reserved.
 * */

using System;
using System.Runtime.Serialization;
using System.Text;
using System.Windows;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Net.NetworkInformation;

namespace mOceanWindowsPhone
{
	internal class OrmmaController
	{
		#region "Variables"
		private AdViewer viewer = null;
		private AdViewer expandedViewer = null;
		private string library = String.Empty;
		private Point defaultPosition = new Point(0, 0);
		private Size defaultSize = new Size(0, 0);
		private Size maxSize = new Size(0, 0);
		private AccelerometerController accelerometer = null;
		private AssetManager assetManager = null;
		private PageOrientation screenOrientation = PageOrientation.None;
		#endregion

		#region "Properties"
		public string Library
		{
			get { return library; }
			protected set { library = value; }
		}
		#endregion

		#region "public"
		public OrmmaController(AdViewer viewer, AdViewer expandedViewer)
		{
			this.viewer = viewer;
			this.expandedViewer = expandedViewer;
			viewer.LoadCompleted += new EventHandler(Viewer_LoadCompleted);
			viewer.ScriptNotify += new AdViewer.ScriptNotifyEventHandler(Viewer_ScriptNotify);
			expandedViewer.ScriptNotify += new AdViewer.ScriptNotifyEventHandler(ExpandedViewer_ScriptNotify);

			library = "<script type=\"text/javascript\">" + mOceanWindowsPhone.Resources.ormmalib + "</script>";

			AutoDetectParameters.Instance.NetworkChanged += new AutoDetectParameters.NetworkChangedEventHandler(NetworkChanged);
			AutoDetectParameters.Instance.LocationChanged += new AutoDetectParameters.LocationChangedEventHandler(LocationChanged);
			AutoDetectParameters.Instance.CourseChanged += new AutoDetectParameters.CourseChangedEventHandler(CourseChanged);

			accelerometer = new AccelerometerController();
			accelerometer.TiltChange += new AccelerometerController.TiltChangeEventHandler(TiltChange);
			accelerometer.Shake += new EventHandler(Shake);

			assetManager = new AssetManager("");
			assetManager.AssetReady += new AssetManager.AssetEventHandler(AssetReady);
			assetManager.AssetRemoved += new AssetManager.AssetEventHandler(AssetRemoved);
			assetManager.Response += new AssetManager.ResponseEventHandler(Response);
		}

		public void SetDefaultPosition(double x, double y, Size size)
		{
			defaultPosition.X = x;
			defaultPosition.Y = y;
			defaultSize = size;
		}

		public void SetMaxSize(Size maxSize)
		{
			this.maxSize = maxSize;
		}

		public void ChangeOrientation(PageOrientation orientation)
		{
			screenOrientation = orientation;

			int degrees = -1;
			switch (orientation)
			{
				case PageOrientation.Portrait:
				case PageOrientation.PortraitUp:
					degrees = 0;
					break;
				case PageOrientation.PortraitDown:
					degrees = 180;
					break;
				case PageOrientation.Landscape:
				case PageOrientation.LandscapeLeft:
					degrees = 270;
					break;
				case PageOrientation.LandscapeRight:
					degrees = 90;
					break;
				default:
					break;
			}

			RunScript("orientation = " + degrees.ToString() + ";");
			RaiseScreenChangeEvent();
		}

		public void ChangeSize(double width, double height)
		{
			RaiseSizeChangeEvent(width, height);
		}

		public void SetExpandedState()
		{
			RunScript("Ormma.setState(ORMMA_STATE_EXPANDED);");
		}
		#endregion

		#region "Events"
		public class VisibilityEventArgs : EventArgs
		{
			public bool Visible = true;
			public VisibilityEventArgs() { }
		}
		private event EventHandler<VisibilityEventArgs> ChangeVisibilityEvent = null;
		private void OnChangeVisibility(VisibilityEventArgs e)
		{
			if (ChangeVisibilityEvent != null)
			{
				ChangeVisibilityEvent(this, e);
			}
		}
		public event EventHandler<VisibilityEventArgs> ChangeVisibility
		{
			add { ChangeVisibilityEvent += value; }
			remove { ChangeVisibilityEvent -= value; }
		}

		public class ResizeEventArgs : EventArgs
		{
			public double Width = 0;
			public double Height = 0;
			public ResizeEventArgs() { }
		}
		private event EventHandler<ResizeEventArgs> ResizeEvent = null;
		private void OnResize(ResizeEventArgs e)
		{
			if (ResizeEvent != null)
			{
				ResizeEvent(this, e);
			}
		}
		public event EventHandler<ResizeEventArgs> Resize
		{
			add { ResizeEvent += value; }
			remove { ResizeEvent -= value; }
		}

		public class ExpandEventArgs : EventArgs
		{
			[DataContract]
			public class Dimensions
			{
				[DataMember]
				public double x = 0;
				[DataMember]
				public double y = 0;
				[DataMember]
				public double width = 0;
				[DataMember]
				public double height = 0;

				public Dimensions()
				{}
			}

			[DataContract]
			public class Properties
			{
				[DataMember]
				public bool useBackground = false;
				[DataMember]
				public string backgroundColor = null;
				[DataMember]
				public double backgroundOpacity = 1;
				[DataMember]
				public bool lockOrientation = false;

				public Properties()
				{}
			}

			public Dimensions dimensions = null;
			public Properties properties = null;
			private string url = null;
			public string Url
			{
				get { return url; }
				set
				{
					if (value != "null")
					{
						url = value;
					}
				}
			}

			public ExpandEventArgs()
			{
			}

			public bool InitDimensions(string dimensionsJsonStr)
			{
				try
				{
					using (var memoryStream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(dimensionsJsonStr)))
					{
						var serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(Dimensions));
						dimensions = serializer.ReadObject(memoryStream) as Dimensions;
					}
				}
				catch (System.Exception)
				{}

				return (dimensions != null);
			}

			public bool InitProperties(string propertiesJsonStr)
			{
				try
				{
					using (var memoryStream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(propertiesJsonStr)))
					{
						var serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(Properties));
						properties = serializer.ReadObject(memoryStream) as Properties;
					}
				}
				catch (System.Exception)
				{}

				return (properties != null);
			}
		}
		private event EventHandler<ExpandEventArgs> ExpandEvent = null;
		private void OnExpand(ExpandEventArgs e)
		{
			if (ExpandEvent != null)
			{
				ExpandEvent(this, e);
				RaiseSizeChangeEvent(e.dimensions.width, e.dimensions.height);
			}
		}
		public event EventHandler<ExpandEventArgs> Expand
		{
			add { ExpandEvent += value; }
			remove { ExpandEvent -= value; }
		}

		private event EventHandler CloseEvent = null;
		private void OnClose()
		{
			if (CloseEvent != null)
			{
				CloseEvent(this, EventArgs.Empty);
			}
		}
		public event EventHandler Close
		{
			add { CloseEvent += value; }
			remove { CloseEvent -= value; }
		}

		public class OpenUrlEventArgs : EventArgs
		{
			public string Url = null;
			public OpenUrlEventArgs()
			{}
		}
		private event EventHandler<OpenUrlEventArgs> OpenUrlEvent = null;
		private void OnOpenUrl(OpenUrlEventArgs e)
		{
			if (OpenUrlEvent != null)
			{
				OpenUrlEvent(this, e);
			}
		}
		public event EventHandler<OpenUrlEventArgs> OpenUrl
		{
			add { OpenUrlEvent += value; }
			remove { OpenUrlEvent -= value; }
		}


		public class OpenMapEventArgs : EventArgs
		{
			public string POI = null;
			public bool FullScreen = true;
			public OpenMapEventArgs(string poi, string fullScreen)
			{
				POI = poi;
				Boolean.TryParse(fullScreen, out FullScreen);
			}
		}
		private event EventHandler<OpenMapEventArgs> OpenMapEvent = null;
		private void OnOpenMap(OpenMapEventArgs e)
		{
			if (OpenMapEvent != null)
			{
				OpenMapEvent(this, e);
			}
		}
		public event EventHandler<OpenMapEventArgs> OpenMap
		{
			add { OpenMapEvent += value; }
			remove { OpenMapEvent -= value; }
		}

		public class PlayMediaEventArgs : EventArgs
		{
			public string Url = null;
			public string Properties = null;
			public PlayMediaEventArgs(string url, string properties)
			{
				this.Url = url;
				this.Properties = properties;
			}
		}
		private event EventHandler<PlayMediaEventArgs> PlayMediaEvent = null;
		private void OnPlayMedia(PlayMediaEventArgs e)
		{
			if (PlayMediaEvent != null)
			{

				System.Diagnostics.Debug.WriteLine("PlayMediaEvent");

				PlayMediaEvent(this, e);
			}
		}
		public event EventHandler<PlayMediaEventArgs> PlayMedia
		{
			add { PlayMediaEvent += value; }
			remove { PlayMediaEvent -= value; }
		}
		#endregion

		#region "Private"
		~OrmmaController()
		{
			accelerometer = null;
			viewer = null;
		}

		private void RunScript(string script)
		{
			Deployment.Current.Dispatcher.BeginInvoke(() =>
			{
				try
				{
					viewer.ExecuteScript(script);
				}
				catch (System.Exception)
				{}
			});

			Deployment.Current.Dispatcher.BeginInvoke(() =>
			{
				try
				{
					expandedViewer.ExecuteScript(script);
				}
				catch (System.Exception)
				{ }
			});
		}

		private void Viewer_LoadCompleted(object sender, EventArgs e)
		{
			accelerometer.StartListen();

			StringBuilder script = new StringBuilder();

			if (!maxSize.IsEmpty)
			{
				script.AppendLine("maxSize = { width:" + maxSize.Width.ToString("F0") + ", height:" + maxSize.Height.ToString("F0") + "};");
			}

			script.AppendLine("size = { width:" + viewer.Width.ToString("F0") + ", height:" + viewer.Height.ToString("F0") + "};");
			script.AppendLine("defaultPosition = {x: " + defaultPosition.X.ToString("F0") + ", y: " + defaultPosition.Y.ToString("F0") + ", width: " + defaultSize.Width.ToString("F0") + ", height: " + defaultSize.Height.ToString("F0") + "};");
			script.AppendLine("cacheRemaining = " + assetManager.GetCacheRemaining().ToString() + ";");
			
			RunScript(script.ToString());
		}

		private void Viewer_ScriptNotify(string notify)
		{
			string[] notifyParts = notify.Split('|');

			if (notifyParts.Length > 0)
			{
				if (notifyParts[0] == "hide" || notifyParts[0] == "show")
				{
					VisibilityEventArgs e = new VisibilityEventArgs();

					if (notifyParts[0] == "hide")
					{
						e.Visible = false;
					}
					else if (notifyParts[0] == "show")
					{
						e.Visible = true;
					}

					OnChangeVisibility(e);
				}
				else if (notifyParts[0] == "resize")
				{
					double width = Double.NaN;
					double height = Double.NaN;

					ResizeEventArgs e = new ResizeEventArgs();
					try
					{
						width = Double.Parse(notifyParts[1]);
						height = Double.Parse(notifyParts[2]);

						if (!Double.IsNaN(width) && !Double.IsNaN(height))
						{
							e.Width = width;
							e.Height = height;
						}
					}
					catch (System.Exception)
					{
						e = null;
					}

					OnResize(e);
				}
				else if (notifyParts[0] == "expand" && notifyParts.Length == 4)
				{
					ExpandEventArgs e = new ExpandEventArgs();
					if (e.InitDimensions(notifyParts[1]) && e.InitProperties(notifyParts[2]))
					{
						e.Url = notifyParts[3];
						OnExpand(e);
					}
				}
				else if (notifyParts[0] == "close")
				{
					OnClose();
				}
				else if (notifyParts[0] == "open")
				{
					try
					{
						OpenUrlEventArgs e = new OpenUrlEventArgs();
						e.Url = notifyParts[1];
						OnOpenUrl(e);
					}
					catch (System.Exception)
					{}
				}
				else if (notifyParts[0] == "makeCall")
				{
					try
					{
						NativeAppManager.MakeCall(notifyParts[1]);
					}
					catch (System.Exception)
					{}
				}
				else if (notifyParts[0] == "sendMail")
				{
					try
					{
						NativeAppManager.SendMail(notifyParts[1], notifyParts[2], notifyParts[3]);
					}
					catch (System.Exception)
					{ }
				}
				else if (notifyParts[0] == "sendSMS")
				{
					try
					{
						NativeAppManager.SendSms(notifyParts[1], notifyParts[2]);
					}
					catch (System.Exception)
					{ }
				}
				else if (notifyParts[0] == "playAudio" || notifyParts[0] == "playVideo")
				{
					try
					{
						PlayMediaEventArgs e = new PlayMediaEventArgs(notifyParts[1], notifyParts[2]);
						OnPlayMedia(e);
					}
					catch (System.Exception)
					{}
				}
				else if (notifyParts[0] == "openMap")
				{
					try
					{
						OpenMapEventArgs e = new OpenMapEventArgs(notifyParts[1], notifyParts[2]);
						OnOpenMap(e);
					}
					catch (System.Exception)
					{}
				}
				else if (notifyParts[0] == "addAsset")
				{
					try
					{
						assetManager.AddAsset(notifyParts[1], notifyParts[2]);
					}
					catch (System.Exception)
					{ }
				}
				else if (notifyParts[0] == "removeAsset")
				{
					try
					{
						assetManager.RemoveAsset(notifyParts[1]);
					}
					catch (System.Exception)
					{ }
				}
				else if (notifyParts[0] == "removeAllAssets")
				{
					try
					{
						assetManager.RemoveAllAssets();
					}
					catch (System.Exception)
					{ }
				}
				else if (notifyParts[0] == "request")
				{
					try
					{
						assetManager.Request(notifyParts[1], notifyParts[2]);
					}
					catch (System.Exception)
					{ }
				}
				else if (notifyParts[0] == "storePicture")
				{
					try
					{
						assetManager.StorePicture(notifyParts[1]);
					}
					catch (System.Exception)
					{ }
				}
			}
		}

		private void ExpandedViewer_ScriptNotify(string notify)
		{
			Viewer_ScriptNotify(notify);
		}

		private void RaiseScreenChangeEvent()
		{
			double width = Double.NaN;
			double height = Double.NaN;

			switch (screenOrientation)
			{
				case PageOrientation.Portrait:
				case PageOrientation.PortraitUp:
				case PageOrientation.PortraitDown:
					width = Application.Current.RootVisual.RenderSize.Width;
					height = Application.Current.RootVisual.RenderSize.Height;
					break;
				case PageOrientation.Landscape:
				case PageOrientation.LandscapeLeft:
				case PageOrientation.LandscapeRight:
					width = Application.Current.RootVisual.RenderSize.Height;
					height = Application.Current.RootVisual.RenderSize.Width;
					break;
				default:
					break;
			}

			if (!Double.IsNaN(width) && !Double.IsNaN(height))
			{
				RaiseEvent("screenChange", "{width: " + width.ToString("F0") + ", height:" + height.ToString("F0") + "}");
			}
		}

		private void RaiseSizeChangeEvent(double width, double height)
		{
			RaiseEvent("sizeChange", "{width: " + width.ToString("F0") + ", height:" + height.ToString("F0") + "}");
		}

		private void LocationChanged(double latitude, double longitude, double accuracy)
		{
			string location = "{lat:" + latitude.ToString("F1") + ", lon:" + longitude.ToString("F1") + ", acc:" + accuracy.ToString("F1") + "}";
			RunScript("geoLocation = " + location + ";");
			RaiseEvent("locationChange", location);
		}

		private void CourseChanged(int course)
		{
			string heading = course.ToString();
			RunScript("heading = " + heading + ";");
			RaiseEvent("headingChange", heading);
		}

		private void NetworkChanged(Microsoft.Phone.Net.NetworkInformation.NetworkInterfaceType networkType)
		{
			string netWork = "offline";

			switch (networkType)
			{
				case NetworkInterfaceType.Wireless80211:
					netWork = "wifi";
					break;
				case NetworkInterfaceType.MobileBroadbandGsm:
				case NetworkInterfaceType.MobileBroadbandCdma:
				netWork = "cell";
					break;
				case NetworkInterfaceType.Unknown:
				default:
					netWork = "unknown";
					break;
			}

			RunScript("network = \"" + netWork + "\";");
			RaiseEvent("networkChange", "{online: " + (netWork == "wifi" || netWork == "cell").ToString().ToLower() + ", connection: \"" + netWork + "\"}");
		}

		private void TiltChange(double x, double y, double z)
		{
			string tilt = "{x:" + x.ToString("F2") + ", y:" + y.ToString("F2") + ", z:" + z.ToString("F2") + "}";
			RunScript("tilt = " + tilt + ";");
			RaiseEvent("tiltChange", tilt);
		}

		private void Shake(object sender, EventArgs e)
		{
			RaiseEvent("shake", String.Empty);
		}

		private void AssetReady(string alias)
		{
			RaiseEvent("assetReady", "\"" + alias.Replace("\"", "\\\"") + "\"");
			SetCacheRemaining();
		}

		private void AssetRemoved(string alias)
		{
			RaiseEvent("assetRemoved", "\"" + alias.Replace("\"", "\\\"") + "\"");
			SetCacheRemaining();
		}

		private void Response(string uri, string response)
		{
			RaiseEvent("response", "{uri:\"" + uri + "\", response:\"" + response.Replace("\"", "\\\"").Replace("\'", "\\\'").Replace("\n", "\\n") + "\"}");
		}

		private void RaiseEvent(string eventName, string eventData)
		{
			string script = "Ormma.raiseEvent(\"" + eventName + "\"" + (String.IsNullOrEmpty(eventData) ? String.Empty : (", " + eventData)) + ");";

			RunScript(script);
		}

		private void SetCacheRemaining()
		{
			RunScript("cacheRemaining = " + assetManager.GetCacheRemaining().ToString() + ";");
		}
		#endregion
	}
}
