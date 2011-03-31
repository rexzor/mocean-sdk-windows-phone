/****
 * © 2010-2011 mOcean Mobile. A subsidiary of Mojiva, Inc. All Rights Reserved.
 * */

using System;
using Microsoft.Devices.Sensors;
using Microsoft.Xna.Framework;

namespace MojivaPhone
{
	internal class CAccelerometer
	{
		#region "Variables"
		private Accelerometer acc = null;
		private Vector3 tilt;
		private int intensity, interval;
		private AccelerometerReadingEventArgs lastReading = null;
		private int shakeCount = 0;
		private bool shaking = false;
		private const double ShakeThreshold = 0.7;
		#endregion

		#region "Public methods"
		public CAccelerometer()
		{
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
			catch (System.Exception /*ex*/)
			{}
			finally
			{
				acc = null;
			}
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
				OnTiltChange(e);

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
							OnShake();
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
					System.Diagnostics.Debug.WriteLine("accelerometer: " + ex.Message);
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

		#region "Events"
		public delegate void TiltChangeDelegate(double x, double y, double z);
		private event TiltChangeDelegate TiltChangeEvent;
		public event TiltChangeDelegate TiltChange
		{
			add { TiltChangeEvent += value; }
			remove { TiltChangeEvent -= value; }
		}
		protected virtual void OnTiltChange(AccelerometerReadingEventArgs tilt)
		{
			if (TiltChangeEvent != null)
			{
				TiltChangeEvent(tilt.X, tilt.Y, tilt.Z);
			}
		}

		private event EventHandler ShakeEvent;
		public event EventHandler Shake
		{
			add { ShakeEvent += value; }
			remove { ShakeEvent -= value; }
		}
		protected virtual void OnShake()
		{
			if (ShakeEvent != null)
			{
				ShakeEvent(this, EventArgs.Empty);
			}
		}
		#endregion
	}
}
