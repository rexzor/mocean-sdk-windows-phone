/****
 * © 2010-2011 mOcean Mobile. A subsidiary of Mojiva, Inc. All Rights Reserved.
 * */

using System;
using System.Xml.Linq;
using System.Collections.Generic;

namespace MojivaPhone
{
	internal class CThirdPartyManager
	{
		protected static CThirdPartyManager instance = null;
		protected static readonly object padlock = new object();

		#region "Constants"
		private const string CONTENT_BEGIN = "<!-- client_side_external_campaign";
		private const string CONTENT_END = "-->";
		private const string CONTENT_ROOT_NODE_NAME = "external_campaign";
		private const string CONTENT_CAMPAIGN_ID_NODE_NAME = "campaign_id";
		private const string CONTENT_TYPE_NODE_NAME = "type";
		private const string CONTENT_PARAMS_NODE_NAME = "external_params";
		private const string CONTENT_TRACK_URL_NODE_NAME = "track_url";

		private const string MILLENNIAL_TYPE_NAME = "Millennial";
		#endregion

		#region "Variables"
		protected string trackUrl = null;
		protected string campaignId = null;
		protected CMillennialAdView millennialAdView = null;
		#endregion

		protected CThirdPartyManager()
		{
			millennialAdView = new CMillennialAdView();
			millennialAdView.AdViewSuccess += new EventHandler(MillennialAdViewSuccess);
			millennialAdView.AdViewFailure += new EventHandler(MillennialAdViewFailure);
		}

		public static CThirdPartyManager Instance
		{
			get
			{
				lock (padlock)
				{
					if (instance == null)
					{
						instance = new CThirdPartyManager();
					}
					return instance;
				}
			}
		}

		public static void Release()
		{
			instance = null;
		}

		public static bool IsExternalCampaignContent(string pageContent)
		{
			pageContent = pageContent.Trim();

			if (pageContent.StartsWith(CONTENT_BEGIN) && pageContent.EndsWith(CONTENT_END))
			{
				return true;
			}

			return false;
		}

		public void Run(string pageContent)
		{
			pageContent = pageContent.Replace(CONTENT_BEGIN, "").Replace(CONTENT_END, "").Trim();

			try
			{
				XElement rootNode = XElement.Parse(pageContent);

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

					if (trackUrlNode != null)
					{
						trackUrl = trackUrlNode.Value;
					}

					if (campaignIdNode != null)
					{
						campaignId = campaignIdNode.Value;
					}

					if (typeNode != null)
					{
						switch (typeNode.Value)
						{
							case MILLENNIAL_TYPE_NAME:
								if (externalParamsNode != null)
								{
									millennialAdView.Run(externalParamsNode);
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

		protected void MillennialAdViewSuccess(object sender, EventArgs e)
		{
			if (!String.IsNullOrEmpty(trackUrl))
			{
				DataRequest.SendGetRequest(trackUrl, String.Empty);
			}
		}

		protected void MillennialAdViewFailure(object sender, EventArgs e)
		{
			if (!String.IsNullOrEmpty(campaignId))
			{
				AdserverRequest.Instance.AddExCampaign(campaignId);
			}
		}
	}

	internal abstract class CBaseThirdPartyAdView
	{
		private event EventHandler AdViewSuccessEvent;
		private event EventHandler AdViewFailureEvent;
		public event EventHandler AdViewSuccess
		{
			add { AdViewSuccessEvent += value; }
			remove { AdViewSuccessEvent -= value; }
		}
		public event EventHandler AdViewFailure
		{
			add { AdViewFailureEvent += value; }
			remove { AdViewFailureEvent -= value; }
		}

		protected CBaseThirdPartyAdView()
		{
		}

		protected Dictionary<string, string> GetParamsFromNode(XElement extParams)
		{
			var parameters = new Dictionary<string, string>();

			try
			{
				foreach (var paramNode in extParams.Elements())
				{
					if (paramNode.Name.LocalName == "param")
					{
						var attr = paramNode.Attribute("name");
						if (attr != null)
						{
							parameters.Add(attr.Value.ToLower(), paramNode.Value);
						}
					}
				}
			}
			catch (System.Exception /*ex*/)
			{
				return null;
			}

			return parameters;
		}

		protected void OnAdViewSuccess()
		{
			if (AdViewSuccessEvent != null)
			{
				AdViewSuccessEvent(this, EventArgs.Empty);
			}
		}

		protected void OnAdViewFailure()
		{
			if (AdViewFailureEvent != null)
			{
				AdViewFailureEvent(this, EventArgs.Empty);
			}
		}
	}

	internal class CMillennialAdView : CBaseThirdPartyAdView
	{
		public CMillennialAdView()
		{}

		public void Run(XElement extParams)
		{
			if (IsInitParams(GetParamsFromNode(extParams)))
			{
				RunAd();
			}
		}

		protected virtual bool IsInitParams(Dictionary<string, string> parameters)
		{
			return false;
		}

		protected virtual void RunAd()
		{}
	}
}
