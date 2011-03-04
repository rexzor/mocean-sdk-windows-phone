/****
 * © 2010-2011 mOcean Mobile. A subsidiary of Mojiva, Inc. All Rights Reserved.
 * */

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Xml.Linq;
using Microsoft.Phone.Controls;
using mmiWP7SDK;

namespace MojivaPhone
{
	partial class ThirdPartyContainer : UserControl
	{
		private const string CONTENT_BEGIN = "<!-- client_side_external_campaign";
		private const string CONTENT_END = "-->";
		private const string CONTENT_ROOT_NODE_NAME = "external_campaign";
		private const string CONTENT_CAMPAIGN_ID_NODE_NAME = "campaign_id";
		private const string CONTENT_TYPE_NODE_NAME = "type";
		private const string CONTENT_PARAMS_NODE_NAME = "external_params";
		private const string CONTENT_TRACK_URL_NODE_NAME = "track_url";

		private const string MILLENNIAL_TYPE_NAME = "Millennial";
		private CMillennialManager millennialManager = null;

		#region "Events"
		
		private event EventHandler<EventArgs> AdSuccessEvent;
		private event EventHandler<EventArgs> AdFailureEvent;
		private event EventHandler<StringEventArgs> ExCampaignAddEvent;

		/// <summary>
		/// Event that is raised when external campaign ad shown successfully
		/// </summary>
		public event EventHandler<EventArgs> AdSuccess
		{
			add { AdSuccessEvent += value; }
			remove { AdSuccessEvent -= value; }
		}

		/// <summary>
		/// Event that is raised when external campaign ad shown unsuccessfully
		/// </summary>
		public event EventHandler<EventArgs> AdFailure
		{
			add { AdFailureEvent += value; }
			remove { AdFailureEvent -= value; }
		}

		/// <summary>
		/// Event that is raised when external campaign ad shown unsuccessfully and it's id should be excluded from the result.
		/// </summary>
		public event EventHandler<StringEventArgs> ExCampaignAdd
		{
			add { ExCampaignAddEvent += value; }
			remove { ExCampaignAddEvent -= value; }
		}
		#endregion

		public ThirdPartyContainer()
		{
			InitializeComponent();
			millennialManager = new CMillennialManager(mMAdView);

			millennialManager.AdViewSuccess += new EventHandler(OnAdViewSuccess);
			millennialManager.AdViewFailure += new EventHandler(OnAdViewFailure);
			millennialManager.ExCampaignAdd += new EventHandler<StringEventArgs>(OnExCampaignAdd);
		}

		private void OnAdViewSuccess(object sender, EventArgs e)
		{
			if (AdSuccessEvent != null)
			{
				AdSuccessEvent(this, e);
			}
		}

		private void OnAdViewFailure(object sender, EventArgs e)
		{
			if (AdFailureEvent != null)
			{
				AdFailureEvent(this, e);
			}
		}

		private void OnExCampaignAdd(object sender, StringEventArgs e)
		{
			if (ExCampaignAddEvent != null)
			{
				ExCampaignAddEvent(this, e);
			}
		}

		private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
		{
			Size renderSize = Application.Current.RootVisual.RenderSize;
 			this.Width = renderSize.Width;
 			this.Height = renderSize.Height;
			LayoutRoot.Width = renderSize.Width;
			LayoutRoot.Height = renderSize.Height;
		}

		public void SearchPage(string pageContent)
		{
			pageContent = pageContent.Replace(CONTENT_BEGIN, "").Replace(CONTENT_END, "").Trim();

			try
			{
				XElement rootNode = XElement.Parse(pageContent);

				if (rootNode.Name.LocalName == CONTENT_ROOT_NODE_NAME)
				{
					XElement campaignId = null;
					XElement type = null;
					XElement externalParams = null;
					XElement trackUrl = null;

					foreach (var node in rootNode.Elements())
					{
						System.Diagnostics.Debug.WriteLine(node.Name);

						switch (node.Name.LocalName)
						{
							case CONTENT_CAMPAIGN_ID_NODE_NAME:
								campaignId = node;
								break;
							case CONTENT_TYPE_NODE_NAME:
								type = node;
								break;
							case CONTENT_PARAMS_NODE_NAME:
								externalParams = node;
								break;
							case CONTENT_TRACK_URL_NODE_NAME:
								trackUrl = node;
								break;
							default:
								break;
						}
					}

					if (type != null)
					{
						switch (type.Value)
						{
							case MILLENNIAL_TYPE_NAME:
								if (externalParams != null)
								{
									System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
										{
											if (trackUrl != null)
											{
												millennialManager.TrackUrl = trackUrl.Value;
											}

											if (campaignId != null)
											{
												millennialManager.CampaignId = campaignId.Value;
											}
											millennialManager.Run(externalParams);
										}
									);
								}
								break;
							default:
								break;
						}
					}
				}
			}
			catch (System.Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
			}
		}

		private UIElement InitFromXamlString(string xamlStr)
		{
			var wrapStr = "<Canvas xmlns=\"http://schemas.microsoft.com/client/2007\"";
			wrapStr += " xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"";
			wrapStr += xamlStr;
			wrapStr += "</Canvas>";

			try
			{
				var loadedXAMLWithWrapper = System.Windows.Markup.XamlReader.Load(wrapStr) as Canvas;
				var loadedXAML = loadedXAMLWithWrapper.Children[0];
				loadedXAMLWithWrapper.Children.Remove(loadedXAML);
				return (UIElement)loadedXAML;
			}
			catch (System.Exception /*ex*/)
			{ }

			return null;
		}

		private abstract class CAdManager
		{
			protected string trackUrl = null;
			protected string campaignId = null;

			public string TrackUrl
			{
				set { trackUrl = value; }
			}
			public string CampaignId
			{
				set { campaignId = value; }
			}

			public event EventHandler AdViewSuccess;
			public event EventHandler AdViewFailure;
			public event EventHandler<StringEventArgs> ExCampaignAdd;

			protected CAdManager()
			{
			}

			protected virtual void OnAdViewSuccess(object sender, EventArgs e)
			{
				if (AdViewSuccess != null)
				{
					AdViewSuccess(this, e);
				}
			}

			protected virtual void OnAdViewFailure(object sender, EventArgs e)
			{
				if (AdViewFailure != null)
				{
					AdViewFailure(this, e);
				}
			}

			protected void OnExCampaignAdd(string campaign_id)
			{
				if (ExCampaignAdd != null)
				{
					ExCampaignAdd(this, new StringEventArgs(campaign_id));
				}
			}
		}

		private class CMillennialManager : CAdManager
		{
			private MMAdView adView = null;

			public CMillennialManager(MMAdView mmAdView)
			{
				adView = mmAdView;

				if (adView != null)
				{
					adView.MMAdSuccess += new EventHandler<EventArgs>(OnAdViewSuccess);
					adView.MMAdFailure += new EventHandler<EventArgs>(OnAdViewFailure);
					adView.MMOverlayOpened += new EventHandler<EventArgs>(AdViewOpened);
				}
			}

			public void Run(XElement extParams)
			{
				System.Diagnostics.Debug.WriteLine("RunMillenialAdView");

				if (IsInitParams(extParams.ToString()))
				{
					adView.AdType = MMAdView.MMAdType.MMFullScreenAdLaunch;

					if (adView.AdType == MMAdView.MMAdType.MMFullScreenAdLaunch ||
						adView.AdType == MMAdView.MMAdType.MMFullScreenAdTransition)
					{
						adView.Width = 0;
						adView.Height = 0;

						adView.AdWidth = "0";
						adView.AdHeight = "0";
					}
					adView.CallForAd();
				}
			}

			private bool IsInitParams(string extParamsStr)
			{
				try
				{
					XElement extParams = XElement.Parse(extParamsStr);
					foreach (var paramNode in extParams.Elements())
					{
						if (paramNode.Name.LocalName == "param")
						{
							var attr = paramNode.Attribute("name");
							if (attr != null)
							{
								string attrName = attr.Value.ToLower();
								string paramName = null;

								switch (attrName)
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
										paramName = attrName;
										break;
								}

								PropertyInfo property = typeof(MMAdView).GetProperty(paramName, BindingFlags.Public |
																								BindingFlags.Instance |
																								BindingFlags.IgnoreCase);

								if (property != null)
								{
									System.Diagnostics.Debug.WriteLine("finded property " + property.Name);

									try
									{
										if (property.PropertyType.BaseType == typeof(Enum))
										{
											property.SetValue(adView, (Enum.Parse(property.PropertyType, paramNode.Value, true)), null);
										}
										else if (property.PropertyType == typeof(int))
										{
											property.SetValue(adView, Int32.Parse(paramNode.Value), null);
										}
										else
										{
											property.SetValue(adView, paramNode.Value, null);
										}
									}
									catch (System.Exception /*ex*/)
									{ }
								}
							}
						}
					}
				}
				catch (System.Exception /*ex*/)
				{ }

				return true;
			}

			protected override void OnAdViewSuccess(object sender, EventArgs e)
			{
				base.OnAdViewSuccess(sender, e);

				if (!String.IsNullOrEmpty(trackUrl))
				{
 					//string result = DataRequest.ReadStringData(trackUrl);
 					//System.Diagnostics.Debug.WriteLine("3d party result:" + result);
				}
			}

			protected override void OnAdViewFailure(object sender, EventArgs e)
			{
				System.Diagnostics.Debug.WriteLine("mMAdView_MMAdFailure");

				base.OnAdViewFailure(sender, e);

				OnExCampaignAdd(campaignId);
			}

			private void AdViewOpened(object sender, EventArgs e)
			{ System.Diagnostics.Debug.WriteLine("mMAdView_MMOverlayOpened"); }

			private void CallBackResult(IAsyncResult asynchronousResult)
			{
			}
		}
	}
}
