/****
 * © 2010-2011 mOcean Mobile. A subsidiary of Mojiva, Inc. All Rights Reserved.
 * */

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;
using mmiWP7SDK;

namespace MojivaPhone
{
	internal class CThirdPartyManager : CBaseThirdPartyManager
	{
		private MMAdView mmAdView = null;

		public CThirdPartyManager(MMAdView mmAdView)
		{
			this.mmAdView = mmAdView;
		}

		protected override void RunMillennialAdView(XElement externalParamsNode)
		{
			millennialAdView = new CSLMillennialAdView(mmAdView);
			millennialAdView.AdViewSuccess += new EventHandler(MillennialAdViewSuccess);
			millennialAdView.AdViewFailure += new EventHandler(MillennialAdViewFailure);
			millennialAdView.Run(externalParamsNode);
		}

		private class CSLMillennialAdView : CMillennialAdView
		{
			private MMAdView adView = null;

			public CSLMillennialAdView(MMAdView mmAdView)
			{
				adView = mmAdView;

				if (adView != null)
				{
					adView.MMAdSuccess += new EventHandler<EventArgs>(AdView_MMAdSuccess);
					adView.MMAdFailure += new EventHandler<EventArgs>(AdView_MMAdFailure);
					adView.RefreshTimer = 5;
				}
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
				if (parameters == null || parameters.Count == 0)
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

			protected override void RunAd()
			{
				if (adView != null)
				{
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
		}
	}
}
