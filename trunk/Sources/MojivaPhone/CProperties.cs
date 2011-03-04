/****
 * © 2010-2011 mOcean Mobile. A subsidiary of Mojiva, Inc. All Rights Reserved.
 * */

namespace MojivaPhone
{
	internal class CProperties
	{
		public string transition;
		public string navigation;
		public string use_background;
		public string background_color;
		public double background_opacity;
		public bool is_modal;

		public CProperties()
		{
			Reset();
		}

		public CProperties(string jsonStr)
		{
			InitFromJsonStr(jsonStr);
		}

		private void Reset()
		{
			transition = "default";
			navigation = "none";
			use_background = "false";
			background_color = "";
			background_opacity = 1.0d;
			is_modal = false;
		}

		public void InitFromJsonStr(string jsonStr)
		{
			var ms = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonStr));
			var ser = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(CProperties));

			try
			{
				CProperties prop = ser.ReadObject(ms) as CProperties;
				this.transition			= prop.transition;
				this.navigation			= prop.navigation;
				this.use_background		= prop.use_background;
				this.background_color	= prop.background_color;
				this.is_modal			= prop.is_modal;
				this.background_opacity = prop.background_opacity;
			}
			catch (System.Exception /*ex*/)
			{
				this.Reset();
			}
			ms.Close();
		}

	}
}
