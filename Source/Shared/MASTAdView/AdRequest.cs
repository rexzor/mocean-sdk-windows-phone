using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.moceanmobile.mast
{
    // invoked to inform the caller that the request failed due to connectivity, dns, timout, etc...
    internal delegate void AdRequestFailed(AdRequest request, Exception exception);

    // invoked to inform the caller that the request failed due to an error response from the ad network
    internal delegate void AdRequestError(AdRequest request, string errorCode, string errorMessage);

    // invoked to inform the caller that the request has obtained an ad descriptor from the ad network
    internal delegate void AdRequestCompleted(AdRequest request, AdDescriptor adDescriptor);

    /// <summary>
    /// Represents a one time ad request.  Use the Create method to create and start the ad request.
    /// Once the returned request triggers a callback or is canceled it becomes invalid.
    /// 
    /// The request and response parsing follow the Mocean Ad Server interface found on the following page:
    /// http://developer.moceanmobile.com/Mocean_Ad_Request_API
    /// </summary>
    internal class AdRequest : IDisposable
    {
        // creates and starts ad request
        public static AdRequest Create(int timeout, string adServerURL, string userAgent, Dictionary<String, String> parameters,
            AdRequestCompleted completedCallback, AdRequestError errorCallback, AdRequestFailed failedCallback)
        {
            AdRequest request = new AdRequest(timeout, adServerURL, userAgent, parameters, completedCallback, errorCallback, failedCallback);

            request.Start();
            
            return request;
        }

        private int timeout;
        private string requestURL;
        private string userAgent;
        private AdRequestCompleted completedCallback;
        private AdRequestError errorCallback;
        private AdRequestFailed failedCallback;
        
        private System.Threading.Timer timeoutTimer;
        private System.Net.HttpWebRequest webRequest = null;
        
        private AdRequest(int timeout, string adServerURL, string userAgent, Dictionary<String, String> parameters,
            AdRequestCompleted completedCallback, AdRequestError errorCallback, AdRequestFailed failedCallback)
        {
            this.userAgent = userAgent;
            this.timeout = timeout;
            this.completedCallback = completedCallback;
            this.errorCallback = errorCallback;
            this.failedCallback = failedCallback;

            string requestURL = adServerURL + "?";
            foreach (KeyValuePair<String, String> param in parameters)
            {
                requestURL += System.Net.HttpUtility.UrlEncode(param.Key) + "=" +
                    System.Net.HttpUtility.UrlEncode(param.Value) + "&";
            }
            requestURL = requestURL.Substring(0, requestURL.Length - 1);

            this.requestURL = requestURL;
        }



        public string URL
        {
            get { return this.requestURL; }
        }

        public void Cancel()
        {
            this.completedCallback = null;
            this.errorCallback = null;
            this.failedCallback = null;

            if (this.timeoutTimer != null)
            {
                this.timeoutTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                this.timeoutTimer = null;
            }

            if (this.webRequest != null)
            {
                this.webRequest.Abort();
                this.webRequest = null;
            }
        }

        private void Start()
        {
            try
            {
                System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(OnRequestWork), null);
            }
            catch (Exception ex)
            {
                if (this.failedCallback != null)
                {
                    this.failedCallback(this, ex);
                }

                Cancel();
            }
        }

        // invoked from the thread pool
        private void OnRequestWork(Object state)
        {
            try
            {
                this.timeoutTimer = new System.Threading.Timer(new System.Threading.TimerCallback(OnTimeout),
                    null, this.timeout, System.Threading.Timeout.Infinite);

                this.webRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(this.requestURL);
                this.webRequest.UserAgent = this.userAgent;
                
                IAsyncResult result = this.webRequest.BeginGetResponse(new AsyncCallback(OnRequestResponse), null);
            }
            catch (Exception ex)
            {
                if (this.failedCallback != null)
                {
                    this.failedCallback(this, ex);
                }

                Cancel();
            }
        }

        // invoke on request timeout
        private void OnTimeout(Object state)
        {
            if (this.webRequest != null)
            {
                this.webRequest.Abort();
                this.webRequest = null;
            }

            // TODO: Send failure callback with reason
            if (this.failedCallback != null)
            {
                this.failedCallback(this, new TimeoutException());
            }

            Cancel();
        }

        // invoked on response
        private void OnRequestResponse(IAsyncResult ar)
        {
            if (this.timeoutTimer != null)
                this.timeoutTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            
            try
            {
                System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)this.webRequest.EndGetResponse(ar);

                System.IO.Stream responseStream = response.GetResponseStream();

                System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(OnParseWork), responseStream);
            }
            catch (Exception ex)
            {
                if (this.failedCallback != null)
                {
                    this.failedCallback(this, ex);
                }

                Cancel();
            }
        }

        private void OnParseWork(Object state)
        {
            try
            {
                AdDescriptor adDescriptor = null;

                System.IO.Stream stream = state as System.IO.Stream;

                System.Xml.XmlReaderSettings readerSettings = new System.Xml.XmlReaderSettings();
                readerSettings.IgnoreWhitespace = true;
                readerSettings.IgnoreComments = true;
                readerSettings.IgnoreProcessingInstructions = true;

                System.Xml.XmlReader reader = System.Xml.XmlReader.Create(stream, readerSettings);

                Dictionary<string, string> adInfo = new Dictionary<string, string>();
                while (reader.Read())
                {
                    if (reader.IsStartElement())
                    {
                        if (reader.Name == "error")
                        {
                            string errorCode = reader.GetAttribute("code");
                            string errorMessage = string.Empty;

                            // read past the name
                            reader.Read();

                            // read the contents
                            switch (reader.NodeType)
                            {
                                case System.Xml.XmlNodeType.CDATA:
                                case System.Xml.XmlNodeType.Text:
                                    errorMessage = reader.ReadContentAsString();
                                    break;
                            }

                            if (this.errorCallback != null)
                            {
                                this.errorCallback(this, errorCode, errorMessage);

                                Cancel();
                            }

                            // no need to parse anything else
                            break;
                        }
                        else if (reader.Name == "ad")
                        {
                            string adType = reader.GetAttribute("type");

                            adInfo["type"] = adType;

                            // read each child node (passing ad node first)
                            while (reader.Read())
                            {
                                if (reader.IsStartElement() == false)
                                {
                                    if ((reader.NodeType == System.Xml.XmlNodeType.EndElement) && (reader.Name == "ad"))
                                    {
                                        // done with the descriptor
                                        break;
                                    }

                                    // advance to start of next descriptor property
                                    continue;
                                }

                                string name = reader.Name;

                                // read past the name
                                reader.Read();

                                // read the content which may be text or cdata from the ad server
                                string value = null;
                                switch (reader.NodeType)
                                {
                                    case System.Xml.XmlNodeType.CDATA:
                                    case System.Xml.XmlNodeType.Text:
                                        value = reader.ReadContentAsString();
                                        break;
                                }

                                if ((string.IsNullOrEmpty(name) == false) && (string.IsNullOrEmpty(value) == false))
                                {
                                    adInfo[name] = value;
                                }
                            }

                            adDescriptor = new AdDescriptor(adInfo);
                            
                            // no need to parse anything else
                            break;
                        }
                    }
                }
                
                if (this.completedCallback != null)
                {
                    this.completedCallback(this, adDescriptor);
                }
            }
            catch (Exception ex)
            {
                if (this.failedCallback != null)
                {
                    this.failedCallback(this, ex);
                }
            }

            Cancel();
        }

        public void Dispose()
        {
            if (this.timeoutTimer != null)
            {
                this.timeoutTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                this.timeoutTimer.Dispose();
                this.timeoutTimer = null;
            }
        }
    }
}
