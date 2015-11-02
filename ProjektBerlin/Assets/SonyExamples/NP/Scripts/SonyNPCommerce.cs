using UnityEngine;
#if UNITY_PSP2
using UnityEngine.PSVita;
#endif
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class SonyNpCommerce : IScreen
{
	MenuLayout menu;
	SonyNpCommerceEntitlements entitlements;
	SonyNpCommerceStore store;
	SonyNpCommerceInGameStore inGameStore;

	public SonyNpCommerce()
	{
		Initialize();
	}

	public MenuLayout GetMenu()
	{
		return menu;
	}

	public void Initialize()
	{
		menu = new MenuLayout(this, 450, 34);
		store = new SonyNpCommerceStore();
		inGameStore = new SonyNpCommerceInGameStore();
		entitlements = new SonyNpCommerceEntitlements();

		Sony.NP.Commerce.OnError += OnCommerceError;
		Sony.NP.Commerce.OnDownloadListStarted += OnSomeEvent;
		Sony.NP.Commerce.OnDownloadListFinished += OnSomeEvent;
	}

	public void OnEnter()
	{
	}

	public void OnExit()
	{
	}

	static public Sony.NP.ErrorCode ErrorHandler(Sony.NP.ErrorCode errorCode=Sony.NP.ErrorCode.NP_ERR_FAILED)
	{
		if (errorCode != Sony.NP.ErrorCode.NP_OK)
		{
			Sony.NP.ResultCode result = new Sony.NP.ResultCode();
			Sony.NP.Commerce.GetLastError(out result);
			if(result.lastError != Sony.NP.ErrorCode.NP_OK)
			{
				OnScreenLog.Add("Error: " + result.className + ": " + result.lastError + ", sce error 0x" + result.lastErrorSCE.ToString("X8"));
				return result.lastError;
			}
		}

		return errorCode;
	}

	public void Process(MenuStack stack)
	{
		bool commerceReady = Sony.NP.User.IsSignedInPSN && !Sony.NP.Commerce.IsBusy();
		
		menu.Update();

#if !UNITY_PS3
		if (menu.AddItem("Store", commerceReady))
		{
			stack.PushMenu(store.GetMenu());
		}
#endif

		if (menu.AddItem("In Game Store"))
		{
			stack.PushMenu(inGameStore.GetMenu());
		}

		if (menu.AddItem("Downloads"))
		{
			Sony.NP.Commerce.DisplayDownloadList();
		}

		if (menu.AddItem("Entitlements"))
		{
			stack.PushMenu(entitlements.GetMenu());
		}

#if UNITY_PSP2
		if (menu.AddItem("Find Installed Content"))
		{
			EnumerateDRMContent();
		}
#endif
		
		if (menu.AddBackIndex("Back"))
		{
			stack.PopMenu();
		}
	}

#if UNITY_PSP2
	int EnumerateDRMContentFiles(string contentDir)
	{
		int fileCount = 0;
		PSVitaDRM.ContentOpen(contentDir);

		string filePath = "addcont0:" + contentDir;

		OnScreenLog.Add("Found content folder: " + filePath);
		string[] files = Directory.GetFiles(filePath);
		OnScreenLog.Add(" containing " + files.Length + " files");
		foreach (string file in files)
		{
			OnScreenLog.Add("  " + file);
			fileCount++;
			// As a test, if the content file is an asset bundle load it's assets.
			if (file.Contains(".unity3d"))
			{
#if UNITY_5_3
				AssetBundle bundle = AssetBundle.LoadFromFile(file);
#else
				AssetBundle bundle = AssetBundle.CreateFromFile(file);
#endif

#if UNITY_4_3
				Object[] assets = bundle.LoadAll();
#else
				Object[] assets = bundle.LoadAllAssets();
#endif
				OnScreenLog.Add("  Loaded " + assets.Length + " assets from asset bundle.");

				bundle.Unload(false);
			}
		}

		PSVitaDRM.ContentClose(contentDir);
		return fileCount;
	}
	
	void EnumerateDRMContent()
	{
		int fileCount = 0;
		PSVitaDRM.DrmContentFinder finder = new PSVitaDRM.DrmContentFinder();
		finder.dirHandle = -1;
		if (PSVitaDRM.ContentFinderOpen(ref finder))
		{
			fileCount += EnumerateDRMContentFiles(finder.contentDir);
			while (PSVitaDRM.ContentFinderNext(ref finder))
			{
				fileCount += EnumerateDRMContentFiles(finder.contentDir);
			};
			PSVitaDRM.ContentFinderClose(ref finder);
		}

		OnScreenLog.Add("Found " + fileCount + " files in installed DRM content");
	}
#endif

	void OnCommerceError(Sony.NP.Messages.PluginMessage msg)
	{
		ErrorHandler();
	}

	void OnSomeEvent(Sony.NP.Messages.PluginMessage msg)
	{
		OnScreenLog.Add("Event: " + msg.type);
	}
}
