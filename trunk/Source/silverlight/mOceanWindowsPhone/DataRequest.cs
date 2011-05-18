/****
 * © 2010-2011 mOcean Mobile. A subsidiary of Mojiva, Inc. All Rights Reserved.
 * */

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace mOceanWindowsPhone
{
	internal class DataRequest
	{
		public static bool USE_CACHE = true;
		public static int TIMEOUT = 30000;
		public static string TIMEOUT_MESSAGE = "timeout";
		private static int BLOCK_SIZE = 1024;

		private static Dictionary<string, int> resourceRequestCount = new Dictionary<string, int>();

		public class StringDataEventArgs : EventArgs
		{
			public bool IsError = false;
			public string Data = null;
			public StringDataEventArgs(bool isError, string data)
			{
				IsError = isError;
				Data = data;
			}
		}

		private class RequestCallback
		{
			public object Result = null;
			public EventHandler<StringDataEventArgs> DoneEvent = null;
			public RequestCallback(object result, EventHandler<StringDataEventArgs> doneEvent)
			{
				this.Result = result;
				this.DoneEvent = doneEvent;
			}
		}

		private class ResponseStreamCallback
		{
			public object Result = null;
			public string Data = null;
			public ResponseStreamCallback(object result, string data)
			{
				Result = result;
				Data = data;
			}
		}

		public static void RequestStringData(string address, EventHandler<StringDataEventArgs> e)
		{
			if (String.IsNullOrEmpty(address))
			{
				OnRequestStringDataDone(e, true, "null address");
				return;
			}

			HttpWebRequest request = HttpWebRequest.Create(address) as HttpWebRequest;
			if (request == null)
			{
				OnRequestStringDataDone(e, true, "address not valid");
				return;
			}

			request.Accept = "text/html";

			RequestCallback requestCallback = new RequestCallback(request, e);
			Timer abortTimer = new Timer(new TimerCallback(AbortTimerTick), requestCallback, TIMEOUT, Timeout.Infinite);
			request.BeginGetResponse(ResponseToRequest, requestCallback);
		}

		private static void ResponseToRequest(IAsyncResult result)
		{
			RequestCallback requestCallback = (RequestCallback)result.AsyncState;

			var request = requestCallback.Result as HttpWebRequest;
			if (request == null)
			{
				OnRequestStringDataDone(requestCallback.DoneEvent, true, "no response");
				return;
			}

			try
			{
				var response = request.EndGetResponse(result);

				Stream stream = response.GetResponseStream();
				System.Text.Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
				StreamReader reader = new StreamReader(stream, encode);

				string data = "";
				char[] read = new char[BLOCK_SIZE];
				int count = reader.Read(read, 0, BLOCK_SIZE);
				while (count > 0)
				{
					string str = new string(read, 0, count);
					data += str;
					count = reader.Read(read, 0, BLOCK_SIZE);
				}
				reader.Close();
				response.Close();

				OnRequestStringDataDone(requestCallback.DoneEvent, false, data);
			}
			catch (System.Exception)
			{
				OnRequestStringDataDone(requestCallback.DoneEvent, true, "bad response");
			}
		}

		private static void AbortTimerTick(object stateInfo)
		{
			RequestCallback requestCallback = (RequestCallback)stateInfo;

			var request = requestCallback.Result as HttpWebRequest;
			if (request != null)
			{
				request.Abort();
				OnRequestStringDataDone(requestCallback.DoneEvent, true, TIMEOUT_MESSAGE);
			}
		}

		private static void OnRequestStringDataDone(EventHandler<StringDataEventArgs> e, bool isError, string data)
		{
			if (e != null)
			{
				e(null, new StringDataEventArgs(isError, data));
			}
		}



		public static void SendGetRequest(string address, string strRequest)
		{
			HttpWebRequest request = HttpWebRequest.Create(address) as HttpWebRequest;

			request.AllowReadStreamBuffering = false;
			request.Method = "POST";
			ResponseStreamCallback responseStreamCallback = new ResponseStreamCallback(request, strRequest);
			request.BeginGetRequestStream(new AsyncCallback(GetRequestStreamCallback), responseStreamCallback);
		}

		private static void GetRequestStreamCallback(IAsyncResult asynchronousResult)
		{
			ResponseStreamCallback responseStreamCallback = (ResponseStreamCallback)asynchronousResult.AsyncState;
			HttpWebRequest request = (HttpWebRequest)responseStreamCallback.Result;

			Stream SendStream = request.EndGetRequestStream(asynchronousResult);
			byte[] bytes = Encoding.UTF8.GetBytes(responseStreamCallback.Data);
			SendStream.Write(bytes, 0, bytes.Length);
			SendStream.Close();
		}





		public static string RequestStringDataSync(string address)
		{
			if (String.IsNullOrEmpty(address))
			{
				return null;
			}

			HttpWebRequest request = HttpWebRequest.Create(address) as HttpWebRequest;
			if (request == null)
			{
				return null;
			}

			string requestedData = String.Empty;

			ManualResetEvent requestDone = new ManualResetEvent(false);

			request.Accept = "text/html";
			request.BeginGetResponse((ar) =>
			{
				HttpWebResponse response = null;
				Stream stream = null;
				StreamReader reader = null;

				try
				{
					response = (HttpWebResponse)request.EndGetResponse(ar);

					stream = response.GetResponseStream();
					Encoding encode = Encoding.GetEncoding("utf-8");

					reader = new StreamReader(stream, encode);

					char[] read = new char[BLOCK_SIZE];
					int count = reader.Read(read, 0, BLOCK_SIZE);

					while (count > 0)
					{
						string str = new string(read, 0, count);
						requestedData += str;
						count = reader.Read(read, 0, BLOCK_SIZE);
					}
				}
				catch (System.Exception)
				{
					return;
				}
				finally
				{
					if (response != null)
					{
						response.Close();
						response = null;
					}

					if (reader != null)
					{
						reader.Close();
						reader = null;
					}

					if (stream != null)
					{
						stream.Close();
						stream = null;
					}
				}

				requestDone.Set();
			}, request);

			if (!requestDone.WaitOne(TIMEOUT))
			{
				request.Abort();
				requestedData = TIMEOUT_MESSAGE;
			}

			requestDone.Close();
			request = null;

			if (!String.IsNullOrEmpty(requestedData))
			{
				return requestedData;
			}

			return null;
		}

		public static byte[] RequestByteDataSync(string address)
		{
			if (String.IsNullOrEmpty(address))
			{
				return null;
			}

			HttpWebRequest request = HttpWebRequest.Create(address) as HttpWebRequest;
			if (request == null)
			{
				return null;
			}

			ManualResetEvent requestDone = new ManualResetEvent(false);

			List<byte> requestedData = new List<byte>();

			request.BeginGetResponse((ar) =>
			{
				HttpWebResponse response = null;
				Stream stream = null;

				try
				{
					response = (HttpWebResponse)request.EndGetResponse(ar);
					stream = response.GetResponseStream();


					// Read the stream into arrays of 1024 characters
					byte[] read = new byte[BLOCK_SIZE];
					while (true)
					{
						int count = stream.Read(read, 0, BLOCK_SIZE);
						if (count == 0) break;

						if (count == read.Length)
						{
							requestedData.AddRange(read);
						}
						else
						{
							byte[] arr = new byte[count];
							Array.Copy(read, arr, count);
							requestedData.AddRange(arr);
						}
					}
				}
				catch (System.Exception)
				{
					return;
				}
				finally
				{
					if (response != null)
					{
						response.Close();
						response = null;
					}

					if (stream != null)
					{
						stream.Close();
						stream = null;
					}
				}

				requestDone.Set();
			}, request);

			if (!requestDone.WaitOne(TIMEOUT))
			{
				request.Abort();
				requestedData = null;
			}

			requestDone.Close();
			request = null;

			if (requestedData != null && requestedData.Count > 0)
			{
				return requestedData.ToArray();
			}

			return null;
		}

		private class Resource
		{
			public enum TYPE { IMAGE, VIDEO };
			public TYPE ResType { get; private set; }
			public string Source { get; private set; }
			public Resource(TYPE type, string source)
			{
				ResType = type;
				Source = source;
			}
		}

		private static List<Resource> FindResources(string adContent)
		{
			List<Resource> resources = new List<Resource>();

			string adPattern = "<(img|video).*?src=[\",\'](.*?)[\",\'].*?>";
			Regex adRegex = new Regex(adPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
			MatchCollection adMatches = adRegex.Matches(adContent);
			foreach (Match currMatch in adMatches)
			{
				string tag = currMatch.Groups[1].Value;
				string src = currMatch.Groups[2].Value;
				if (!String.IsNullOrEmpty(src))
				{
					if (tag == "img")
					{
						resources.Add(new Resource(Resource.TYPE.IMAGE, src));
					}
					else if (tag == "video")
					{
						resources.Add(new Resource(Resource.TYPE.VIDEO, src));
					}
				}
			}

			if (resources.Count > 0)
			{
				return resources;
			}

			return null;
		}

		private static string SaveResource(string absoluteUri)
		{
			try
			{
				using (IsolatedStorageFile appStorage = IsolatedStorageFile.GetUserStoreForApplication())
				{
					string localUri = absoluteUri.Substring(absoluteUri.IndexOf('/', ("http://").Length) + 1);
					if (appStorage.FileExists(localUri))
					{
						if (resourceRequestCount.ContainsKey(localUri))
						{
							resourceRequestCount[localUri]++;
						}
						else
						{
							resourceRequestCount.Add(localUri, 1);
						}
					}
					else
					{
						string[] fileDirs = localUri.Split('/');
						string currDir = "";
						for (int i = 0; i < fileDirs.Length - 1; i++)
						{
							currDir += fileDirs[i];
							if (!appStorage.DirectoryExists(currDir))
							{
								appStorage.CreateDirectory(currDir);
							}
							currDir += "/";
						}

						byte[] data = RequestByteDataSync(absoluteUri);

						if (FreeIsolatedStorage(data.Length))
						{
							using (IsolatedStorageFileStream file = appStorage.OpenFile(localUri, FileMode.Create))
							{
								file.Write(data, 0, data.Length);
							}

							if (resourceRequestCount.ContainsKey(localUri))
							{
								resourceRequestCount.Remove(localUri);
							}

							resourceRequestCount.Add(localUri, 1);
						}
						else
						{
							return absoluteUri;
						}
					}

					return localUri;
				}
			}
			catch (System.Exception)
			{}

			return absoluteUri;
		}

		private static bool FreeIsolatedStorage(int needSpace)
		{
			bool available = false;

			try
			{
				using (IsolatedStorageFile appStorage = IsolatedStorageFile.GetUserStoreForApplication())
				{
					if (appStorage.AvailableFreeSpace >= needSpace)
					{
						return true;
					}

					while (true)
					{
						string minRequestFileName = null;
						int minRequestCount = 0;

						// find most infrequently requested file
						foreach (string fileName in resourceRequestCount.Keys)
						{
							if (minRequestCount == 0)
							{
								minRequestCount = resourceRequestCount[fileName];
								minRequestFileName = fileName;
							}
							else
							{
								if (resourceRequestCount[fileName] < minRequestCount)
								{
									minRequestFileName = fileName;
									minRequestCount = resourceRequestCount[fileName];
								}
							}
						}

						// if finded delete it
						if (minRequestCount > 0 && !String.IsNullOrEmpty(minRequestFileName))
						{
							try
							{
								appStorage.DeleteFile(minRequestFileName);
								resourceRequestCount.Remove(minRequestFileName);

								if (appStorage.AvailableFreeSpace >= needSpace)
								{
									available = true;
									break;
								}
								else if (resourceRequestCount.Count == 0)
								{
									available = false;
									break;
								}
								else
								{
									continue; // find and delete next file
								}
							}
							catch (System.Exception)
							{ }
						}
						else
						{
							available = false;
							break;
						}
					}
				}
			}
			catch (System.Exception)
			{}

			return available;
		}

		private static string FindHrefInVideoAd(string adContent)
		{
			string adPattern = "<a.*?href=[\",\'](.*?)[\",\'].*?>";
			Regex adRegex = new Regex(adPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
			MatchCollection adMatches = adRegex.Matches(adContent);
			foreach (Match currMatch in adMatches)
			{
				string href = currMatch.Groups[1].Value;
				return href;
			}

			return null;
		}



		public enum AD_TYPE { SIMPLE, VIDEO, ERROR }

		public class AdEventArgs : EventArgs
		{
			public AD_TYPE Type = AD_TYPE.SIMPLE;
			public string Content = null;
			public string VideoSrc = null;
			public string VideoHref = null;
			public int AdRequestId = -1;
			public AdEventArgs() { }
		}

		public static HttpWebRequest RequestAdAsync(string address, EventHandler<AdEventArgs> doneEvent, int requestId)
		{
			if (String.IsNullOrEmpty(address))
			{
				return null;
			}

			HttpWebRequest request = HttpWebRequest.Create(address) as HttpWebRequest;
			if (request == null)
			{
				return null;
			}

			request.Accept = "text/html";
			IAsyncResult result = request.BeginGetResponse((ar) =>
			{
				try
				{
					HttpWebResponse response = null;
					Stream stream = null;
					StreamReader reader = null;

					AdEventArgs requestDoneArgs = new AdEventArgs();
					requestDoneArgs.AdRequestId = requestId;

					string requestedAd = String.Empty;

					try
					{
						response = (HttpWebResponse)request.EndGetResponse(ar);

						stream = response.GetResponseStream();
						reader = new StreamReader(stream, Encoding.GetEncoding("utf-8"));

						char[] read = new char[BLOCK_SIZE];
						int count = reader.Read(read, 0, BLOCK_SIZE);

						while (count > 0)
						{
							string str = new string(read, 0, count);
							requestedAd += str;
							count = reader.Read(read, 0, BLOCK_SIZE);
						}
					}
					catch (System.Exception ex)
					{
						requestDoneArgs.Type = AD_TYPE.ERROR;
						requestDoneArgs.Content = ex.Message;
						return;
					}
					finally
					{
						if (response != null)
						{
							response.Close();
							response = null;
						}

						if (reader != null)
						{
							reader.Close();
							reader = null;
						}

						if (stream != null)
						{
							stream.Close();
							stream = null;
						}
					}

					if (!String.IsNullOrEmpty(requestedAd))
					{
						if (USE_CACHE)
						{
							List<Resource> resources = FindResources(requestedAd);
							if (resources != null)
							{
								for (int i = 0; i < resources.Count; i++)
								{
									string localUri = SaveResource(resources[i].Source);

									if (!String.IsNullOrEmpty(localUri))
									{
										requestedAd = requestedAd.Replace(resources[i].Source, localUri);
									}

									if (resources[i].ResType == Resource.TYPE.VIDEO)
									{
										requestDoneArgs.Type = AD_TYPE.VIDEO;
										//requestDoneArgs.VideoSrc = resources[i].Source;
										requestDoneArgs.VideoSrc = localUri;
										requestDoneArgs.VideoHref = FindHrefInVideoAd(requestedAd);

										break;
									}
								}
							}
						}

						requestDoneArgs.Content = requestedAd;
						if (doneEvent != null)
						{
							doneEvent(null, requestDoneArgs);
							doneEvent = null;
						}
					}
					else
					{
						requestDoneArgs.Type = AD_TYPE.ERROR;
						requestDoneArgs.Content = String.Empty;
						if (doneEvent != null)
						{
							doneEvent(null, requestDoneArgs);
							doneEvent = null;
						}
					}
				}
				catch (System.Net.WebException)
				{}
			}, request);


			ThreadPool.RegisterWaitForSingleObject(new AutoResetEvent(false), (state, timedOut) =>
				{
					if (timedOut)
					{
						try
						{
							request.Abort();
							if (doneEvent != null)
							{
								doneEvent(null, new AdEventArgs() { AdRequestId = requestId, Content = TIMEOUT_MESSAGE, Type = AD_TYPE.ERROR });
								doneEvent = null;
							}
						}
						catch (System.Exception)
						{}
					}
				},
				result.AsyncState, TIMEOUT, true);

			return request;
		}
	}
}