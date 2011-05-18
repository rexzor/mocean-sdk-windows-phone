/****
 * © 2010-2011 mOcean Mobile. A subsidiary of Mojiva, Inc. All Rights Reserved.
 * */

using System;
using Microsoft.Devices.Sensors;
using Microsoft.Xna.Framework;

namespace mOceanWindowsPhone
{
	internal class AccelerometerController
	{
		private const double TILT_CHANGE_DIFFERENCE = 0.05;

		#region "Variables"
		private Accelerometer acc = null;
		private double tiltX = 1.0;
		private double tiltY = 1.0;
		private double tiltZ = 1.0;
		private int intensity, interval;
		private AccelerometerReadingEventArgs lastReading = null;
		private int shakeCount = 0;
		private bool shaking = false;
		private const double ShakeThreshold = 0.1;

		private bool started = false;
		#endregion

		#region "Public methods"
		public AccelerometerController()
		{
			Init();
			Start();
		}

		public void SetProperties(int intensity, int interval)
		{
			this.intensity = intensity;
			this.interval = interval;
		}

		public void StartListen()
		{
			if (!started)
			{
				started = true;
				acc.ReadingChanged += new EventHandler<AccelerometerReadingEventArgs>(Accelerometer_ReadingChanged);
			}
		}

		public void StopListen()
		{
			acc.ReadingChanged -= Accelerometer_ReadingChanged;
			started = false;
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
			lastReading = null;
		}

		~AccelerometerController()
		{
			StopListen();
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
			intensity = interval = 0;
			SetProperties(intensity, interval);
		}

		private void Accelerometer_ReadingChanged(object sender, AccelerometerReadingEventArgs e)
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
				catch (System.Exception)
				{}
			}
		}

		private static bool CheckForShake(AccelerometerReadingEventArgs last, AccelerometerReadingEventArgs current, double threshold)
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
		public delegate void TiltChangeEventHandler(double x, double y, double z);
		private event TiltChangeEventHandler TiltChangeEvent;
		public event TiltChangeEventHandler TiltChange
		{
			add { TiltChangeEvent += value; }
			remove { TiltChangeEvent -= value; }
		}
		protected void OnTiltChange(AccelerometerReadingEventArgs tilt)
		{
			if (TiltChangeEvent != null)
			{
				if (Math.Abs(tilt.X - tiltX) >= TILT_CHANGE_DIFFERENCE ||
					Math.Abs(tilt.Y - tilt.Y) >= TILT_CHANGE_DIFFERENCE ||
					Math.Abs(tilt.Z - tiltZ) >= TILT_CHANGE_DIFFERENCE)
				{
					TiltChangeEvent(tilt.X, tilt.Y, tilt.Z);

					tiltX = tilt.X;
					tiltY = tilt.Y;
					tiltZ = tilt.Z;
				}
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
