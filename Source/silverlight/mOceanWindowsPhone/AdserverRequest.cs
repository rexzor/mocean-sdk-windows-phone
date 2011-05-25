/****
 * © 2010-2011 mOcean Mobile. A subsidiary of Mojiva, Inc. All Rights Reserved.
 * */

using System;
using System.Collections.Generic;

namespace mOceanWindowsPhone
{
	internal class AdserverRequest
	{
		#region "Variables"
		private Dictionary<string, string> parameters = new Dictionary<string, string>();
		private string parameter_site = "site";
		private string parameter_zone = "zone";
		private string parameter_userAgent = "ua";
		private string parameter_test = "test";
		private string parameter_premium = "premium";
		private string parameter_keywords = "keywords";
		private string parameter_min_size_x = "min_size_x";
		private string parameter_min_size_y = "min_size_y";
		private string parameter_size_x = "size_x";
		private string parameter_size_y = "size_y";
		private string parameter_background_color = "paramBG";
		private string parameter_text_color = "paramLINK";
		private string parameter_latitude = "lat";
		private string parameter_longitude = "long";
		private string parameter_country = "country";
		private string parameter_region = "region";
		private string parameter_city = "city";
		private string parameter_area = "area";
		private string parameter_metro = "metro";
		private string parameter_zip = "zip";
		private string parameter_carrier = "carrier";
		private string parameter_connection_speed = "connection_speed";

		private string parameter_key = "key";
		private string parameter_adstype = "adstype";
		private string parameter_count = "count";
		private string parameter_version = "version";
		private string parameter_size_required = "size_required";
		private string parameter_device_id = "udid";

		private string parameter_excampaigns = "excampaigns";

		private string adserverURL = String.Empty;
		#endregion

		#region "Public methods"
		public AdserverRequest()
		{
			SetupAutodetectParameters();

			AddParameter(parameter_key, "1");
			AddParameter(parameter_count, "1");
		}

		public void SetAdserverURL(string adserverURL)
		{
			this.adserverURL = adserverURL;
		}

		public void SetSite(int site)
		{
			AddParameter(parameter_site, site.ToString());
		}

		public void SetZone(int zone)
		{
			AddParameter(parameter_zone, zone.ToString());
		}

		public void SetUserAgent(string userAgent)
		{
			AddParameter(parameter_userAgent, userAgent);
		}

		public void SetTest(bool test)
		{
			if (test)
			{
				AddParameter(parameter_test, "1");
			}
			else
			{
				RemoveParameter(parameter_test);
			}
		}

		public void SetPremium(int premium)
		{
			AddParameter(parameter_premium, premium.ToString());
		}

		public void SetKeywords(string keywords)
		{
			AddParameter(parameter_keywords, keywords);
		}

		public void SetAdsType(int adsType)
		{
			AddParameter(parameter_adstype, adsType.ToString());
		}

		public void SetMinSizeX(int size)
		{
			AddParameter(parameter_min_size_x, size.ToString());
		}

		public void SetMinSizeY(int size)
		{
			AddParameter(parameter_min_size_y, size.ToString());
		}

		public void SetMaxSizeX(int size)
		{
			AddParameter(parameter_size_x, size.ToString());
		}

		public void SetMaxSizeY(int size)
		{
			AddParameter(parameter_size_y, size.ToString());
		}

		public void SetBackgroundColor(string backgroundColor)
		{
			AddParameter(parameter_background_color, backgroundColor);
		}

		public void SetTextColor(string textColor)
		{
			AddParameter(parameter_text_color, textColor);
		}

		public void SetCustomParameters(string customParametersString)
		{
			try
			{
				String[] paramsArray = customParametersString.Split(',');
				for (int f = 0; f < paramsArray.Length; f += 2)
				{
					AddParameter(paramsArray[f], paramsArray[f + 1]);
				}
			}
			catch (System.Exception)
			{ }
		}

		public void SetLatitude(string latitude)
		{
			AddParameter(parameter_latitude, latitude);
		}

		public void SetLongitude(string longitude)
		{
			AddParameter(parameter_longitude, longitude);
		}

		public void SetCountry(string country)
		{
			AddParameter(parameter_country, country);
		}

		public void SetRegion(string region)
		{
			AddParameter(parameter_region, region);
		}

		public void SetCity(string city)
		{
			AddParameter(parameter_city, city);
		}

		public void SetArea(string area)
		{
			AddParameter(parameter_area, area);
		}

		public void SetMetro(string metro)
		{
			AddParameter(parameter_metro, metro);
		}

		public void SetZip(string zip)
		{
			AddParameter(parameter_zip, zip);
		}

		public void SetCarrier(string carrier)
		{
			AddParameter(parameter_carrier, carrier);
		}

		public void SetSizeRequired(bool sizeRequired)
		{
			if (sizeRequired)
			{
				AddParameter(parameter_size_required, "1");
			}
			else
			{
				RemoveParameter(parameter_size_required);
			}
		}

		public void SetDeviceId(string deviceId)
		{
			AddParameter(parameter_device_id, deviceId);
		}

		public void SetVersion(string version)
		{
			AddParameter(parameter_version, version);
		}

		public void AddExternalCampaign(string campaign_id)
		{
			AddParameter(parameter_excampaigns, campaign_id);
		}

		public override string ToString()
		{
			string url = String.Empty;

			foreach (string parameter in parameters.Keys)
			{
				string value = parameters[parameter];

				if (parameter == parameter_latitude)
				{
					if (value == null)
					{
						double latitude = AutoDetectParameters.Instance.Latitude;
						if (!Double.IsNaN(latitude))
						{
							value = latitude.ToString("0.0");
						}
					}
				}
				else if (parameter == parameter_longitude)
				{
					if (value == null)
					{
						double longitude = AutoDetectParameters.Instance.Longitude;
						if (!Double.IsNaN(longitude))
						{
							value = longitude.ToString("0.0");
						}
					}
				}
				else if (parameter == parameter_connection_speed)
				{
					value = AutoDetectParameters.Instance.ConnectionSpeed;
				}

				if (!String.IsNullOrEmpty(value))
				{
					url += "&" + parameter + "=" + System.Net.HttpUtility.UrlEncode(value);
				}
			}

			try
			{
				url = url.Substring(1);
			}
			catch (System.Exception)
			{ }

			return adserverURL + "?" + url;
		}
		#endregion

		#region "Private methods"
		private void SetupAutodetectParameters()
		{
			AddParameter(parameter_connection_speed, null);
			AddParameter(parameter_latitude, null);
			AddParameter(parameter_longitude, null);
		}

		private void AddParameter(string key, string value)
		{
			if (parameters.ContainsKey(key))
			{
				parameters[key] = value;
			}
			else
			{
				parameters.Add(key, value);
			}
		}

		private void RemoveParameter(string key)
		{
			try
			{
				parameters.Remove(key);
			}
			catch (System.Exception)
			{ }
		}
		#endregion
	}
}
