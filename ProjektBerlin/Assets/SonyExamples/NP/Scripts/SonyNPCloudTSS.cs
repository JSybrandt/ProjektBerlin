using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

// Title Small Storage (TSS) allows read-only cloud storage of title configuration data and other
// data which may be updated and read by the title after release. 16 data slots can be created on 
// the PSN servers.
//
// For additional information for PS Vita please refer to...
// https://psvita.scedev.net/docs/vita-en,NP_TSS-Overview-vita,Library_Overview/1
//
// And for PS4 please refer to...
// https://ps4.scedev.net/resources/documents/SDK/2.500/NpTss-Overview/0001.html

public class SonyNpCloudTSS : IScreen
{
	MenuLayout menuTss;

	public SonyNpCloudTSS()
	{
		Initialize();
	}

	public MenuLayout GetMenu()
	{
		return menuTss;
	}

	public void OnEnter()
	{
	}

	public void OnExit()
	{
	}

	Sony.NP.ErrorCode ErrorHandler(Sony.NP.ErrorCode errorCode = Sony.NP.ErrorCode.NP_ERR_FAILED)
	{
		if (errorCode != Sony.NP.ErrorCode.NP_OK)
		{
			Sony.NP.ResultCode result = new Sony.NP.ResultCode();
			Sony.NP.TusTss.GetLastError(out result);
			if (result.lastError != Sony.NP.ErrorCode.NP_OK)
			{
				OnScreenLog.Add("Error: " + result.className + ": " + result.lastError + ", sce error 0x" + result.lastErrorSCE.ToString("X8"));
				return result.lastError;
			}
		}

		return errorCode;
	}

	public void Process(MenuStack stack)
	{
		MenuTss(stack);
	}

	public void Initialize()
	{
		menuTss = new MenuLayout(this, 550, 34);

		Sony.NP.TusTss.OnTssDataRecieved += OnGotTssData;
		Sony.NP.TusTss.OnTssNoData += OnSomeEvent;
		Sony.NP.TusTss.OnTusTssError += OnTusTssError;
	}

	public void MenuTss(MenuStack menuStack)
	{
		menuTss.Update();

		bool TSSIsReady = Sony.NP.User.IsSignedInPSN && !Sony.NP.TusTss.IsTssBusy();

		if (menuTss.AddItem("TSS Request Data", TSSIsReady))
		{
			ErrorHandler(Sony.NP.TusTss.RequestTssData());
		}

		if (menuTss.AddItem("TSS Request Data from slot", TSSIsReady))
		{
			int slotNumber = 1;
			ErrorHandler(Sony.NP.TusTss.RequestTssDataFromSlot(slotNumber));
		}

		if (menuTss.AddBackIndex("Back"))
		{
			menuStack.PopMenu();
		}
	}

	void OnSomeEvent(Sony.NP.Messages.PluginMessage msg)
	{
		OnScreenLog.Add("Event: " + msg.type);
	}

	void OnTusTssError(Sony.NP.Messages.PluginMessage msg)
	{
		ErrorHandler();
	}

	void OnGotTssData(Sony.NP.Messages.PluginMessage msg)
	{
		OnScreenLog.Add("Got TSS Data");
		byte[] data = Sony.NP.TusTss.GetTssData();
		OnScreenLog.Add(" Data size: " + data.Length);
		string dataString = "";
		for (int i = 0; i < 16 && i < data.Length; i++)
		{
			dataString += data[i].ToString() + ", ";
		}
		OnScreenLog.Add(" Data: " + dataString);
	}
}
