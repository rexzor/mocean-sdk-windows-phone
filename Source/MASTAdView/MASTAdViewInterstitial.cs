using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.moceanmobile.mast
{
    /// <summary>
    /// This is a helper class to define an interstitial that automatically updates
    /// and presents itself on successfull reception.
    /// </summary>
    public class MASTAdViewInterstitial : MASTAdView 
    {
        public MASTAdViewInterstitial()
            : base(true)
        {
            base.AdReceived += MASTAdViewInterstitial_AdReceived;
        }

        public void MASTAdViewInterstitial_AdReceived(object sender, EventArgs e)
        {
            base.ShowInterstitial();
        }
    }
}
