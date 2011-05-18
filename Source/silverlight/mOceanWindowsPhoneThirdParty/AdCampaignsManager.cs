/****
 * © 2010-2011 mOcean Mobile. A subsidiary of Mojiva, Inc. All Rights Reserved.
 * */

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Google.AdMob.Ads.WindowsPhone7;
using Google.AdMob.Ads.WindowsPhone7.WPF;
using mmiWP7SDK;

namespace mOceanWindowsPhone
{
	internal class AdCampaignsManager
	{
		#region "Constants"
		static string TEST_ADMOB_ID = "a14db2e69514950";

		private const int TIME_OUT = 30000;
		private const string CONTENT_BEGIN = "<!-- client_side_external_campaign";
		private const string CONTENT_END = "-->";
		private const string CONTENT_ROOT_NODE_NAME = "external_campaign";
		private const string CONTENT_CAMPAIGN_ID_NODE_NAME = "campaign_id";
		private const string CONTENT_TYPE_NODE_NAME = "type";
		private const string CONTENT_PARAMS_NODE_NAME = "external_params";
		private const string CONTENT_TRACK_URL_NODE_NAME = "track_url";

		private const string MILLENNIAL_TYPE_NAME = "millennial";
		private const string ADMOB_TYPE_NAME = "admob";
		#endregion

		#region "Variables"
		private static Dictionary<int, ThirdPartyAdView> thirdPartyControls = new Dictionary<int, ThirdPartyAdView>();
		#endregion

		#region "Public methods"
		public static AdCampaignsManager Instance
		{
			get
			{
				lock (padlock)
				{
					if (instance == null)
					{
						instance = new AdCampaignsManager();
					}
					return instance;
				}
			}
		}

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

		public ThirdPartyAdView GetInitThirdPartyAdView(int viewId, string adContent)
		{
			adContent = adContent.Trim();
			if (!adContent.StartsWith(CONTENT_BEGIN) || !adContent.EndsWith(CONTENT_END))
			{
				return null;
			}

			ThirdPartyAdView newThirdPartyAdView = GetInitThirdPartyAdView(adContent);

			try
			{
				if (thirdPartyControls.ContainsKey(viewId))
				{
					thirdPartyControls.Remove(viewId);
				}

				thirdPartyControls.Add(viewId, newThirdPartyAdView);

				return newThirdPartyAdView;
			}
			catch (System.Exception)
			{}

			return null;
		}

		public bool TryDeleteThirdPartyAdView(int viewId)
		{
			try
			{
				return thirdPartyControls.Remove(viewId);
			}
			catch (System.Exception)
			{}

			return false;
		}
		#endregion

		#region "Private methods"
		private static readonly object padlock = new object();
		private static AdCampaignsManager instance = null;
		private static volatile int references = 0;
		static AdCampaignsManager()
		{ }
		AdCampaignsManager()
		{ }

		private ThirdPartyAdView GetInitThirdPartyAdView(string adContent)
		{
			adContent = adContent.Replace(CONTENT_BEGIN, "").Replace(CONTENT_END, "").Trim();

			try
			{
				XElement rootNode = XElement.Parse(adContent);

				if (rootNode.Name.LocalName == CONTENT_ROOT_NODE_NAME)
				{
					XElement campaignIdNode = null;
					XElement typeNode = null;
					XElement externalParamsNode = null;
					XElement trackUrlNode = null;

					foreach (var node in rootNode.Elements())
					{
						switch (node.Name.LocalName)
						{
							case CONTENT_CAMPAIGN_ID_NODE_NAME:
								campaignIdNode = node;
								break;
							case CONTENT_TYPE_NODE_NAME:
								typeNode = node;
								break;
							case CONTENT_PARAMS_NODE_NAME:
								externalParamsNode = node;
								break;
							case CONTENT_TRACK_URL_NODE_NAME:
								trackUrlNode = node;
								break;
							default:
								break;
						}
					}

					ThirdPartyAdView newAdView = null;

					string trackUrl = null;
					string campaignId = null;

					if (trackUrlNode != null)
					{
						trackUrl = trackUrlNode.Value;
					}

					if (campaignIdNode != null)
					{
						campaignId = campaignIdNode.Value;
					}

					if (typeNode != null && externalParamsNode != null)
					{
						switch (typeNode.Value.ToLower())
						{
							case MILLENNIAL_TYPE_NAME:
								newAdView = new MillennialAdView();
								break;
							case ADMOB_TYPE_NAME:
								newAdView = new AdMobView();
								break;
							default:
								break;
						}

						ManualResetEvent initedEvent = new ManualResetEvent(false);
						Deployment.Current.Dispatcher.BeginInvoke(() =>
						{
							newAdView.InitParams(externalParamsNode);

							initedEvent.Set();
						});

						initedEvent.WaitOne();
						newAdView.TrackUrl = trackUrl;
						newAdView.CampaignId = campaignId;
					}

					return newAdView;
				}
			}
			catch (System.Exception)
			{ }

			return null;
		}
		#endregion

		#region "Internal structures"
		public abstract class ThirdPartyAdView
		{
			#region "Properties"
			public UserControl View
			{
				get;
				protected set;
			}
			public string TrackUrl
			{
				get;
				set;
			}
			public string CampaignId
			{
				get;
				set;
			}
			#endregion

			#region "Events"
			public class ExCampaignEventArgs : EventArgs
			{
				public string CampaignId
				{
					get;
					set;
				}
				public ExCampaignEventArgs(string campaignId)
				{
					CampaignId = campaignId;
				}
			}
			private event EventHandler<ExCampaignEventArgs> ExCampaignEvent = null;
			protected void OnExCampaign()
			{
				if (ExCampaignEvent != null && !String.IsNullOrEmpty(CampaignId))
				{
					ExCampaignEvent(this, new ExCampaignEventArgs(CampaignId));
				}
			}
			public event EventHandler<ExCampaignEventArgs> ExCampaign
			{
				add { ExCampaignEvent += value; }
				remove { ExCampaignEvent -= value; }
			}
			#endregion

			private Timer waitTimer = null;

			protected ThirdPartyAdView() { }
			~ThirdPartyAdView()
			{
				if (waitTimer != null)
				{
					waitTimer.Dispose();
					waitTimer = null;
				}
				View = null;
			}
			protected void OnTrackUrl()
			{
				if (!String.IsNullOrEmpty(TrackUrl))
				{
					DataRequest.SendGetRequest(TrackUrl, String.Empty);
				}
			}
			public virtual void InitParams(XElement extParams) { }
			public virtual void Run()
			{
				waitTimer = new Timer(new TimerCallback(TimeOutCallBack), this, TIME_OUT, Timeout.Infinite);
			}

			protected void TimeOutCallBack(object state)
			{
				OnExCampaign();
			}
		}

		private class MillennialAdView : ThirdPartyAdView
		{
			public MillennialAdView()
			{
				ManualResetEvent created = new ManualResetEvent(false);
				Deployment.Current.Dispatcher.BeginInvoke(() =>
				{
					View = new MMAdView()
					{
						RefreshTimer = -1
					};

					created.Set();
				});
				created.WaitOne();

				((MMAdView)View).MMAdSuccess += new EventHandler<EventArgs>(MMAdSuccess);
				((MMAdView)View).MMAdFailure += new EventHandler<EventArgs>(MMAdFailure);
			}

			public override void InitParams(XElement extParams)
			{
				foreach (var paramNode in extParams.Elements())
				{
					if (paramNode.Name.LocalName == "param")
					{
						XAttribute attribute = paramNode.Attribute("name");
						if (attribute != null)
						{
							string attributeName = paramNode.Attribute("name").Value.ToLower();
							string paramName = null;
							string paramValue = paramNode.Value;

							switch (attributeName)
							{
								case "id":
									paramName = "Apid";
									break;
								case "long":
									paramName = "Longitude";
									break;
								case "lat":
									paramName = "Latitude";
									break;
								default:
									paramName = attributeName;
									break;
							}

							PropertyInfo property = null;
							try
							{
								property = typeof(MMAdView).GetProperty(paramName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
							}
							catch (System.Exception)
							{ }

							if (property != null)
							{
								try
								{
									if (property.PropertyType.BaseType == typeof(Enum))
									{
										property.SetValue(View, (Enum.Parse(property.PropertyType, paramValue, true)), null);
									}
									else if (property.PropertyType == typeof(int))
									{
										property.SetValue(View, Int32.Parse(paramValue), null);
									}
									else
									{
										property.SetValue(View, paramValue, null);
									}
								}
								catch (System.Exception)
								{ }
							}
						}
					}
				}
			}

			public override void Run()
			{
				base.Run();
				((MMAdView)View).UpdateLayout();
				((MMAdView)View).CallForAd();
			}

			private void MMAdSuccess(object sender, EventArgs e)
			{
				OnTrackUrl();
			}

			private void MMAdFailure(object sender, EventArgs e)
			{
				OnExCampaign();
			}
		}

		private class AdMobView : ThirdPartyAdView
		{
			public AdMobView()
			{
				ManualResetEvent created = new ManualResetEvent(false);
				Deployment.Current.Dispatcher.BeginInvoke(() =>
				{
					View = new BannerAd();
					((BannerAd)View).EndUpdates();

					created.Set();
				});
				created.WaitOne();

				((BannerAd)View).AdReceived += new RoutedEventHandler(AdReceived);
				((BannerAd)View).AdFailed += new ErrorEventHandler(AdFailed);
			}
			public override void InitParams(XElement extParams)
			{
				double latitude = Double.NaN;
				double longitude = Double.NaN;

				foreach (var paramNode in extParams.Elements())
				{
					if (paramNode.Name.LocalName == "param")
					{
						XAttribute attribute = paramNode.Attribute("name");
						if (attribute != null)
						{
							string attributeName = paramNode.Attribute("name").Value.ToLower();
							string paramValue = paramNode.Value;

							if (!String.IsNullOrEmpty(paramValue))
							{
								switch (attributeName)
								{
									case "publisherid":
										//paramValue = TEST_ADMOB_ID;

										((BannerAd)View).AdUnitID = paramValue;
										break;
									case "lat":
										Double.TryParse(paramValue, out latitude);
										break;
									case "long":
										Double.TryParse(paramValue, out longitude);
										break;
									case "backgroundColor":
										((BannerAd)View).SetProperty("color_bg", paramValue);
										break;
									case "primaryTextColor":
										((BannerAd)View).SetProperty("color_text", paramValue);
										break;
									case "secondaryTextColor":
										((BannerAd)View).SetProperty("color_link", paramValue);
										break;
									default:
										((BannerAd)View).SetProperty(attributeName, paramValue);
										break;
								}
							}
						}
					}
				}


				if (!Double.IsNaN(latitude) && !Double.IsNaN(longitude))
				{
					((BannerAd)View).GpsLocation = new Google.AdMob.Ads.WindowsPhone7.GpsLocation
					{
						Latitude = latitude,
						Longitude = longitude,
						Accuracy = 1
					};
				}
			}

			public override void Run()
			{
				base.Run();
				((BannerAd)View).BeginUpdates();
			}

			private void AdReceived(object sender, RoutedEventArgs e)
			{
				OnTrackUrl();
			}

			private void AdFailed(object sender, AdException exception)
			{
				OnExCampaign();
			}
		}
		#endregion
	}
}
