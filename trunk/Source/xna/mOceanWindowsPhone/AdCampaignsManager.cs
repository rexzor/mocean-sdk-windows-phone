/****
 * © 2010-2011 mOcean Mobile. A subsidiary of Mojiva, Inc. All Rights Reserved.
 * */

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WP7XNASDK;

namespace mOceanWindowsPhone
{
	internal class AdCampaignsManager
	{
		#region "Constants"
		private const int TIME_OUT = 1000;
		private const string CONTENT_BEGIN = "<!-- client_side_external_campaign";
		private const string CONTENT_END = "-->";
		private const string CONTENT_ROOT_NODE_NAME = "external_campaign";
		private const string CONTENT_CAMPAIGN_ID_NODE_NAME = "campaign_id";
		private const string CONTENT_TYPE_NODE_NAME = "type";
		private const string CONTENT_PARAMS_NODE_NAME = "external_params";
		private const string CONTENT_TRACK_URL_NODE_NAME = "track_url";

		private const string MILLENNIAL_TYPE_NAME = "millennial";
		#endregion

		#region "Variables"
		private SpriteBatch spriteBatch = null;

		private MMAdView mmAdView = null;
		private string trackUrl = null;
		private string campaignId = null;
		#endregion

		#region "Public methods"
		public AdCampaignsManager(SpriteBatch spriteBatch)
		{
			this.spriteBatch = spriteBatch;
		}

		public void TryAddView(string adContent)
		{
			adContent = adContent.Trim();
			if (!adContent.StartsWith(CONTENT_BEGIN) || !adContent.EndsWith(CONTENT_END))
			{
				if (mmAdView != null)
				{
					mmAdView = null;
				}
				return;
			}

			adContent = adContent.Replace(CONTENT_BEGIN, "").Replace(CONTENT_END, "").Trim();

			try
			{
				XElement rootNode = XElement.Parse(adContent);

				if (rootNode.Name.LocalName == CONTENT_ROOT_NODE_NAME)
				{
					XElement typeNode = null;
					XElement externalParamsNode = null;

					foreach (var node in rootNode.Elements())
					{
						switch (node.Name.LocalName)
						{
							case CONTENT_CAMPAIGN_ID_NODE_NAME:
								campaignId = node.Value;
								break;
							case CONTENT_TYPE_NODE_NAME:
								typeNode = node;
								break;
							case CONTENT_PARAMS_NODE_NAME:
								externalParamsNode = node;
								break;
							case CONTENT_TRACK_URL_NODE_NAME:
								trackUrl = node.Value;
								break;
							default:
								break;
						}
					}

					if (typeNode != null && typeNode.Value.ToLower() == MILLENNIAL_TYPE_NAME)
					{
						InitMillenialAdView(externalParamsNode);
					}
				}
			}
			catch (System.Exception)
			{}
		}

		public void Update(GameTime gameTime)
		{
			if (mmAdView != null)
			{
				mmAdView.Update(gameTime);
			}
		}

		public void Draw(GameTime gameTime)
		{
			spriteBatch.Begin();
			if (mmAdView != null)
			{
				mmAdView.Draw(gameTime);
			}
			spriteBatch.End();
		}
		#endregion

		#region "Private methods"
		private void InitMillenialAdView(XElement externalParamsNode)
		{
			string adPlaceName = null;
			foreach (var paramNode in externalParamsNode.Elements())
			{
				if (paramNode.Name.LocalName == "param")
				{
					var attr = paramNode.Attribute("name");
					if (attr != null && attr.Value == "adType")
					{
						adPlaceName = (paramNode.Value).Replace("MM", String.Empty);
						break;
					}
				}
			}

			AdPlacement adPlacement = AdPlacement.Default;

			try
			{
				adPlacement = (AdPlacement)Enum.Parse(typeof(AdPlacement), adPlaceName, true);
			}
			catch (System.Exception /*ex*/)
			{ }

			mmAdView = new MMAdView(spriteBatch.GraphicsDevice, adPlacement);
			mmAdView.MMAdSuccess += new EventHandler(MmAdView_MMAdSuccess);
			mmAdView.MMAdFailure += new EventHandler(MmAdView_MMAdFailure);


			foreach (var paramNode in externalParamsNode.Elements())
			{
				if (paramNode.Name.LocalName == "param")
				{
					var attr = paramNode.Attribute("name");
					if (attr != null)
					{
						//parameters.Add(attr.Value.ToLower(), paramNode.Value);
						string paramName = String.Empty;
						string paramValue = paramNode.Value;

						switch (attr.Value.ToLower())
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
							case "adtype":
								paramName = String.Empty;
								break;
							default:
								paramName = attr.Value.ToLower();
								break;
						}

						PropertyInfo property = typeof(MMAdView).GetProperty(paramName, BindingFlags.Public |
																						BindingFlags.Instance |
																						BindingFlags.IgnoreCase);

						if (property != null)
						{
							try
							{
								if (property.PropertyType.BaseType == typeof(Enum))
								{
									property.SetValue(mmAdView, (Enum.Parse(property.PropertyType, paramValue, true)), null);
								}
								else if (property.PropertyType == typeof(int))
								{
									property.SetValue(mmAdView, Int32.Parse(paramValue), null);
								}
								else
								{
									property.SetValue(mmAdView, paramValue, null);
								}
							}
							catch (System.Exception /*ex*/)
							{ }
						}
					}
				}
			}

			mmAdView.RefreshTimer = 5;
		}

		private void MmAdView_MMAdSuccess(object sender, EventArgs e)
		{
			if (!String.IsNullOrEmpty(trackUrl))
			{
				DataRequest.SendGetRequest(trackUrl, String.Empty);
			}
		}

		private void MmAdView_MMAdFailure(object sender, EventArgs e)
		{
			if (!String.IsNullOrEmpty(campaignId))
			{
				if (AddExCampaignEvent != null)
				{
					AddExCampaignEvent(this, new ExCampaignEventArgs(campaignId));
				}
			}
		}
		#endregion

		#region "Events"
		private event EventHandler<ExCampaignEventArgs> AddExCampaignEvent;
		public event EventHandler<ExCampaignEventArgs> AddExCampaign
		{
			add { AddExCampaignEvent += value; }
			remove { AddExCampaignEvent -= value; }
		}
		#endregion
	}

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
}
