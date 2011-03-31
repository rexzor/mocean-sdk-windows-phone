/****
 * © 2010-2011 mOcean Mobile. A subsidiary of Mojiva, Inc. All Rights Reserved.
 * */

using System.Collections.Generic;
using Microsoft.Phone.Tasks;

namespace MojivaPhone
{
	internal class CNativeAppManager
	{
		public CNativeAppManager()
		{
		}

		public void RunApp(string appName, List<string> args)
		{
			switch (appName)
			{
				case "makecall":
					MakeCall(args[0]);
					break;
				case "sendmail":
					SendMail(args[0], args[1], args[2]);
					break;
				case "sendsms":
					SendSms(args[0], args[1]);
					break;
			}
		}

		private void MakeCall(string number)
		{
			PhoneCallTask callTask = new PhoneCallTask();
			callTask.PhoneNumber = number;
			callTask.DisplayName = "";
			callTask.Show();
		}

		private void SendMail(string recipient, string subject, string body)
		{
			EmailComposeTask emailTask = new EmailComposeTask();
			emailTask.To = recipient;
			emailTask.Cc = "";
			emailTask.Subject = subject;
			emailTask.Body = body;
			emailTask.Show();
		}

		private void SendSms(string recipient, string body)
		{
			SmsComposeTask smsTask = new SmsComposeTask();
			smsTask.To = recipient;
			smsTask.Body = body;
			smsTask.Show();
		}
	}
}
