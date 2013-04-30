using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media.Animation;

namespace com.moceanmobile.mast.samples.Advanced
{
    public partial class Animation : PhoneApplicationPage
    {
        public Animation()
        {
            InitializeComponent();
        }

        private void adView_AdReceived(object sender, EventArgs e)
        {
            Storyboard storyboard = (Storyboard)this.FindName("adViewFadeIn");
            storyboard.Begin();
        }
    }
}