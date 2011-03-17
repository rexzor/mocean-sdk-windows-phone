/****
 * © 2010-2011 mOcean Mobile. A subsidiary of Mojiva, Inc. All Rights Reserved.
 * */

using System;
using System.Net;
using System.Threading;
using System.IO;

namespace MojivaPhone
{
    public class HttpWebRequestSync
    {
        private ManualResetEvent getRequestDone = new ManualResetEvent(false);
        private ManualResetEvent getResponseDone = new ManualResetEvent(false);
        private HttpWebRequest request;

        private HttpWebResponse response;
        private Stream requestStream;

        /// <summary>
        /// Private ctor, to get an HttpWebRequestSync you shall use Create()
        /// </summary>
        private HttpWebRequestSync()
        {

        }

        /// <summary>
        /// Initializes a new HttpWebRequestSync for the specified URI scheme
        /// </summary>
        public static HttpWebRequestSync Create(string url)
        {
            return Create(new Uri(url));
        }

        /// <summary>
        /// Initializes a new HttpWebRequestSync for the specified URI scheme
        /// </summary>
        public static HttpWebRequestSync Create(Uri uri)
        {
            HttpWebRequestSync wrs = new HttpWebRequestSync();
            wrs.request = (HttpWebRequest)WebRequest.Create(uri);

            return wrs;
        }

        #region HttpWebRequest Interface
        /// <summary>
        /// Summary:
        ///     Gets or sets the value of the Accept HTTP header.
        ///
        /// Returns:
        ///     The value of the Accept HTTP header. The default value is null.
        /// </summary>
        public string Accept
        {
            get
            {
                return request.Accept;
            }
            set
            {
                request.Accept = value;
            }
        }
        /// <summary>
        /// Not Implemented Yet
        /// </summary>
        public int ContentLength
        {
            get
            {
                return 0;
            }
            set
            {
                return;
            }
        }
        /// <summary>
        /// Summary:
        ///     Gets or sets the value of the Content-type HTTP header.
        ///
        /// Returns:
        ///     The value of the Content-type HTTP header. The default value is null.
        /// </summary>
        public string ContentType
        {
            get
            {
                return request.ContentType;
            }
            set
            {
                request.ContentType = value;
            }
        }
        /// <summary>
        /// Summary:
        ///     Specifies the collection of System.Net.CookieCollection objects associated
        ///     with the HTTP request.
        ///
        /// Returns:
        ///     A System.Net.CookieContainer that contains a collection of System.Net.CookieCollection
        ///     objects associated with the HTTP request.
        ///
        /// Exceptions:
        ///   System.NotImplementedException:
        ///     This property is not implemented.
        /// </summary>
        public CookieContainer CookieContainer
        {
            get
            {
                return request.CookieContainer;
            }
            set
            {
                request.CookieContainer = value;
            }
        }
        /// <summary>
        /// Summary:
        ///     Gets a value that indicates whether a response has been received from an
        ///     Internet resource.
        ///
        /// Returns:
        ///     true if a response has been received; otherwise, false.
        ///
        /// Exceptions:
        ///   System.NotImplementedException:
        ///     This property is not implemented.
        /// </summary>
        public bool HaveResponse
        {
            get
            {
                return request.HaveResponse;
            }
        }
        /// <summary>
        /// Returns the underlying HttpWebRequest (handle it with care)
        /// </summary>
        public HttpWebRequest HttpWebRequest
        {
            get
            {
                return request;
            }
        }
        /// <summary>
        /// Summary:
        ///     Specifies a collection of the name/value pairs that make up the HTTP headers.
        ///
        /// Returns:
        ///     A System.Net.WebHeaderCollection that contains the name/value pairs that
        ///     make up the headers for the HTTP request.
        ///
        /// Exceptions:
        ///   System.InvalidOperationException:
        ///     The request has been started by calling the System.Net.HttpWebRequest.BeginGetRequestStream
        /// 
        /// (System.AsyncCallback,System.Object)
        ///     or System.Net.HttpWebRequest.BeginGetResponse(System.AsyncCallback,System.Object)
        ///     method.
        /// </summary>
        public WebHeaderCollection Headers
        {
            get
            {
                return request.Headers;
            }
            set
            {
                request.Headers = value;
            }
        }
        /// <summary>
        /// Summary:
        ///     Gets or sets the method for the request.
        ///
        /// Returns:
        ///     The request method to use to contact the Internet resource. The default value
        ///     is GET.
        ///
        /// Exceptions:
        ///   System.ArgumentException:
        ///     No method is supplied.-or- The method string contains invalid characters.
        ///
        ///   System.NotImplementedException:
        ///     This property is not implemented.
        ///
        ///   System.NotSupportedException:
        ///     The System.Net.HttpWebRequest.Method property is not GET or POST.
        /// </summary>
        public string Method
        {
            get
            {
                return request.Method;
            }
            set
            {
                request.Method = value;
            }
        }
        /// <summary>
        /// Summary:
        ///     Gets the original Uniform Resource Identifier (URI) of the request.
        ///
        /// Returns:
        ///     A System.Uri that contains the URI of the Internet resource passed to the
        ///     System.Net.WebRequest.Create(System.Uri) method.
        ///
        /// Exceptions:
        ///   System.NotImplementedException:
        ///     This property is not implemented.
        /// </summary>
        public Uri RequestUri
        {
            get
            {
                return request.RequestUri;
            }
        }
        #endregion

        public Stream GetRequestStream()
        {
            // start the asynchronous operation
            request.AllowReadStreamBuffering = false;
            request.BeginGetRequestStream(new AsyncCallback(GetRequestStreamCallback), request);

            // Keep the main thread from continuing while the asynchronous
            // operation completes. A real world application
            // could do something useful such as updating its user interface.
            getRequestDone.WaitOne();

            Stream result = requestStream;

            // Avoid keeping an unnecessary ref on the stream in this object
            requestStream = null;

            return result;
        }

        private void GetRequestStreamCallback(IAsyncResult asynchronousResult)
        {
            request = (HttpWebRequest)asynchronousResult.AsyncState;
            requestStream = request.EndGetRequestStream(asynchronousResult);

            getRequestDone.Set();
        }

        public HttpWebResponse GetResponse()
        {
            HttpWebResponse result = null;
            try
            {
                // Start the asynchronous operation to get the response
                //request.BeginGetResponse(new AsyncCallback(GetResponseCallback), request);
				request.BeginGetResponse(GetResponseCallback, request);

                getResponseDone.WaitOne();

                result = response;

                // Avoid keeping an unnecessary ref on the response in this object
                response = null;
            }
            catch(Exception)
            {
            }

// 			for (int i = 0; i < result.Headers.AllKeys.Length; i++)
// 			{
// 				System.Diagnostics.Debug.WriteLine("header:" + result.Headers.AllKeys[i]);
// 			}

            return result;
        }

        private void GetResponseCallback(IAsyncResult asynchronousResult)
        {
            try
            {
                request = (HttpWebRequest)asynchronousResult.AsyncState;

                // End the operation
                response = (HttpWebResponse)request.EndGetResponse(asynchronousResult);

                getResponseDone.Set();
            }
            catch(Exception)
            {
                getResponseDone.Set();
            }
        }
    }
}