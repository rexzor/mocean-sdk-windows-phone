/****
 * © 2010-2011 mOcean Mobile. A subsidiary of Mojiva, Inc. All Rights Reserved.
 * */

namespace MojivaPhone
{
	internal class CPosition
	{
		public int x;
		public int y;
		public int width;
		public int height;

		public CPosition()
		{
			Reset();
		}

		public CPosition(string jsonStr)
		{
			this.InitFromJsonStr(jsonStr);
		}

		private void Reset()
		{
			x = y = width = height = 0;
		}

		public void InitFromJsonStr(string jsonStr)
		{
			var ms = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonStr));
			var ser = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(CPosition));
			try
			{
				CPosition pos = ser.ReadObject(ms) as CPosition;
				this.x = pos.x;
				this.y = pos.y;
				this.width = pos.width;
				this.height = pos.height;
			}
			catch (System.Exception /*ex*/)
			{
				this.Reset();
			}
			ms.Close();
		}

	}
}
