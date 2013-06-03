using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.moceanmobile.mast
{
    /// <summary>
    /// This is a helper class to define an interstitial that automatically updates
    /// and presents itself on successfull reception.  Can be used from XAML but 
    /// the instance should be set to invisible as the interstitial instances are NOT
    /// able to handle interstitial content inline with other application content.
    /// 
    /// This stub is usefull for XAML implementations that just need a quick interstitial
    /// added without any need for background code.
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
