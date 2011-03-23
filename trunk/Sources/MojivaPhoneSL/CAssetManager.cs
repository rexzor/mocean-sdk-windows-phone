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
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;

namespace MojivaPhone
{
	internal class CAssetManager
	{
		#region "Variables"
		private FireEventDelegate fireEventDelegate;
		private IsolatedStorageFile fileStorage;
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
		public CAssetManager(FireEventDelegate fireEventDelegate, string assetsRootDir)
		{
			this.fireEventDelegate = fireEventDelegate;
			this.assetsRootDir = assetsRootDir;

			try
			{
				fileStorage = IsolatedStorageFile.GetUserStoreForApplication();
				if (fileStorage.DirectoryExists(assetsRootDir))
				{
					//fileStorage.DeleteDirectory(assetsRootDir);
				}
				fileStorage.CreateDirectory(assetsRootDir);
				isFileStorage = true;
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
				if (!isExist || assetsStored.ContainsKey(alias))
				{
					assetsPool.Add(new CAsset(url, alias));
					System.Diagnostics.Debug.WriteLine("\n NEW asset: " + alias);
				}
			}
		}

		public void RemoveAsset(string alias)
		{
			string fullFilePath = GetFullFilePath(alias);
			try
			{
				if (fileStorage.FileExists(fullFilePath))
				{
					fileStorage.DeleteFile(fullFilePath);
				}

				Deployment.Current.Dispatcher.BeginInvoke(() => fireEventDelegate("assetRemoved", new string[] { alias }));
				assetsStored.Remove(alias);
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

		public string GetAssetURL(string alias)
		{
			try
			{
				return assetsStored[alias];
			}
			catch (System.Exception /*ex*/)
			{ }

			return null;
		}

		public long GetCacheRemaining()
		{
			try
			{
				if (isFileStorage)
				{
					return fileStorage.AvailableFreeSpace;
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
				requests.Add(new CRequest(uri, display));
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
				IsolatedStorageFileStream picStream = null;

				try
				{
					string fullfilePath = GetFullFilePath(url);
					picStream = fileStorage.OpenFile(fullfilePath, FileMode.Open);
					SaveToMediaLibrary(picName, picStream);
				}
				catch
				{ }
				finally
				{
					if (picStream != null)
					{
						picStream.Close();
					}
				}
			}
		}

		public void SaveTextFile(string fileName, string fileContent)
		{
			IsolatedStorageFileStream fileStream = null;
			StreamWriter fileWriter = null;

			try
			{
				fileStream = new IsolatedStorageFileStream(GetFullFilePath(fileName), FileMode.Create, fileStorage);
				fileWriter = new StreamWriter(fileStream);
				fileWriter.WriteLine(fileContent);
			}
			catch (System.Exception /*ex*/)
			{ }
			finally
			{
				if (fileWriter != null)
				{
					fileWriter.Close();
					fileWriter = null;
				}

				if (fileStream != null)
				{
					fileStream.Close();
					fileStream = null;
				}
			}
		}

		~CAssetManager()
		{
			fileStorage.Dispose();
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

			fireEventDelegate = null;
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

			Deployment.Current.Dispatcher.BeginInvoke(() => fireEventDelegate("assetReady", new string[] { asset.alias }));
			addingAsset = null;
		}

		private void StoreScreenShotAsset(CAsset asset)
		{
			System.Windows.Media.Imaging.WriteableBitmap screenShot = new System.Windows.Media.Imaging.WriteableBitmap((int)Application.Current.RootVisual.RenderSize.Width, (int)Application.Current.RootVisual.RenderSize.Height);
			screenShot.Render(Application.Current.RootVisual, null);
			screenShot.Invalidate();

			MemoryStream stream = new MemoryStream();
			System.Windows.Media.Imaging.Extensions.SaveJpeg(screenShot, stream, (int)Application.Current.RootVisual.RenderSize.Width, (int)Application.Current.RootVisual.RenderSize.Height, 0, 100);

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
					if (requests[i].response == null)
					{
						requests[i].response = DataRequest.ReadStringData(requests[i].uri).Trim();
					}
					else
					{
						string uri = requests[i].uri;
						string response = requests[i].response;
						if (requests[i].display == RESPONSE_DISPLAY_PROXY)
						{
							Deployment.Current.Dispatcher.BeginInvoke(() => fireEventDelegate("response", new string[] { uri, response }));
						}
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
			try
			{
				if ((long)content.Length > GetCacheRemaining() && !fileStorage.IncreaseQuotaTo(fileStorage.Quota + (long)content.Length))
				{
					return false;
				}
			}
			catch (System.Exception /*ex*/)
			{
				return false;
			}

			IsolatedStorageFileStream fs = null;
			try
			{
				// creating folder of new asset
				string fullfilePath = GetFullFilePath(file);
				string[] fileFolders = fullfilePath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
				string currFolder = "";
				for (int i = 0; i < fileFolders.Length - 1; i++)
				{
					currFolder += fileFolders[i];
					//System.Diagnostics.Debug.WriteLine(currFolder);
					if (!fileStorage.DirectoryExists(currFolder))
					{
						fileStorage.CreateDirectory(currFolder);
					}
					currFolder += "/";
				}

				// creating asset
				fs = fileStorage.OpenFile(fullfilePath, FileMode.Create);
				fs.Write(content, 0, content.Length);

				return true;
			}
			catch
			{ }
			finally
			{
				if (fs != null)
				{
					fs.Close();
					fs = null;
				}
			}

			return false;
		}

		private void SaveToMediaLibrary(string fileNmae, Stream fileContent)
		{
			try
			{
				fileContent.Seek(0, System.IO.SeekOrigin.Begin);
				var lib = new Microsoft.Xna.Framework.Media.MediaLibrary();
				lib.SavePicture(fileNmae, fileContent);
			}
			catch (System.Exception /*ex*/)
			{ }
			finally
			{ }
		}

		private void BackupAssets()
		{
			System.Diagnostics.Debug.WriteLine("\n ASSETS POOL Deactivated");

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
			public string uri;
			public string display;
			public string response;

			public CRequest(string uri, string display)
			{
				this.uri = uri;
				this.display = display;
				this.response = null;
			}
		}
		#endregion
	}
}
