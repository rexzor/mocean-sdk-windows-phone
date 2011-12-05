/****
 * © 2010-2011 mOcean Mobile. A subsidiary of Mojiva, Inc. All Rights Reserved.
 * */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace mOceanWindowsPhone
{
	internal interface IDataRequestListener
	{
		void OnRegisterResponse(string response);
		void OnAdResponse(string adContent);
		void OnAdVideoResponse(string adContent, string src, string href);
		void OnOrmmaResponse(string url, string response);
	}

	internal class DataRequest
	{
		public static string RootDir { get; set; }
		private static int BLOCK_SIZE = 1024;
		private static Encoding AD_CONTENT_ENCODE = Encoding.UTF8;

		private static Dictionary<IDataRequestListener, HttpWebRequest> adRequests = new Dictionary<IDataRequestListener, HttpWebRequest>();
		private static Dictionary<IDataRequestListener, Dictionary<string, HttpWebRequest>> adOrmmaRequests = new Dictionary<IDataRequestListener, Dictionary<string, HttpWebRequest>>();

		private static object adRequestsLocker = new object();

		private static IDataRequestListener registrator = null;

		public static void RegisterAd(IDataRequestListener listener, string uri)
		{
			if (registrator == null)
			{
				registrator = listener;

				HttpWebRequest request = HttpWebRequest.Create(uri) as HttpWebRequest;
				if (request != null)
				{
					request.Accept = "text/html";
					request.BeginGetResponse(new AsyncCallback(RegisterAdCallback), request);
				}
			}
		}

		private static void RegisterAdCallback(IAsyncResult result)
		{
			try
			{
				HttpWebRequest request = result.AsyncState as HttpWebRequest;
				if (request != null)
				{
					HttpWebResponse response = request.EndGetResponse(result) as HttpWebResponse;

					string registerResponse = GetStringAd(response);
					response.Close();

					if (registrator != null)
					{
						registrator.OnRegisterResponse(registerResponse);
					}
				}
			}
			catch (Exception)
			{ }
		}

		public static void RequestAd(IDataRequestListener listener, string uri)
		{
			lock (adRequestsLocker)
			{
				HttpWebRequest request = null;
				if (listener != null)
				{
					bool isRequested = adRequests.TryGetValue(listener, out request);
					if (isRequested)
					{
						request.Abort();
						adRequests.Remove(listener);
					}
				}

				request = HttpWebRequest.Create(uri) as HttpWebRequest;
				if (request != null)
				{
					if (listener != null)
					{
						adRequests.Add(listener, request);
					}

					request.Accept = "text/html";
					request.BeginGetResponse(new AsyncCallback(RequestAdCallback), request);
					Debug.WriteLine("BeginGetResponse");
				}
			}
		}

		public static void CancelRequestAd(IDataRequestListener listener)
		{
			lock (adRequestsLocker)
			{
				HttpWebRequest request = null;
				bool isRequested = adRequests.TryGetValue(listener, out request);
				if (isRequested)
				{
					request.Abort();
					adRequests.Remove(listener);
				}
			}
		}

		private static void RequestAdCallback(IAsyncResult result)
		{
			string adContent = null;
			IDataRequestListener listener = null;
			HttpWebRequest request = result.AsyncState as HttpWebRequest;

			if (request != null)
			{
				lock (adRequestsLocker)
				{
					foreach (KeyValuePair<IDataRequestListener, HttpWebRequest> listenerRequestPair in adRequests)
					{
						if (listenerRequestPair.Value == request)
						{
							listener = listenerRequestPair.Key;
							break;
						}
					}

					if (listener != null)
					{
						adRequests.Remove(listener);
					}
				}

				try
				{
					HttpWebResponse response = request.EndGetResponse(result) as HttpWebResponse;
					adContent = GetStringAd(response);
					response.Close();
				}
				catch (WebException we)
				{
					Debug.WriteLine(we.InnerException.Message);
					adContent = null;
				}
				catch (Exception)
				{
					adContent = null;
				}
			}

			if (listener != null)
			{
				Resource videoContent = null;
				if (adContent != null)
				{
					adContent = CheckAdForMedia(adContent, out videoContent);
				}

				if (videoContent != null)
				{
					listener.OnAdVideoResponse(adContent, videoContent.Source, FindHrefInVideoAd(adContent));
				} 
				else
				{
					listener.OnAdResponse(adContent);
				}
			}
		}

		private static string GetStringAd(HttpWebResponse response)
		{
			if (response != null && response.StatusCode == HttpStatusCode.OK)
			{
				StreamReader responseReader = null;
				try
				{
					responseReader = new StreamReader(response.GetResponseStream(), AD_CONTENT_ENCODE);
					string adContent = "";
					char[] read = new char[BLOCK_SIZE];
					int count = responseReader.Read(read, 0, BLOCK_SIZE);
					while (count > 0)
					{
						string str = new string(read, 0, count);
						adContent += str;
						count = responseReader.Read(read, 0, BLOCK_SIZE);
					}

					return adContent;
				}
				catch (System.Exception)
				{}
				finally
				{
					if (responseReader != null)
					{
						responseReader.Close();
					}
				}
			}

			return null;
		}

		private static string CheckAdForMedia(string adContent, out Resource video)
		{
			video = null;
			List<Resource> adImages = FindResources(adContent);
			if (adImages != null)
			{
				foreach (Resource image in adImages)
				{
					try
					{
						string imageLocalPath = LoadAndSaveImage(image.Source);
						if (imageLocalPath != null)
						{
							adContent = adContent.Replace(image.Source, imageLocalPath);

							if (image.ResType == Resource.TYPE.VIDEO)
							{
								video = image;
								video.Source = video.Source.Replace(image.Source, imageLocalPath);
							}
						}
					}
					catch (System.Exception)
					{}
				}
			}

			return adContent;
		}

		private static string LoadAndSaveImage(string source)
		{
			try
			{
				string localImageFile = source.Substring(source.IndexOf('/', ("http://").Length) + 1);
				string localImageFilePath = Path.Combine(RootDir, localImageFile);

				bool exists = false;
				using (IsolatedStorageFile appStorage = IsolatedStorageFile.GetUserStoreForApplication())
				{
					exists = appStorage.FileExists(localImageFilePath);
				}

				if (!exists)
				{
					Debug.WriteLine("file " + localImageFilePath + " not exists");

					WebClient imageLoader = new WebClient();
					ManualResetEvent imageLoaded = new ManualResetEvent(false);
					Stream imageStream = null;
					imageLoader.OpenReadCompleted += (sender, evt) =>
					{
						if (evt.Error == null && evt.Cancelled == false)
						{
							imageStream = evt.Result;
						}
						imageLoaded.Set();
						Debug.WriteLine("loaded");
					};
					imageLoader.OpenReadAsync(new Uri(source, UriKind.RelativeOrAbsolute));
					imageLoaded.WaitOne();

					if (imageStream != null && SaveFileStream(localImageFilePath, imageStream))
					{
						return localImageFile;
					}
				}
				else
				{
					Debug.WriteLine("file " + localImageFilePath + " exists");
					return localImageFile;
				}
			}
			catch (System.Exception)
			{}

			return null;
		}

		private static bool SaveFileStream(string path, Stream fileStream)
		{
			try
			{
				List<string> dirs = new List<string>();
				string rootDir = Path.GetDirectoryName(path);
				while (!String.IsNullOrEmpty(rootDir))
				{
					dirs.Add(rootDir);
					rootDir = Path.GetDirectoryName(rootDir);
				}

				using (IsolatedStorageFile appStorage = IsolatedStorageFile.GetUserStoreForApplication())
				{
					int dirsCount = dirs.Count;
					for (int dirIdx = 0; dirIdx < dirs.Count; dirIdx++)
					{
						appStorage.CreateDirectory(dirs[dirIdx]);
					}

					using (IsolatedStorageFileStream imageFile = appStorage.OpenFile(path, FileMode.Create, FileAccess.Write))
					{
						long fileLength = (long)fileStream.Length;
						byte[] imgBytes = new byte[fileLength];
						fileStream.Read(imgBytes, 0, (int)fileLength);
						imageFile.Write(imgBytes, 0, (int)fileLength);
						Debug.WriteLine("saved");
					}
				}

				return true;
			}
			catch (System.Exception)
			{}
			finally
			{
				fileStream.Close();
			}

			return false;
		}

		private class Resource
		{
			public enum TYPE { IMAGE, VIDEO };
			public TYPE ResType { get; private set; }
			public string Source { get; set; }
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
			//string adPattern = "<(img).*?src=[\",\'](.*?)[\",\'].*?>";
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

		private static string FindHrefInVideoAd(string adContent)
		{
			try
			{
				string adPattern = "<a.*?href=[\",\'](.*?)[\",\'].*?>";
				Regex adRegex = new Regex(adPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
				MatchCollection adMatches = adRegex.Matches(adContent);
				foreach (Match currMatch in adMatches)
				{
					string href = currMatch.Groups[1].Value;
					return href;
				}
			}
			catch (Exception)
			{}

			return null;
		}



		public static void StoreMediaFile(string url)
		{
			try
			{
				string fileName = Path.GetFileName(url);

				Uri picUri = new Uri(url, UriKind.RelativeOrAbsolute);
				if (picUri.IsAbsoluteUri)
				{
					WebClient loader = new WebClient();
					loader.OpenReadCompleted += (s, evt) =>
					{
						if (evt.Error == null && evt.Cancelled == false)
						{
							Stream result = evt.Result;
							SaveToMediaLibrary(fileName, result);
						}
					};
					loader.OpenReadAsync(picUri);
				}
				else
				{
					using (IsolatedStorageFile appStorage = IsolatedStorageFile.GetUserStoreForApplication())
					{
						using (IsolatedStorageFileStream fileStream = appStorage.OpenFile(Path.Combine(RootDir, fileName), FileMode.Open))
						{
							SaveToMediaLibrary(fileName, fileStream);
						}
					}
				}
			}
			catch (Exception)
			{}
		}

		private static void SaveToMediaLibrary(string fileName, Stream fileStream)
		{
			try
			{
				var library = new Microsoft.Xna.Framework.Media.MediaLibrary();
				library.SavePicture(fileName, fileStream);
			}
			catch (Exception)
			{ }
		}


		public static void OrmmaRequest(IDataRequestListener listener, string url, string display)
		{
			try
			{
				Dictionary<string, HttpWebRequest> requests = null;
				if (!adOrmmaRequests.TryGetValue(listener, out requests))
				{
					requests = new Dictionary<string, HttpWebRequest>();
					adOrmmaRequests.Add(listener, requests);
				}

				HttpWebRequest request = null;
				if (!requests.TryGetValue(url, out request))
				{
					request = HttpWebRequest.Create(new Uri(url, UriKind.RelativeOrAbsolute)) as HttpWebRequest;
					if (request != null)
					{
						requests.Add(url, request);

						request.Accept = "text/html";
						request.BeginGetResponse(new AsyncCallback(OrmmaCallback), request);
					}
				}
			}
			catch (Exception)
			{}
		}

		private static void OrmmaCallback(IAsyncResult result)
		{
			IDataRequestListener listener = null;
			string url = null;

			try
			{
				HttpWebRequest request = result.AsyncState as HttpWebRequest;
				if (request != null)
				{
					foreach (KeyValuePair<IDataRequestListener, Dictionary<string, HttpWebRequest>> ormmaRequests in adOrmmaRequests)
					{
						Dictionary<string, HttpWebRequest> allRequests = ormmaRequests.Value;
						if (allRequests != null)
						{
							foreach (KeyValuePair<string, HttpWebRequest> listenerRequests in allRequests)
							{
								if (listenerRequests.Value != null && listenerRequests.Value.Equals(request))
								{
									listener = ormmaRequests.Key;
									url = listenerRequests.Key;
									break;
								}
							}
						}

						if (listener != null)
						{
							break;
						}
					}

					HttpWebResponse response = request.EndGetResponse(result) as HttpWebResponse;
					string ormmaResponse = GetStringAd(response);
					response.Close();

					if (listener != null)
					{
						listener.OnOrmmaResponse(url, ormmaResponse);

						adOrmmaRequests[listener].Remove(url);
					}
				}
			}
			catch (Exception)
			{ }
		}
	}
}

