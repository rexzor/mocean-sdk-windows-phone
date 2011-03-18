/****
 * © 2010-2011 mOcean Mobile. A subsidiary of Mojiva, Inc. All Rights Reserved.
 * */

using Microsoft.Xna.Framework;
namespace MojivaPhone
{
	public class AdInterstitialView : AdView
	{
		public AdInterstitialView(int site, int zone)
			: base(site, zone)
		{}

		public AdInterstitialView(int site, int zone, Game game)
			: base(site, zone, game)
		{}

		/// <summary>
		/// AdInterstitialView Locaion; (0, 0)
		/// </summary>
		public new Point Location
		{
			get { return Point.Zero; }
		}
	}
}
