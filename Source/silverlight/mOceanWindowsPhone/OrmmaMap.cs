/****
 * © 2010-2011 mOcean Mobile. A subsidiary of Mojiva, Inc. All Rights Reserved.
 * */

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Microsoft.Phone.Controls.Maps;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Controls.Maps.Platform;
using System.Windows.Media;
using System.Device.Location;
using System.Collections.Generic;

namespace mOceanWindowsPhone
{
	internal class OrmmaMap : PopupWindow
	{
		private const string KEY = "AogiRkuyqKxbvxlcu99SUeRp5M9V3j4hije0x4onsbMqkoOEE6p4QNPkA6mjqvxD";
		private const double DEFAULT_ZOOM_LEVEL = 0;
		private const double DEFAULT_LATITUDE = 56.632778;
		private const double DEFAULT_LONGITUDE = 47.895833;
		private Color PUSHPIN_COLOR = Colors.LightGray;
		private Color PUSHPIN_TEXT_COLOR = Colors.Black;
		private const int PUSHPIN_TEXT_SIZE = 30;

		private const string MAP_TYPE_PARAM_NAME = "t";
		private const string MAP_TYPE_MAP = "m";
		private const string MAP_TYPE_SATELLITE = "k";
		private const string MAP_TYPE_HYBRID = "h";
		private const string MAP_TYPE_TERRAIN = "p";
		private const string MAP_TYPE_GOOGLE_EARTH = "e";

		private const string QUERY_PARAM_NAME = "q";
		private const string CENTER_PARAM_NAME = "ll";
		private const string ZOOM_LEVEL_PARAM_NAME = "z";

		private const string PROOF = "http://maps.google.ru/maps/ms?gl=ru&ie=UTF8&msa=0&msid=213579429772951119527.0004a3c5ac3165f7db1ea&ll=56.674338,47.891464&spn=0.19013,0.623474&t=h&z=11&iwloc=0004a3c5ac333eb8e4989";

		public OrmmaMap()
		{
			control = new Map();
			((Map)control).LoadingError += MapLoadingError;
			((Map)control).CredentialsProvider = new ApplicationIdCredentialsProvider(KEY);
			((Map)control).CopyrightVisibility = Visibility.Collapsed;
			((Map)control).LogoVisibility = Visibility.Collapsed;
			((Map)control).ZoomBarVisibility = Visibility.Visible;
			((Map)control).Center = new GeoCoordinate(DEFAULT_LATITUDE, DEFAULT_LONGITUDE);
			((Map)control).ZoomLevel = DEFAULT_ZOOM_LEVEL;
		}
		
		public OrmmaMap(PhoneApplicationPage page, Point parentPosition, Size size)
			: base(page, parentPosition, size)
		{
		}

		public void OpenPoi(string poiUrl)
		{
			Map mapControl = control as Map;
			if (mapControl != null)
			{
				Dictionary<string, string> poiParams = SplitPoiParams(poiUrl);
				//poiParams = SplitPoiParams(PROOF);

				if (poiParams != null)
				{
					mapControl.Children.Clear();

					Pushpin pushpin = new Pushpin();
					pushpin.Background = new SolidColorBrush(PUSHPIN_COLOR);

					foreach (string paramName in poiParams.Keys)
					{
						string paramValue = poiParams[paramName];

						if (paramName == MAP_TYPE_PARAM_NAME)
						{
							switch (paramValue)
							{
								case MAP_TYPE_SATELLITE:
								case MAP_TYPE_TERRAIN:
								case MAP_TYPE_GOOGLE_EARTH:
									mapControl.Mode = new Microsoft.Phone.Controls.Maps.AerialMode();
									break;
								case MAP_TYPE_MAP:
								case MAP_TYPE_HYBRID:
									mapControl.Mode = new Microsoft.Phone.Controls.Maps.RoadMode();
									break;
								default:
									break;
							}
						}
						else if (paramName == CENTER_PARAM_NAME)
						{
							Point center = ParseLocation(paramValue);
							mapControl.Center = new GeoCoordinate(center.X, center.Y);
							pushpin.Location = mapControl.Center;
						}
						else if (paramName == ZOOM_LEVEL_PARAM_NAME)
						{
							try
							{
								mapControl.ZoomLevel = Convert.ToDouble(paramValue);
							}
							catch (System.Exception)
							{}
						}
						else if (paramName == QUERY_PARAM_NAME)
						{
							TextBlock textBlock = new TextBlock();
							textBlock.Text = paramValue;
							pushpin.Content = textBlock;
							pushpin.FontSize = PUSHPIN_TEXT_SIZE;
							pushpin.Foreground = new SolidColorBrush(PUSHPIN_TEXT_COLOR);
						}
					}

					mapControl.Children.Add(pushpin);
				}
			}
		}

		#region "Properties"
		public Map MapControl
		{
			get { return (control as Map); }
			private set { }
		}
		#endregion

		protected override void Init()
		{
			BG_COLOR = Colors.Black;
			control = new Map();

			base.Init();
	
			((Map)control).LoadingError += MapLoadingError;
			((Map)control).CredentialsProvider = new ApplicationIdCredentialsProvider(KEY);
			((Map)control).CopyrightVisibility = Visibility.Collapsed;
			((Map)control).LogoVisibility = Visibility.Collapsed;
			((Map)control).ZoomBarVisibility = Visibility.Visible;
			((Map)control).Center = new GeoCoordinate(DEFAULT_LATITUDE, DEFAULT_LONGITUDE);
			((Map)control).ZoomLevel = DEFAULT_ZOOM_LEVEL;
		}

		private Dictionary<string, string> SplitPoiParams(string poiUrl)
		{
			Dictionary<string, string> result = null;

			try
			{
				poiUrl = System.Net.HttpUtility.UrlDecode(poiUrl);

				poiUrl = poiUrl.Replace("http://maps.google.com/maps?", String.Empty).Trim();
				string[] parameters = poiUrl.Split('&');

				if (parameters != null)
				{
					result = new Dictionary<string, string>();
					for (int i = 0; i < parameters.Length; i++)
					{
						string[] nameValue = parameters[i].Split('=');
						if (nameValue.Length == 2)
						{
							result.Add(nameValue[0], nameValue[1]);
						}
					}
				}
			}
			catch (System.Exception)
			{}

			return result;
		}

		private Point ParseLocation(string locationString)
		{
			Point location = new Point(0, 0);
			try
			{
				string[] coords = locationString.Split(',');
				location.X = Convert.ToDouble(coords[0]);
				location.Y = Convert.ToDouble(coords[1]);
			}
			catch (System.Exception)
			{
				location.X = location.Y = 0;
			}

			return location;
		}

		private void MapLoadingError(object sender, LoadingErrorEventArgs e)
		{
			Close();
		}
	}
}
