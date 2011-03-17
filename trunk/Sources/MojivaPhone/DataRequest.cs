/****
 * © 2010-2011 mOcean Mobile. A subsidiary of Mojiva, Inc. All Rights Reserved.
 * */

using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Xml;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows;

namespace MojivaPhone
{
	class DataRequest
	{
        private static void httpRequest_GetResponseCallBack(IAsyncResult res)
        {
            HttpWebRequest req = (HttpWebRequest)res.AsyncState;
            if (res.IsCompleted)
            {
                HttpWebResponse result = (HttpWebResponse)req.EndGetResponse(res);
            }
        }

        public static String ReadStringData(String address)
        {
            String resultValue = "";

            
            try
            {
                HttpWebRequestSync req = (HttpWebRequestSync)HttpWebRequestSync.Create(address);
                req.Method = "GET";
                //req.ContentType = "text/xml; encoding='utf-8'";
                //req.UserAgent = "Mozilla";

                HttpWebResponse result = (HttpWebResponse)req.GetResponse();

                Stream ReceiveStream = result.GetResponseStream();
                System.Text.Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
                StreamReader sr = new StreamReader(ReceiveStream, encode);
                
                // Read the stream into arrays of 1024 characters
                Char[] read = new Char[1024];
                int count = sr.Read(read, 0, 1024);
                while (count > 0)
                {
                    String str = new String(read, 0, count);
                    resultValue += str;
                    count = sr.Read(read, 0, 1024);
                }
                result.Close();
            }
            catch (Exception)
            {
            }

            return resultValue;
        }

        public static String SendGetRequest(String address, String strRequest)
        {
            String resultValue = "";

			try
            {
                HttpWebRequestSync req = (HttpWebRequestSync)HttpWebRequestSync.Create(address);
                req.Method = "GET";
                //req.ContentType = "text/xml; encoding='utf-8'";
                //req.UserAgent = "Mozilla";

                Byte[] bytes = Encoding.UTF8.GetBytes(strRequest);
                req.ContentLength = bytes.Length;
                Stream SendStream = req.GetRequestStream();
                SendStream.Write(bytes, 0, bytes.Length);
                SendStream.Close();
            }
            catch (Exception /*ex*/)
            { }

            return resultValue;
        }

        public static byte[] ReadByteData(String address)
        {
            List<byte> bytez = new List<byte>();

            try
            {
                HttpWebRequestSync req = (HttpWebRequestSync)HttpWebRequestSync.Create(address);
                req.Method = "GET";
                //req.ContentType = "text/xml; encoding='utf-8'";
                //req.UserAgent = "Mozilla";


                HttpWebResponse result = (HttpWebResponse)req.GetResponse();

                Stream ReceiveStream = result.GetResponseStream();

                // Read the stream into arrays of 1024 characters
                byte[] read = new byte[8192];
                while (true)
                {
                    int count = ReceiveStream.Read(read, 0, 8192);
                    if (count == 0) break;

                    if (count == read.Length)
                        bytez.AddRange(read);
                    else
                    {
                        byte[] arr = new byte[count];
                        Array.Copy(read, arr, count);
                        bytez.AddRange(arr);
                    }
                }
                result.Close();
            }
            catch (Exception)
            {
            }

            return bytez.ToArray();
        }
	}
}
