/****
 * © 2010-2011 mOcean Mobile. A subsidiary of Mojiva, Inc. All Rights Reserved.
 * */

using System;
using System.Windows;
using Microsoft.Devices.Sensors;
using Microsoft.Xna.Framework;

namespace MojivaPhone
{
	internal class CAccelerometer
	{
		#region "Variables"
		private Accelerometer acc = null;
		private Vector3 tilt;
		private FireEventDelegate fireEventDelegate;
		private int intensity, interval;
		private AccelerometerReadingEventArgs lastReading = null;
		private int shakeCount = 0;
		private bool shaking = false;
		private const double ShakeThreshold = 0.7;
		#endregion

		#region "Public methods"
		public CAccelerometer(FireEventDelegate fireEventDelegate)
		{
			this.fireEventDelegate = fireEventDelegate;
			Init();
			Start();
		}

		public void SetProperties(int intensity, int interval)
		{
			this.intensity = intensity;
			this.interval = interval;
		}

		public void StartAccelListener()
		{
			acc.ReadingChanged += new EventHandler<AccelerometerReadingEventArgs>(Accel_ReadingChanged);
		}

		public void StopAccelListener()
		{
			acc.ReadingChanged -= new EventHandler<AccelerometerReadingEventArgs>(Accel_ReadingChanged);
			tilt = new Vector3(0);
		}

		public void StartShakeListener()
		{
			lastReading = null;
			lastReading = null;
			shakeCount = 0;
			shaking = false;
		}

		public void StopShakeListener()
		{
			tilt = new Vector3(0);
			lastReading = null;
		}

		public string[] TiltToStringArr()
		{
			return new string[] { tilt.X.ToString("F2"), tilt.Y.ToString("F2"), tilt.Z.ToString("F2") };
		}

		~CAccelerometer()
		{
			StopAccelListener();
			StopShakeListener();

			try
			{
				acc.Stop();
			}
			catch
			{ }
			finally
			{
				acc.Dispose();
				acc = null;
			}

			fireEventDelegate = null;
		}
		#endregion

		#region "Private methods"
		private void Init()
		{
			acc = new Accelerometer();
			tilt = new Vector3(0);
			intensity = interval = 0;
			SetProperties(intensity, interval);
		}

		private void Accel_ReadingChanged(object sender, AccelerometerReadingEventArgs e)
		{
			if (acc.State == SensorState.Ready)
			{
				tilt = new Vector3((float)e.X, (float)e.Y, (float)e.Z);
				if (Deployment.Current.Dispatcher.CheckAccess())
				{
					fireEventDelegate("tiltChange", TiltToStringArr());
				}
				else
				{
					Deployment.Current.Dispatcher.BeginInvoke(() => fireEventDelegate("tiltChange", TiltToStringArr()));
				}

				AccelerometerReadingEventArgs reading = e;
				try
				{
					if (reading != null)
					{
						if (!shaking && CheckForShake(lastReading, reading, ShakeThreshold) && shakeCount >= 1)
						{
							//We are shaking
							shaking = true;
							shakeCount = 0;
							if (Deployment.Current.Dispatcher.CheckAccess())
							{
								fireEventDelegate("shake", new string[] { });
							}
							else
							{
								Deployment.Current.Dispatcher.BeginInvoke(() => fireEventDelegate("shake", new string[] { }));
							}
						}
						else if (CheckForShake(lastReading, reading, ShakeThreshold))
						{
							shakeCount++;
						}
						else if (!CheckForShake(lastReading, reading, 0.2))
						{
							shakeCount = 0;
							shaking = false;
						}
					}
					lastReading = reading;
				}
				catch (System.Exception ex)
				{
					System.Diagnostics.Debug.WriteLine(ex.Message);
				}
			}
		}

		private static bool CheckForShake(AccelerometerReadingEventArgs last, AccelerometerReadingEventArgs current,
										double threshold)
		{
			double deltaX = 0.0d;
			double deltaY = 0.0d;
			double deltaZ = 0.0d;

			if (last != null && current != null)
			{
				deltaX = Math.Abs((last.X - current.X));
				deltaY = Math.Abs((last.Y - current.Y));
				deltaZ = Math.Abs((last.Z - current.Z));
			}

			return (deltaX > threshold && deltaY > threshold) ||
					(deltaX > threshold && deltaZ > threshold) ||
					(deltaY > threshold && deltaZ > threshold);
		}

		private void Start()
		{
			try
			{
				acc.Start();
			}
			catch (Exception /*e*/)
			{ }
		}
		#endregion
	}
}
