/****
 * © 2010-2011 mOcean Mobile. A subsidiary of Mojiva, Inc. All Rights Reserved.
 * */

using System;
using Microsoft.Phone.Tasks;

namespace mOceanWindowsPhone
{
	internal class NativeAppManager
	{
		public static void MakeCall(string number)
		{
			try
			{
				PhoneCallTask callTask = new PhoneCallTask();
				callTask.PhoneNumber = number;
				callTask.DisplayName = "";
				callTask.Show();
			}
			catch (Exception)
			{}
		}

		public static void SendMail(string recipient, string subject, string body)
		{
			try
			{
				EmailComposeTask emailTask = new EmailComposeTask();
				emailTask.To = recipient;
				emailTask.Cc = "";
				emailTask.Subject = subject;
				emailTask.Body = body;
				emailTask.Show();
			}
			catch (Exception)
			{ }
		}

		public static void SendSms(string recipient, string body)
		{
			try
			{
				SmsComposeTask smsTask = new SmsComposeTask();
				smsTask.To = recipient;
				smsTask.Body = body;
				smsTask.Show();
			}
			catch (Exception)
			{ }
		}

		public static void PlayMedia(string url, string properties)
		{
			try
			{
				MediaPlayerLauncher mediaPlayerLauncher = new MediaPlayerLauncher();
				mediaPlayerLauncher.Location = MediaLocationType.Data;
				mediaPlayerLauncher.Media = new Uri(url, UriKind.RelativeOrAbsolute);

				if (properties.Contains("controls"))
				{
					mediaPlayerLauncher.Controls = MediaPlaybackControls.All;
				}

				mediaPlayerLauncher.Show();
			}
			catch (Exception)
			{}
		}
	}
}
