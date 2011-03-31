/****
 * © 2010-2011 mOcean Mobile. A subsidiary of Mojiva, Inc. All Rights Reserved.
 * */

using System;

namespace MojivaPhone
{
	public class StringEventArgs : EventArgs
	{
		private string value = null;

		public StringEventArgs(string value)
		{
			this.value = value;
		}

		public string Value
		{
			get { return value; }
		}
	}
}
