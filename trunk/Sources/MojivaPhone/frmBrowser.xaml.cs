/****
 * © 2010-2011 mOcean Mobile. A subsidiary of Mojiva, Inc. All Rights Reserved.
 * */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

namespace MojivaPhone
{
    public partial class frmBrowser : PhoneApplicationPage
    {
        protected String m_strUrl;

		public String Url
		{
			get { return m_strUrl; }
		}

        public frmBrowser()
        {
            InitializeComponent();
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (NavigationContext.QueryString["val"].Length > 0)
            {
				m_strUrl = NavigationContext.QueryString["val"];
				webBrowser.Navigate(new Uri(m_strUrl));
            }
        }

		private void PhoneApplicationPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
		{
		}
    }
}