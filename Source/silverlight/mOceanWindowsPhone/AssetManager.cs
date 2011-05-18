/****
 * © 2010-2011 mOcean Mobile. A subsidiary of Mojiva, Inc. All Rights Reserved.
 * */

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Tasks;

namespace mOceanWindowsPhone
{
	internal class AssetManager
	{
		#region "Variables"
		private enum EAssetState { None, New, Adding, Added }
		private List<CAsset> assetsPool = new List<CAsset>();
		private CAsset addingAsset = null;
		private Dictionary<string, string> assetsStored = new Dictionary<string, string>();
		private List<CRequest> requests = new List<CRequest>();
		private bool isFileStorage = false;
		private string assetsRootDir = "";
		private const string ASSETS_BACKUP_FILENAME = "assets_list";
		private const string SCREENSHOT_ASSET = "ormma://screenshot";
		private const string PHOTO_ASSET = "ormma://photo";
		private const string DEFAULT_PICTURE_NAME = "picture.jpg";
		private const string RESPONSE_DISPLAY_IGNORE = "ignore";
		private const string RESPONSE_DISPLAY_PROXY = "proxy";
		private Thread assetsAddingThread = null;
		private const int ASSETS_ADDING_SLEEP = 100;
		public enum EDeviceState { Online, Offline, None }
		private CDeviceState deviceState = new CDeviceState(EDeviceState.None);
		#endregion

		#region "Public methods"
		public AssetManager(string assetsRootDir)
		{
			this.assetsRootDir = assetsRootDir;

			try
			{
				using (IsolatedStorageFile appStorage = IsolatedStorageFile.GetUserStoreForApplication())
				{
					if (appStorage.DirectoryExists(assetsRootDir))
					{
						//fileStorage.DeleteDirectory(assetsRootDir);
					}

					if (!String.IsNullOrEmpty(assetsRootDir))
					{
						appStorage.CreateDirectory(assetsRootDir);
					}

					isFileStorage = true;
				}
			}
			catch (System.Exception /*ex*/)
			{
				isFileStorage = false;
			}

			StartAssetStoring();
		}

		public void StartAssetStoring()
		{
			bool needStartThread = false;
			lock (deviceState)
			{
				if (deviceState.state == EDeviceState.None)
				{
					needStartThread = true;
				}

				deviceState.state = EDeviceState.Online;
			}

			if (needStartThread)
			{
				try
				{
					assetsAddingThread = new Thread(new ThreadStart(AddingAssetsThreadProc));
					assetsAddingThread.Name = "assetsAddingThread";
					assetsAddingThread.Start();
				}
				catch (System.Exception /*ex*/)
				{ }
			}
		}

		public void StopAssetStoring()
		{
			lock (deviceState)
			{
				deviceState.state = EDeviceState.Offline;
			}
		}

		public void AddAsset(string url, string alias)
		{
			lock (assetsPool)
			{
				bool isExist = false;
				for (int i = 0; i < assetsPool.Count; i++)
				{
					if (assetsPool[i].alias == alias)
					{
						isExist = true;
						break;
					}
				}

				if (isExist)
				{
					OnAssetReady(alias);
				}
				else if (!assetsStored.ContainsKey(alias))
				{
					assetsPool.Add(new CAsset(url, alias));
				}
			}
		}

		public void RemoveAsset(string alias)
		{
			string fullFilePath = GetFullFilePath(alias);
			try
			{
				using (IsolatedStorageFile appStorage = IsolatedStorageFile.GetUserStoreForApplication())
				{
					if (appStorage.FileExists(fullFilePath))
					{
						appStorage.DeleteFile(fullFilePath);
					}
				}

				assetsStored.Remove(alias);
				OnAssetRemoved(alias);
			}
			catch (System.Exception /*ex*/)
			{ }
		}

		public void RemoveAllAssets()
		{
			foreach (string alias in assetsStored.Keys)
			{
				RemoveAsset(alias);
			}
		}

		public long GetCacheRemaining()
		{
			try
			{
				if (isFileStorage)
				{
					return IsolatedStorageFile.GetUserStoreForApplication().AvailableFreeSpace;
				}
				else
				{
					return 0;
				}
			}
			catch (System.Exception /*ex*/)
			{
				return 0;
			}
		}

		public bool Request(string uri, string display)
		{
			lock (requests)
			{
				CRequest request = new CRequest(uri, display);
				request.Response += new ResponseEventHandler(OnResponse);
				requests.Add(request);
			}
			return false;
		}

		public void StorePicture(string url)
		{
			string picName = "";
			try
			{
				picName = url.Substring(url.LastIndexOf('/') + 1);
			}
			catch (System.Exception /*ex*/)
			{
				picName = DEFAULT_PICTURE_NAME;
			}

			Uri picUri = new Uri(url, UriKind.RelativeOrAbsolute);
			if (picUri.IsAbsoluteUri)
			{
				WebClient webClient = new WebClient();
				webClient.OpenReadCompleted += (s, evt) =>
				{
					if (evt.Error == null && evt.Cancelled == false)
					{
						Stream picStream = evt.Result;
						long fileLength = (long)picStream.Length;
						byte[] imgBytes = new byte[fileLength];
						picStream.Read(imgBytes, 0, (int)fileLength);
						picStream.Seek(0, System.IO.SeekOrigin.Begin);
						SaveToMediaLibrary(picName, picStream);
					}
				};
				webClient.OpenReadAsync(picUri);
			}
			else // from storage
			{
				try
				{
					string fullfilePath = GetFullFilePath(url);
					using (IsolatedStorageFile appStorage = IsolatedStorageFile.GetUserStoreForApplication())
					{
						using (IsolatedStorageFileStream picStream = appStorage.OpenFile(fullfilePath, FileMode.Open))
						{
							SaveToMediaLibrary(picName, picStream);
						}
					}
				}
				catch
				{ }
			}
		}

		public void SaveTextFile(string fileName, string fileContent)
		{
			try
			{
				using (IsolatedStorageFile appStorage = IsolatedStorageFile.GetUserStoreForApplication())
				{
					using (IsolatedStorageFileStream fileStream = new IsolatedStorageFileStream(GetFullFilePath(fileName), FileMode.Create, appStorage))
					{
						using (StreamWriter fileWriter = new StreamWriter(fileStream))
						{
							fileWriter.WriteLine(fileContent);
						}
					}
				}
			}
			catch (System.Exception)
			{}
		}

		~AssetManager()
		{
			assetsPool.Clear();
			assetsStored.Clear();
			requests.Clear();
			try
			{
				assetsAddingThread.Abort();
				assetsAddingThread.Join();
			}
			catch (System.Exception /*ex*/)
			{ }
			finally
			{
				assetsAddingThread = null;
			}
		}
		#endregion

		#region "Private methods"
		private void AddingAssetsThreadProc()
		{
			while (true)
			{
				lock (deviceState)
				{
					if (deviceState.state == EDeviceState.Online)
					{
						int assetsPoolCount = 0;
						lock (assetsPool)
						{
							assetsPoolCount = assetsPool.Count;

							if (assetsPoolCount > 0 && addingAsset == null)
							{
								addingAsset = new CAsset(assetsPool[0].url, assetsPool[0].alias);
							}
						}

						if (assetsPoolCount > 0)
						{
							switch (addingAsset.state)
							{
								case EAssetState.New:
									BeginStoreAsset(addingAsset);
									break;
								case EAssetState.Added:
									EndStoreAsset(addingAsset);
									break;
								default:
									break;
							}
						}

						// execute cached requests
						ExecRequests();
					}
				}

				Thread.Sleep(ASSETS_ADDING_SLEEP);
			}
		}

		private void BeginStoreAsset(CAsset asset)
		{
			asset.state = EAssetState.Adding;

			if (asset.url == SCREENSHOT_ASSET)
			{
				Deployment.Current.Dispatcher.BeginInvoke(() => StoreScreenShotAsset(asset));
			}
			else if (asset.url == PHOTO_ASSET)
			{
				StorePhotoAsset(asset);
			}
			else
			{
				StoreWebAsset(asset);
			}
		}

		private void EndStoreAsset(CAsset asset)
		{
			lock (assetsStored)
			{
				try
				{
					assetsStored.Add(asset.alias, asset.url);
				}
				catch (System.ArgumentException /*ex*/)
				{
					assetsStored[asset.alias] = asset.url;
				}
				catch (System.Exception /*ex*/)
				{ }
			}

			lock (assetsPool)
			{
				assetsPool.RemoveAt(0);
			}

			OnAssetReady(asset.alias);
			addingAsset = null;
		}

		private void StoreScreenShotAsset(CAsset asset)
		{
			WriteableBitmap screenShot = new WriteableBitmap((int)Application.Current.RootVisual.RenderSize.Width, (int)Application.Current.RootVisual.RenderSize.Height);
			screenShot.Render(Application.Current.RootVisual, null);
			screenShot.Invalidate();

			MemoryStream stream = new MemoryStream();
			Extensions.SaveJpeg(screenShot, stream, (int)Application.Current.RootVisual.RenderSize.Width, (int)Application.Current.RootVisual.RenderSize.Height, 0, 100);

			if (SaveAssetContent(asset.alias, stream))
			{
				asset.state = EAssetState.Added;
			}
		}

		private void StorePhotoAsset(CAsset asset)
		{
			CameraCaptureTask camTask = new CameraCaptureTask();
			camTask.Completed += (s, evt) =>
			{
				if (evt.Error == null && evt.TaskResult == TaskResult.OK)
				{
					if (SaveAssetContent(asset.alias, evt.ChosenPhoto))
					{
						asset.state = EAssetState.Added;
					}
				}
			};
			camTask.Show();
		}

		private void StoreWebAsset(CAsset asset)
		{
			WebClient webClient = new WebClient();
			webClient.OpenReadCompleted += (s, evt) =>
			{
				if (evt.Error == null && evt.Cancelled == false)
				{
					if (SaveAssetContent(asset.alias, evt.Result))
					{
						asset.state = EAssetState.Added;
					}
				}
			};
			webClient.OpenReadAsync(new Uri(asset.url));
		}

		private void ExecRequests()
		{
			lock (requests)
			{
				for (int i = 0; i < requests.Count; i++)
				{
					if (!requests[i].done && requests[i].response == null)
					{
						DataRequest.RequestStringData(requests[i].uri, requests[i].OnRequestDone);
					}
				}
			}
		}

		private string GetFullFilePath(string fileName)
		{
			return assetsRootDir + "/" + fileName.Trim().TrimStart(new char[] { '/' });
		}

		private bool SaveAssetContent(string file, Stream content)
		{
			long fileLength = (long)content.Length;
			byte[] fileBytes = new byte[fileLength];
			content.Read(fileBytes, 0, (int)fileLength);
			return SaveAssetContent(file, fileBytes);
		}

		private bool SaveAssetContent(string file, byte[] content)
		{
			bool result = false;

			try
			{
				using (IsolatedStorageFile appStorage = IsolatedStorageFile.GetUserStoreForApplication())
				{
					if ((long)content.Length > appStorage.AvailableFreeSpace && !appStorage.IncreaseQuotaTo(appStorage.Quota + (long)content.Length))
					{
						System.Diagnostics.Debug.WriteLine("return false");
						return false;
					}

					string fullfilePath = GetFullFilePath(file);
					string[] fileFolders = fullfilePath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
					string currFolder = "";
					for (int i = 0; i < fileFolders.Length - 1; i++)
					{
						currFolder += fileFolders[i];
						if (!appStorage.DirectoryExists(currFolder))
						{
							appStorage.CreateDirectory(currFolder);
						}
						currFolder += "/";
					}

					using (IsolatedStorageFileStream fileStream = new IsolatedStorageFileStream(GetFullFilePath(fullfilePath), FileMode.Create, appStorage))
					{
						fileStream.Write(content, 0, content.Length);
					}

					result = true;
				}
			}
			catch (System.Exception)
			{
				result = false;
			}

			return result;
		}

		private void SaveToMediaLibrary(string fileName, Stream fileContent)
		{
			try
			{
				fileContent.Seek(0, System.IO.SeekOrigin.Begin);
				var lib = new Microsoft.Xna.Framework.Media.MediaLibrary();
				lib.SavePicture(fileName, fileContent);
			}
			catch (System.Exception /*ex*/)
			{ }
		}

		private void BackupAssets()
		{
			string assetsBackup = "";

			lock (assetsPool)
			{
				for (int i = 0; i < assetsPool.Count; i++)
				{
					if (assetsPool[i].state == EAssetState.New || assetsPool[i].state == EAssetState.Adding)
					{
						assetsBackup += assetsPool[i].alias + "\n";
						assetsBackup += assetsPool[i].url + "\n";
					}
				}
			}

			SaveTextFile(ASSETS_BACKUP_FILENAME, assetsBackup);
		}

		private class CDeviceState
		{
			public EDeviceState state;

			public CDeviceState()
			{
				state = EDeviceState.None;
			}

			public CDeviceState(EDeviceState state)
			{
				this.state = state;
			}
		}

		private class CAsset
		{
			public readonly string alias;
			public readonly string url;
			public EAssetState state;

			private CAsset()
			{
				this.alias = null;
				this.url = null;
				state = EAssetState.None;
			}

			public CAsset(string url, string alias)
			{
				this.alias = alias;
				this.url = url;
				state = EAssetState.New;
			}
		}

		private class CRequest
		{
			public bool done = false;
			public string uri = null;
			public string display = null;
			public string response = null;

			private event ResponseEventHandler responseEvent = null;
			public event ResponseEventHandler Response
			{
				add { responseEvent += value; }
				remove { responseEvent -= value; }
			}

			public CRequest(string uri, string display)
			{
				this.uri = uri;
				this.display = display;
				this.response = null;
			}

			public void OnRequestDone(object sender, DataRequest.StringDataEventArgs e)
			{
				if (!done)
				{
					done = true;
					if (!e.IsError)
					{
						response = e.Data;
						if (responseEvent != null)
						{
							responseEvent(uri, response);
						}
					}
				}
			}
		}
		#endregion

		#region "Events"
		public delegate void AssetEventHandler(string alias);
		private event AssetEventHandler AssetReadyEvent;
		public event AssetEventHandler AssetReady
		{
			add { AssetReadyEvent += value; }
			remove { AssetReadyEvent -= value; }
		}
		protected virtual void OnAssetReady(string alias)
		{
			if (AssetReadyEvent != null)
			{
				AssetReadyEvent(alias);
			}
		}

		private event AssetEventHandler AssetRemovedEvent;
		public event AssetEventHandler AssetRemoved
		{
			add { AssetRemovedEvent += value; }
			remove { AssetRemovedEvent -= value; }
		}
		protected virtual void OnAssetRemoved(string alias)
		{
			if (AssetRemovedEvent != null)
			{
				AssetRemovedEvent(alias);
			}
		}

		public delegate void ResponseEventHandler(string uri, string response);
		private event ResponseEventHandler ResponseEvent;
		public event ResponseEventHandler Response
		{
			add { ResponseEvent += value; }
			remove { ResponseEvent -= value; }
		}
		protected virtual void OnResponse(string uri, string response)
		{
			if (ResponseEvent != null)
			{
				ResponseEvent(uri, response);
			}
		}
		#endregion
	}
}
