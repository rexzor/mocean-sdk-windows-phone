/****
 * © 2010-2011 mOcean Mobile. A subsidiary of Mojiva, Inc. All Rights Reserved.
 * */

using System;
using System.Net;
using System.Windows;
using Microsoft.Phone.Net.NetworkInformation;
using System.Threading;

namespace MojivaPhone
{
	internal class CNetwork
	{
		static CNetwork instance = null;
		static readonly object padlock = new object();

		private Thread networkCheckThread = null;
		private NetworkInterfaceType currType = NetworkInterfaceType.None;
		private NetworkInterfaceType lastType = NetworkInterfaceType.None;
		private const int NETWORK_CHECK_PERIOD = 100;
		private event EventHandler NetworkChangeEvent;

		CNetwork()
		{
			currType = NetworkInterface.NetworkInterfaceType;
			lastType = NetworkInterface.NetworkInterfaceType;

			networkCheckThread = new Thread(new ThreadStart(NetWorkCheckProc));
			networkCheckThread.Name = "networkCheckThread";
			networkCheckThread.Start();
		}

		~CNetwork()
		{
			try
			{
				networkCheckThread.Abort();
				networkCheckThread.Join();
			}
			catch (System.Exception /*ex*/)
			{ }
			finally
			{
				networkCheckThread = null;
			}
		}

		public static CNetwork Instance
		{
			get
			{
				lock (padlock)
				{
					if (instance == null)
					{
						instance = new CNetwork();
					}
					return instance;
				}
			}
		}

		public static void Release()
		{
			if (instance != null)
			{
				instance = null;
			}
		}

		private void NetWorkCheckProc()
		{
			while (true)
			{
				lastType = NetworkInterface.NetworkInterfaceType;		// ~20 секунд

				if (lastType != currType)
				{
					//System.Diagnostics.Debug.WriteLine("send offline msg time: " + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + ":" + DateTime.Now.Millisecond);
					OnNetworkChanged();
				}

				currType = lastType;

				Thread.Sleep(NETWORK_CHECK_PERIOD);
			}
		}

		private void OnNetworkChanged()
		{
			if (NetworkChangeEvent != null)
			{
				NetworkChangeEvent(this, EventArgs.Empty);
			}

			//Deployment.Current.Dispatcher.BeginInvoke(() => fireEventDelegate("networkChange", new string[] { Microsoft.Phone.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable().ToString(), GetStatus() }));
		}

		public string GetStatus()
		{
			string netWorkStatus = "offline";
			//*
			NetworkInterfaceType lastType = NetworkInterface.NetworkInterfaceType;

			switch (lastType)
			{
				case NetworkInterfaceType.Wireless80211:
					netWorkStatus = "wifi";
					break;
				case NetworkInterfaceType.MobileBroadbandGsm:
					netWorkStatus = "cell";
					break;
				case NetworkInterfaceType.MobileBroadbandCdma:
					netWorkStatus = "cdma";
					break;
				case NetworkInterfaceType.Unknown:
					netWorkStatus = "unknown";
					break;
			}
			//*/
			return netWorkStatus;
		}

		public event EventHandler NetWorkChange
		{
			add
			{
				NetworkChangeEvent += value;
			}
			remove
			{
				NetworkChangeEvent -= value;
			}
		}
	}
}
