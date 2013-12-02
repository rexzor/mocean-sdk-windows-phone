using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace com.moceanmobile.mast
{
    internal class AdTracking
    {
        public static void InvokeTrackingURL(string url, string userAgent)
        {
            WebRequest request = WebRequest.Create(url);
            
            if (request is HttpWebRequest)
            {
                HttpWebRequest httpRequest = (HttpWebRequest)request;

                httpRequest.Method = "GET";
                httpRequest.UserAgent = userAgent;
                httpRequest.AllowAutoRedirect = true;

                httpRequest.BeginGetResponse(new AsyncCallback(RequestCallback), httpRequest);
            }
        }

        public static void RequestCallback(IAsyncResult ar)
        {
            try
            {
                HttpWebRequest httpRequest = ar.AsyncState as HttpWebRequest;

                if (httpRequest != null)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.EndGetResponse(ar);
                    httpResponse.Close();
                }
            }
            catch (Exception)
            {
                // Ignore anything for tracking.
            }
        }
    }
}
