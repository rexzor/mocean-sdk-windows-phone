/****
 * © 2010-2011 mOcean Mobile. A subsidiary of Mojiva, Inc. All Rights Reserved.
 * */

using System;
using System.Collections.Generic;

namespace MojivaPhone
{
	/// <summary>
	/// Builds http request with all parameters
	/// </summary>
	internal class AdserverRequest
	{
		#region "Variables"
		private Dictionary<String, String> parameters = new Dictionary<String, String>();
		private Dictionary<String, String> _customParams = new Dictionary<String, String>();
		private String parameter_site = "site";
		private String parameter_userAgent = "ua";
		private String parameter_url = "url";
		private String parameter_keywords = "keywords";
		private String parameter_premium = "premium";
		private String parameter_zone = "zone";
		private String parameter_test = "test";
		private String parameter_count = "count";
		private String parameter_country = "country";
		private String parameter_city = "city";
		private String parameter_area = "area";
		private String parameter_metro = "metro";
		private String parameter_zip = "zip";
		private String parameter_region = "region";
		private String parameter_output_format = "key";
		private String parameter_adstype = "adstype";
		private String parameter_over_18 = "over_18";
		private String parameter_latitude = "lat";
		private String parameter_longitude = "long";
		private String parameter_course = "course";
		private String parameter_textborder = "textborder";
		private String parameter_border = "paramBORDER";
		private String parameter_background = "paramBG";
		private String parameter_link = "paramLINK";
		private String parameter_carrier = "carrier";
		private String parameter_target = "target";
		private String parameter_pixel = "pixel";
		private String parameter_min_size_x = "min_size_x";
		private String parameter_min_size_y = "min_size_y";
		private String parameter_size_x = "size_x";
		private String parameter_size_y = "size_y";
		private String parameter_version = "version";
		private String parameter_network = "connection_speed";
		private String parameter_size_required = "size_required";
		private String parameter_udid = "udid";

		private String parameter_excampaigns = "excampaigns";
		private List<String> exCampaignIds = new List<String>();
		#endregion

		private String adserverURL = String.Empty;

		/// <summary>
		/// Class constructor
		/// </summary>
		public AdserverRequest()
		{
			SetupAutodetectParameters();
		}

		/// <summary>
		/// Sets up autodetect parameters (if user omitted them)
		/// </summary>
		protected void SetupAutodetectParameters()
		{
			// Autodetect following parameters
			if (!parameters.ContainsKey(parameter_country)) parameters.Add(parameter_country, "");
			if (!parameters.ContainsKey(parameter_course)) parameters.Add(parameter_course, "");
			if (!parameters.ContainsKey(parameter_carrier)) parameters.Add(parameter_carrier, "");
			if (!parameters.ContainsKey(parameter_userAgent)) parameters.Add(parameter_userAgent, "");
			if (!parameters.ContainsKey(parameter_network)) parameters.Add(parameter_network, "");
		}

		/// <summary>
		/// Get url of AD server
		/// </summary>
		/// <returns>url</returns>
		public String getAdserverURL()
		{
			return adserverURL;
		}

		/// <summary>
		/// Overrides the URL of ad server.
		/// </summary>
		/// <param name="adserverURL">new url</param>
		public void setAdserverURL(String adserverURL)
		{
			this.adserverURL = adserverURL;
		}

		public bool SizeRequired
		{
			get { return (parameter_size_required == "1"); }
			set { AddParameter(parameter_size_required, value ? "1" : "0"); }
		}

		/// <summary>
		/// Add parameter to list if missing or update if present
		/// </summary>
		/// <param name="key">Param name</param>
		/// <param name="value">Param value</param>
		protected void AddParameter(String key, String value)
		{
			if (parameters.ContainsKey(key)) parameters[key] = value;
			else parameters.Add(key, value);
		}

		/// <summary>
		/// Required.
		/// Set the id of the publisher site. 
		/// </summary>
		/// <param name="site">Id of the site assigned by Adserver</param>
		/// <returns>AdserverRequest</returns>
		public AdserverRequest setSite(String site)
		{
			if (!String.IsNullOrEmpty(site))
			{
				AddParameter(parameter_site, site);
			}
			return this;
		}

		/// <summary>
		/// Optional.
		/// Set the browser user agent of the device making the request.
		/// </summary>
		/// <param name="ua">User agent</param>
		/// <returns>AdserverRequest</returns>
		public AdserverRequest setUa(String ua)
		{
			if (!String.IsNullOrEmpty(ua))
			{
				AddParameter(parameter_userAgent, ua);
			}
			return this;
		}

		/// <summary>
		/// Required.
		/// Set URL of site for which it is necessary to receive advertising.
		/// </summary>
		/// <param name="url">url</param>
		/// <returns>AdserverRequest</returns>
		public AdserverRequest setUrl(String url)
		{
			if (!String.IsNullOrEmpty(url))
			{
				AddParameter(parameter_url, url);
			}
			return this;
		}

		/// <summary>
		/// Optional.
		/// Set Keywords to search ad delimited by commas.
		/// </summary>
		/// <param name="keywords">keywords</param>
		/// <returns>AdserverRequest</returns>
		public AdserverRequest setKeywords(String keywords)
		{
			if (!String.IsNullOrEmpty(keywords))
			{
				AddParameter(parameter_keywords, keywords);
			}
			return this;
		}

		/// <summary>
		/// Optional.
		/// Set Filter by premium (PREMIUM_STATUS_NON_PREMIUM - non-premium, 
		/// PREMIUM_STATUS_PREMIUM - premium only, PREMIUM_STATUS_BOTH - both). 
		/// Can be used only by premium publishers.
		/// </summary>
		/// <param name="premium">premium</param>
		/// <returns>AdserverRequest</returns>
		public AdserverRequest setPremium(int premium)
		{
			if (premium != 0)
				AddParameter(parameter_premium, (premium - 1).ToString());
			return this;
		}

		/// <summary>
		/// Required.
		/// Set the id of the zone of publisher site.
		/// </summary>
		/// <param name="zone">zone</param>
		/// <returns>AdserverRequest</returns>
		public AdserverRequest setZone(String zone)
		{
			if (!String.IsNullOrEmpty(zone))
			{
				AddParameter(parameter_zone, zone);
			}
			return this;
		}

		/// <summary>
		/// Optional.
		/// Set Default setting is test mode where, if the ad code is properly installed, 
		/// the ad response is "Test MODE".
		/// </summary>
		/// <param name="enabled">enabled</param>
		/// <returns>AdserverRequest</returns>
		public AdserverRequest setTestModeEnabled(bool enabled)
		{
			if (enabled)
			{
				AddParameter(parameter_test, "1");
			}
			else
			{
				if (parameters.ContainsKey(parameter_test)) parameters.Remove(parameter_test);
			}
			return this;
		}

		/// <summary>
		/// Optional.
		/// Set Quantity of ads, returned by a server. Maximum value is 5.
		/// </summary>
		/// <param name="count">count</param>
		/// <returns>AdserverRequest</returns>
		public AdserverRequest setCount(int count)
		{
			if (count != 0)
				AddParameter(parameter_count, count.ToString());
			return this;
		}

		/// <summary>
		/// Optional.
		/// Set Country of visitor. See codes here (http://www.mojiva.com/docs/iso3166.csv). 
		/// Will override country detected by IP. 
		/// </summary>
		/// <param name="country">country</param>
		/// <returns>AdserverRequest</returns>
		public AdserverRequest setCountry(String country)
		{
			if (!String.IsNullOrEmpty(country))
			{
				AddParameter(parameter_country, country);
			}
			return this;
		}

		public AdserverRequest setCity(String city)
		{
			if (!String.IsNullOrEmpty(city))
			{
				AddParameter(parameter_city, city);
			}
			return this;
		}

		public AdserverRequest setArea(String area)
		{
			if (!String.IsNullOrEmpty(area))
			{
				AddParameter(parameter_area, area);
			}
			return this;
		}

		public AdserverRequest setMetro(String metro)
		{
			if (!String.IsNullOrEmpty(metro))
			{
				AddParameter(parameter_metro, metro);
			}
			return this;
		}

		public AdserverRequest setZip(String zip)
		{
			if (!String.IsNullOrEmpty(zip))
			{
				AddParameter(parameter_zip, zip);
			}
			return this;
		}

		/// <summary>
		/// Optional.
		/// Set Region of visitor. See codes for US and Canada here (http://www.mojiva.com/docs/iso3166_2.csv), 
		/// others - here (http://www.mojiva.com/docs/fips10_4.csv). 
		/// </summary>
		/// <param name="region">region</param>
		/// <returns>AdserverRequest</returns>
		public AdserverRequest setRegion(String region)
		{
			if (!String.IsNullOrEmpty(region))
			{
				AddParameter(parameter_region, region);
			}
			return this;
		}

		/// <summary>
		/// Output format. Normal format uses key = 1. Parameter key should be set to 3 in order to use XML output and to 5 in order to use JSON output.
		/// </summary>
		public AdserverRequest setOutputFormat(int key)
		{
			AddParameter(parameter_output_format, key.ToString());
			return this;
		}

		/// <summary>
		/// Optional.
		/// Set Type of advertisement (ADS_TYPE_TEXT_ONLY - text only, 
		/// ADS_TYPE_IMAGES_ONLY - image only, ADS_TYPE_TEXT_AND_IMAGES - image and text, 
		/// ADS_TYPE_SMS - SMS ad). SMS will be returned in XML.
		/// </summary>
		/// <param name="adstype">type</param>
		/// <returns>AdserverRequest</returns>
		public AdserverRequest setAdstype(int adstype)
		{
			if (adstype != 0)
				AddParameter(parameter_adstype, adstype.ToString());
			return this;
		}

		/// <summary>
		/// Optional.
		/// Set Filter by ad over 18 content (OVER_18_TYPE_DENY - deny over 18 content , 
		/// OVER_18_TYPE_ONLY - only over 18 content, OVER_18_TYPE_ALL - allow all ads 
		/// including over 18 content).
		/// </summary>
		/// <param name="over18">over18</param>
		/// <returns>AdserverRequest</returns>
		public AdserverRequest setOver18(int over18)
		{
			if (over18 != 0)
				AddParameter(parameter_over_18, over18.ToString());
			return this;
		}

		/// <summary>
		/// Optional.
		/// Set Latitude.
		/// </summary>
		/// <param name="latitude">latitude</param>
		/// <returns>AdserverRequest</returns>
		public AdserverRequest setLatitude(String latitude)
		{
			//if(!String.IsNullOrEmpty(latitude))
			if (latitude != String.Empty)
			{
				AddParameter(parameter_latitude, latitude);
			}
			return this;
		}

		/// <summary>
		/// Optional.
		/// Set Longitude.
		/// </summary>
		/// <param name="longitude">longitude</param>
		/// <returns>AdserverRequest</returns>
		public AdserverRequest setLongitude(String longitude)
		{
			//if(!String.IsNullOrEmpty(longitude))
			if (longitude != String.Empty)
			{
				AddParameter(parameter_longitude, longitude);
			}
			return this;
		}

		/// <summary>
		/// Optional.
		/// Set Course.
		/// </summary>
		/// <param name="course">course</param>
		/// <returns>AdserverRequest</returns>
		public AdserverRequest setCourse(String course)
		{
			if (!String.IsNullOrEmpty(course))
			{
				AddParameter(parameter_course, course);
			}
			return this;
		}

		/// <summary>
		/// Optional.
		/// Show borders around text ads.
		/// </summary>
		/// <param name="enabled">show or not</param>
		/// <returns>AdserverRequest</returns>
		public AdserverRequest setTextborderEnabled(bool enabled)
		{
			if (enabled)
			{
				AddParameter(parameter_textborder, "1");
			}
			else
			{
				AddParameter(parameter_textborder, "0");
			}
			return this;
		}

		/// <summary>
		/// Optional.
		/// Set Borders color.
		/// </summary>
		/// <param name="paramBorder">color</param>
		/// <returns>AdserverRequest</returns>
		public AdserverRequest setParamBorder(String paramBorder)
		{
			if (!String.IsNullOrEmpty(paramBorder))
			{
				AddParameter(parameter_border, paramBorder);
			}
			return this;
		}

		/// <summary>
		/// Optional.
		/// Set Background color in borders.
		/// </summary>
		/// <param name="paramBG">color</param>
		/// <returns>AdserverRequest</returns>
		public AdserverRequest setParamBG(String paramBG)
		{
			if (!String.IsNullOrEmpty(paramBG))
			{
				AddParameter(parameter_background, paramBG);
			}
			return this;
		}

		/// <summary>
		/// Optional.
		/// Set Text color.
		/// </summary>
		/// <param name="paramLINK">color</param>
		/// <returns>AdserverRequest</returns>
		public AdserverRequest setParamLINK(String paramLINK)
		{
			if (!String.IsNullOrEmpty(paramLINK))
			{
				AddParameter(parameter_link, paramLINK);
			}
			return this;
		}

		/// <summary>
		/// Optional.
		/// Set Carrier name.
		/// </summary>
		/// <param name="carrier">name</param>
		/// <returns>AdserverRequest</returns>
		public AdserverRequest setCarrier(String carrier)
		{
			if (!String.IsNullOrEmpty(carrier))
			{
				AddParameter(parameter_carrier, carrier);
			}
			return this;
		}

		public AdserverRequest setImageMinWidth(int size)
		{
			if (size != 0)
				AddParameter(parameter_min_size_x, size.ToString());
			return this;
		}

		public AdserverRequest setImageMinHeight(int size)
		{
			if (size != 0)
				AddParameter(parameter_min_size_y, size.ToString());
			return this;
		}

		public AdserverRequest setImageMaxWidth(int size)
		{
			if (size != 0)
				AddParameter(parameter_size_x, size.ToString());
			return this;
		}

		public AdserverRequest setImageMaxHeight(int size)
		{
			if (size != 0)
				AddParameter(parameter_size_y, size.ToString());
			return this;
		}

		/// <summary>
		/// Optional.
		/// Set Target attribute for "a href" (TARGET_BLANK - open the linked document 
		/// in a new window, TARGET_SELF - open the linked document in the same frame, 
		/// TARGET_PARENT - open the linked document in the parent frameset, 
		/// TARGET_TOP - open the linked document in the full body of the window)
		/// </summary>
		/// <param name="target">target</param>
		/// <returns>AdserverRequest</returns>
		public AdserverRequest setTarget(String target)
		{
			if (!String.IsNullOrEmpty(target))
			{
				AddParameter(parameter_target, target);
			}
			return this;
		}

		/// <summary>
		/// Optional.
		/// Set for return redirect to image for ad (or tracking pixel 1x1) directly 
		/// (instead of html response).
		/// </summary>
		/// <param name="enabled">enabled</param>
		/// <returns>AdserverRequest</returns>
		public AdserverRequest setPixelModeEnabled(bool enabled)
		{
			if (enabled)
			{
				AddParameter(parameter_pixel, "1");
			}
			else
			{
				if (parameters.ContainsKey(parameter_pixel)) parameters.Remove(parameter_pixel);
			}
			return this;
		}

		/// <summary>
		/// Optional.
		/// Set Version.
		/// </summary>
		/// <param name="version">version</param>
		/// <returns>AdserverRequest</returns>
		public AdserverRequest setVersion(String version)
		{
			if (!String.IsNullOrEmpty(version))
			{
				AddParameter(parameter_version, version);
			}
			return this;
		}

		public AdserverRequest setUdid(string udid)
		{
			if (!String.IsNullOrEmpty(udid))
			{
				AddParameter(parameter_udid, udid);
			}
			return this;
		}

		public String getSite()
		{
			return parameters.ContainsKey(parameter_site) ? parameters[parameter_site] : String.Empty;
		}

		public String getUa()
		{
			if (parameters.ContainsKey(parameter_userAgent) && !String.IsNullOrEmpty(parameters[parameter_userAgent]))
			{
				return parameters[parameter_userAgent];
			}

			return String.Empty;
		}

		public String getUrl()
		{
			return parameters.ContainsKey(parameter_url) ? parameters[parameter_url] : String.Empty;
		}

		public String getKeywords()
		{
			return parameters.ContainsKey(parameter_keywords) ? parameters[parameter_keywords] : String.Empty;
		}

		public int getPremium()
		{
			if (parameters.ContainsKey(parameter_premium))
			{
				String premium = parameters[parameter_premium];
				return getIntParameter(premium);
			}

			return 10;
		}

		public String getZone()
		{
			return parameters.ContainsKey(parameter_zone) ? parameters[parameter_zone] : String.Empty;
		}

		public int getCount()
		{
			if (parameters.ContainsKey(parameter_count))
			{
				String count = parameters[parameter_count];
				return getIntParameter(count);
			}

			return 0;
		}

		public String getCountry()
		{
			if (parameters.ContainsKey(parameter_country) && !String.IsNullOrEmpty(parameters[parameter_country]))
				return parameters[parameter_country];
			else if (!String.IsNullOrEmpty(AutoDetectParameters.Instance.Country))
			{
				return AutoDetectParameters.Instance.Country;
			}

			return String.Empty;
		}

		public String getRegion()
		{
			return parameters.ContainsKey(parameter_region) ? parameters[parameter_region] : String.Empty;
		}

		public String getCity()
		{
			return parameters.ContainsKey(parameter_city) ? parameters[parameter_city] : String.Empty;
		}

		public String getArea()
		{
			return parameters.ContainsKey(parameter_area) ? parameters[parameter_area] : String.Empty;
		}

		public String getMetro()
		{
			return parameters.ContainsKey(parameter_metro) ? parameters[parameter_metro] : String.Empty;
		}

		public String getZip()
		{
			return parameters.ContainsKey(parameter_zip) ? parameters[parameter_zip] : String.Empty;
		}

		public int getAdstype()
		{
			if (parameters.ContainsKey(parameter_adstype))
			{
				String adstype = parameters[parameter_adstype];
				return getIntParameter(adstype);
			}

			return 0;
		}

		public int getOver18()
		{
			if (parameters.ContainsKey(parameter_over_18))
			{
				String over_18 = parameters[parameter_over_18];
				return getIntParameter(over_18);
			}

			return 0;
		}

		public String getLatitude()
		{
			if (parameters.ContainsKey(parameter_latitude))
			{
				if (parameters[parameter_latitude] == null)
				{
					if (!Double.IsNaN(AutoDetectParameters.Instance.Latitude))
					{
						return AutoDetectParameters.Instance.Latitude.ToString("0.0");
					}
				}
				else
				{
					return parameters[parameter_latitude];
				}
			}

			return String.Empty;
		}

		public String getLongitude()
		{
			if (parameters.ContainsKey(parameter_longitude))
			{
				if (parameters[parameter_longitude] == null)
				{
					if (!Double.IsNaN(AutoDetectParameters.Instance.Longitude))
					{
						return AutoDetectParameters.Instance.Longitude.ToString("0.0");
					}
				}
				else
				{
					return parameters[parameter_longitude];
				}
			}

			return String.Empty;
		}

		public String getCourse()
		{
			if (parameters.ContainsKey(parameter_course) && !String.IsNullOrEmpty(parameters[parameter_course]))
			{
				return parameters[parameter_course];
			}
			else if (AutoDetectParameters.Instance.Course != 0)
			{
				return AutoDetectParameters.Instance.Course.ToString();
			}
			return String.Empty;
		}

		public String getParamBorder()
		{
			return parameters.ContainsKey(parameter_border) ? parameters[parameter_border] : String.Empty;
		}

		public String getParamBG()
		{
			return parameters.ContainsKey(parameter_background) ? parameters[parameter_background] : String.Empty;
		}

		public String getParamLINK()
		{
			return parameters.ContainsKey(parameter_link) ? parameters[parameter_link] : String.Empty;
		}

		public String getCarrier()
		{
			if (parameters.ContainsKey(parameter_carrier) && !String.IsNullOrEmpty(parameters[parameter_carrier]))
				return parameters[parameter_carrier];
			else if (!String.IsNullOrEmpty(AutoDetectParameters.Instance.Carrier))
			{
				return AutoDetectParameters.Instance.Carrier;
			}

			return String.Empty;
		}

		public String getNetwork()
		{
			string networktype = AutoDetectParameters.Instance.NetworkType;
			if (networktype == "wifi" || networktype == "cdma")
			{
				return "1";
			}
			else
			{
				return "0";
			}
		}

		public int getImageMinWidth()
		{
			String image_size = parameters[parameter_min_size_x];
			return getIntParameter(image_size);
		}

		public int getImageMinHeight()
		{
			String image_size = parameters[parameter_min_size_y];
			return getIntParameter(image_size);
		}

		public int getImageMaxWidth()
		{
			String image_size = parameters[parameter_size_x];
			return getIntParameter(image_size);
		}

		public int getImageMaxHeight()
		{
			String image_size = parameters[parameter_size_y];
			return getIntParameter(image_size);
		}

		public int getTarget()
		{
			String target = parameters[parameter_target];
			return getIntParameter(target);
		}

		public String getVersion()
		{
			return parameters.ContainsKey(parameter_version) ? parameters[parameter_version] : String.Empty;
		}

		public String getUdid()
		{
			return parameters.ContainsKey(parameter_udid) ? parameters[parameter_udid] : String.Empty;
		}

		private int getIntParameter(String stringValue)
		{
			if (stringValue != null)
			{
				return Int32.Parse(stringValue);
			}
			else
			{
				return 0;
			}
		}

		public void SetCustomParameters(Dictionary<String, String> pars)
		{
			_customParams = pars;
		}

		public void SetCustomParameters(String customParameters)
		{
			_customParams.Clear();

			try
			{
				String[] parArr = customParameters.Split(new char[] { ',' });
				for (int f = 0; f < parArr.Length; f += 2)
				{
					_customParams.Add(parArr[f], parArr[f + 1]);
				}
			}
			catch (System.Exception /*ex*/)
			{ }
		}

		public void SetCustomParameter(String strName, String strValue)
		{
			_customParams[strName] = strValue;
		}

		public bool RemoveCustomParameter(String strName)
		{
			return _customParams.Remove(strName);
		}

		public bool RemoveCustomParameter(int nIdx)
		{
			if (nIdx >= 0 && nIdx < _customParams.Keys.Count)
			{
				int k = 0;
				foreach (var s in _customParams)
				{
					if (k == nIdx)
					{
						_customParams.Remove(s.Key);
						break;
					}
					k++;
				}

				return true;
			}

			return false;
		}

		public String GetCustomParameter(String strName)
		{
			if (_customParams.ContainsKey(strName)) return _customParams[strName];

			return String.Empty;
		}

		public String GetCustomParameter(int nIdx)
		{
			if (nIdx >= 0 && nIdx < _customParams.Keys.Count)
			{
				int k = 0;
				foreach (var s in _customParams)
				{
					if (k == nIdx)
					{
						return _customParams[s.Key];
					}
					k++;
				}
			}

			return String.Empty;
		}

		/**
			* Creates URL with given parameters.
			*/
		public String createURL()
		{
			return ToString();
		}

		public override string ToString()
		{
			String builderToString = String.Empty;
			String adserverURL = this.adserverURL + "?";

			foreach (String item in parameters.Keys)
			{
				String param = item;
				String value = parameters[item];

				// for auto-detect parameters
				if (item == parameter_userAgent)
				{
					value = getUa();
				}
				else if (item == parameter_country)
				{
					value = getCountry();
				}
				else if (item == parameter_city)
				{
					value = getCity();
				}
				else if (item == parameter_area)
				{
					value = getArea();
				}
				else if (item == parameter_metro)
				{
					value = getMetro();
				}
				else if (item == parameter_zip)
				{
					value = getZip();
				}
				else if (item == parameter_latitude)
				{
					value = getLatitude();
				}
				else if (item == parameter_longitude)
				{
					value = getLongitude();
				}
				else if (item == parameter_course)
				{
					value = getCourse();
				}
				else if (item == parameter_carrier)
				{
					value = getCarrier();
				}
				else if (item == parameter_network)
				{
					value = getNetwork();
				}
				else if (item == parameter_excampaigns)
				{
					value = GetExCampaigns();
				}

				if (!String.IsNullOrEmpty(value))
				{
					builderToString += "&" + param + "=" + System.Net.HttpUtility.UrlEncode(value);
				}
			}

			try
			{
				builderToString = builderToString.Substring(1);
			}
			catch (System.Exception /*ex*/)
			{ }

			builderToString = adserverURL + builderToString;

			return builderToString;
		}

		public AdserverRequest AddExCampaign(string campaign_id)
		{
			AddParameter(parameter_excampaigns, campaign_id);

			if (!String.IsNullOrEmpty(campaign_id) && !exCampaignIds.Contains(campaign_id))
			{
				exCampaignIds.Add(campaign_id);
			}
			return this;
		}

		private String GetExCampaigns()
		{
			String exCampaigns = String.Empty;

			for (int i = 0; i < exCampaignIds.Count; i++)
			{
				exCampaigns += exCampaignIds[i];

				if (i < exCampaignIds.Count - 1)
				{
					exCampaigns += ",";
				}
			}

			return exCampaigns;
		}
	}
}
