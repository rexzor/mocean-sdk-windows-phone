/****
 * © 2010-2011 mOcean Mobile. A subsidiary of Mojiva, Inc. All Rights Reserved.
 * */

using System;
using System.Windows;
using Microsoft.Phone.Controls;
using System.IO.IsolatedStorage;

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

		private void PhoneApplicationPage_Unloaded(object sender, RoutedEventArgs e)
		{
		}
    }
}