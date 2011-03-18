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

namespace MojivaPhone
{
	internal class CThirdPartyManager : CBaseThirdPartyManager
	{
		private GraphicsDevice graphicsDevice = null;
		private Vector2 position = Vector2.Zero;

		private CThirdPartyManager()
		{
		}

		private static CThirdPartyManager Instance
		{
			get
			{
				lock (padlock)
				{
					return (CThirdPartyManager)instance;
				}
			}
			set
			{
				lock (padlock)
				{
					instance = value;
				}
			}
		}

		public static new bool ContainExternalCampaign(string adContent)
		{
			return CBaseThirdPartyManager.ContainExternalCampaign(adContent);
		}

		public static void Create(GraphicsDevice graphicsDevice, Vector2 position, string adContent)
		{
			if (Instance == null)
			{
				Instance = new CThirdPartyManager();
				Instance.graphicsDevice = graphicsDevice;
				Instance.position = position;
				Instance.Run(adContent);
			}
		}

		public static void Update(GameTime gameTime)
		{
			if (Instance != null)
			{
				if (Instance.millennialAdView != null)
				{
					((CXNAMillennialAdView)Instance.millennialAdView).Update(gameTime);
				}

			}
		}

		public static void Draw(GameTime gameTime)
		{
			if (Instance != null)
			{
				if (Instance.millennialAdView != null)
				{
					((CXNAMillennialAdView)Instance.millennialAdView).Draw(gameTime);
				}
			}
		}

		protected override void RunMillennialAdView(XElement externalParamsNode)
		{
			// get AdPlacement parameter. Need to MMAdView constructor
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
			{}

			millennialAdView = new CXNAMillennialAdView(graphicsDevice, position, adPlacement);
			millennialAdView.AdViewSuccess += new EventHandler(MillennialAdViewSuccess);
			millennialAdView.AdViewFailure += new EventHandler(MillennialAdViewFailure);
			millennialAdView.Run(externalParamsNode);
		}

		private class CXNAMillennialAdView : CMillennialAdView
		{
			private MMAdView adView = null;

			public CXNAMillennialAdView(GraphicsDevice graphicsDevice, Vector2 position, AdPlacement adPlacement)
			{
				adView = new MMAdView(graphicsDevice, position, adPlacement);
				adView.MMAdSuccess += new EventHandler(AdView_MMAdSuccess);
				adView.MMAdFailure += new EventHandler(AdView_MMAdFailure);
				adView.RefreshTimer = 5;
			}

			private void AdView_MMAdSuccess(object sender, EventArgs e)
			{
				OnAdViewSuccess();
			}

			private void AdView_MMAdFailure(object sender, EventArgs e)
			{
				OnAdViewFailure();
			}

			protected override bool IsInitParams(Dictionary<string, string> parameters)
			{
				if (adView == null || parameters == null || parameters.Count == 0)
				{
					return false;
				}

				foreach (string param in parameters.Keys)
				{
					string paramName = String.Empty;
					string paramValue = parameters[param];

					switch (param)
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
							paramName = param;
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
								property.SetValue(adView, (Enum.Parse(property.PropertyType, paramValue, true)), null);
							}
							else if (property.PropertyType == typeof(int))
							{
								property.SetValue(adView, Int32.Parse(paramValue), null);
							}
							else
							{
								property.SetValue(adView, paramValue, null);
							}
						}
						catch (System.Exception /*ex*/)
						{ }
					}
				}

				return true;
			}

			public void Draw(GameTime gameTime)
			{
				if (adView != null)
				{
					adView.Draw(gameTime);
				}
			}

			public void Update(GameTime gameTime)
			{
				if (adView != null)
				{
					adView.Update(gameTime);
				}
			}
		}
	}
}
