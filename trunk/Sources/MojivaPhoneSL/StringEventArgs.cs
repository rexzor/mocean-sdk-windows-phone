/****
 * © 2010-2011 mOcean Mobile. A subsidiary of Mojiva, Inc. All Rights Reserved.
 * */

using System;

namespace MojivaPhone
{
	public class StringEventArgs : EventArgs
	{
		private string m_value;

		public StringEventArgs(string value)
		{
			m_value = value;
		}

		public string Value
		{
			get { return m_value; }
		}
	}
}
