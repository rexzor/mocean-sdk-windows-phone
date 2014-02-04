using System;
using System.Collections.Generic;
using System.Net;

namespace com.moceanmobile.mast
{
    public class AdTracking
    {
        public static void InvokeTrackingURL(string url, string userAgent)
        {
            WebRequest request = WebRequest.Create(url);
            
            if (request is HttpWebRequest)
            {
                HttpWebRequest httpRequest = (HttpWebRequest)request;

                httpRequest.Method = "GET";
                //httpRequest.Headers[HttpRequestHeader.UserAgent] = userAgent;

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

                    // TODO: Test redirects just to make sure they happen properly using the portable class library
                }
            }
            catch (Exception)
            {
                // Ignore anything for tracking.
            }
        }
    }
}
