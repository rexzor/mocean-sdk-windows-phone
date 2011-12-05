/****
 * © 2010-2011 mOcean Mobile. A subsidiary of Mojiva, Inc. All Rights Reserved.
 * */

using System;
#if SILVERLIGHT
using System.Windows;
#endif
using System.Text;
using Microsoft.Xna.Framework;
using System.Reflection;

namespace mOceanWindowsPhone
{
	public class Logger
	{
		#region "Constants"
		public enum LogLevel
		{
			Disabled,
			ErrorsOnly,
			ErrorsAndInfo,
			All
		}

		internal const string START_AD_DOWNLOAD = "Start Ad Download";
		internal const string START_LOG = "Start Log";
		internal const string DISPLAY_DEFAULT_IMAGE = "Display default image";
		internal const string UA_DETECTED = "UA detected";
		internal const string GPS_COORDINATES_DETECTED = "GPS coordinates detected";
		internal const string GET_SERVER_RESPONSE = "Get Server Response";
		internal const string FAIL_AD_DOWNLOAD = "Fail Ad Download";
		internal const string FINISH_AD_DOWNLOAD = "Finish Ad Download";
		internal const string START_RENDER_AD = "Start Render Ad";
		internal const string AD_DISPLAYED = "Ad Displayed";
		internal const string AD_DISPLAY_ERROR = "Ad Display error";
		internal const string AD_BECOME_VISIBLE = "Ad Become Visible";
		internal const string AD_BECOME_INVISIBLE = "Ad Become Invisible";
		internal const string OPEN_INTERNAL_BROWSER = "Open Internal Browser";
		internal const string CLOSE_INTERNAL_BROWSER = "Close Internal Browser";
		internal const string WRONG_PARAMETER = "Wrong parameter";
		#endregion

		#region "Variables"
		private string appName = String.Empty;
		#endregion

		#region "Properties"
		#endregion

		#region "Public"
		public Logger()
		{
#if SILVERLIGHT
			appName = Deployment.Current.EntryPointAssembly;
#else
			try
			{
				appName = ((AssemblyTitleAttribute)Assembly.GetCallingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), true)[0]).Title;
			}
			catch (System.Exception)
			{}
			
#endif
			System.Diagnostics.Debug.WriteLine(GetDateTime() + " " + appName + " :: AdSDK " + START_LOG);
		}
		#endregion

		#region "Private, internal"
#if SILVERLIGHT
		internal void WriteLine(LogLevel logLevel, string className, int viewId, string message, string parameter = null)
#else
		internal void WriteLine(LogLevel logLevel, string className, int viewId, string message, string parameter)
#endif
		{
			StringBuilder log = new StringBuilder();
			log.Append(GetDateTime() + " " + appName + " :: AdSDK " + className);
			
			if (viewId >= 0)
			{
				log.Append(" [" + viewId.ToString() + "]");
			}

			log.Append(" " + message);

			if (!String.IsNullOrEmpty(parameter))
			{
				log.Append(" | " + parameter);
			}

			System.Diagnostics.Debug.WriteLine(log);
		}

		private string GetDateTime()
		{
			DateTime dt = DateTime.Now;
			return dt.Year.ToString() + "-" + dt.Month.ToString() + "-" + dt.Day.ToString() + " " + dt.ToLongTimeString() + "." + dt.Millisecond.ToString("D3");
		}
		#endregion
	}
}
