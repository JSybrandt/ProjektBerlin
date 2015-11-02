using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

// Title User Storage (TUS) allows cloud storage of up to 64 x 64-bit integers per user,
// and up to 1 MiB of binary data per user for each title on the PSN servers. Additionally
// 8 virtual users can be created per-title to allow common storage for all users.
//
// For additional information for PS Vita please refer to...
// https://psvita.scedev.net/docs/vita-en,NP_TUS-Overview-vita,Library_Overview/1
//
// And for PS4 please refer to...
// https://ps4.scedev.net/resources/documents/SDK/2.500/NpTus-Overview/0001.html
//
// Also demonstrates saving and loading UnityEngine.PlayerPrefs using TUS as one
// approach for saving game-state for use with cross-play titles (PSVita and PS4).

public class SonyNpCloudTUS : IScreen
{
	MenuLayout menuTus;
	string virtualUserOnlineID = "_ERGVirtualUser1";

	// TUS request states.
	enum TUSDataRequestType
	{
		None,
		SaveRawData,
		LoadRawData,
		SavePlayerPrefs,
		LoadPlayerPrefs,
	}
	TUSDataRequestType m_TUSDataRequestType = TUSDataRequestType.None;

	// TUS data slot IDs for storing raw data and PlayerPrefs, slots are created and
	// their properties setup using the SMT (Server Management Tools) on SCE Dev-net.
	const int kTUS_DataSlot_RawData = 1;
	const int kTUS_DataSlot_PlayerPrefs = 3;

	public SonyNpCloudTUS()
	{
		Initialize();
	}

	public MenuLayout GetMenu()
	{
		return menuTus;
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
		MenuTus(stack);
	}

	public void Initialize()
    {
		menuTus = new MenuLayout(this, 550, 34);

		// Add event handlers for async TUS request completion.
		Sony.NP.TusTss.OnTusDataSet += OnSetTusData;
        Sony.NP.TusTss.OnTusDataRecieved += OnGotTusData;
        Sony.NP.TusTss.OnTusVariablesSet += OnSetTusVariables;
		Sony.NP.TusTss.OnTusVariablesModified += OnModifiedTusVariables;
		Sony.NP.TusTss.OnTusVariablesRecieved += OnGotTusVariables;
		Sony.NP.TusTss.OnTusTssError += OnTusTssError;
	}

	public void MenuTus(MenuStack menuStack)
    {
        menuTus.Update();

        bool TUSIsReady = Sony.NP.User.IsSignedInPSN && !Sony.NP.TusTss.IsTusBusy();

        if (menuTus.AddItem("TUS Set Data", TUSIsReady))
        {
			// Create some data to test with.
            byte[] data = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                data[i] = (byte)(3 - i);
            }
            OnScreenLog.Add(" Data size: " + data.Length);
            string dataString = "";
            for (int i = 0; i < 16 && i < data.Length; i++)
            {
                dataString += data[i].ToString() + ", ";
            }
            OnScreenLog.Add(" Data: " + dataString);

			// Send the data to TUS.
			m_TUSDataRequestType = TUSDataRequestType.SaveRawData;
			ErrorHandler(Sony.NP.TusTss.SetTusData(kTUS_DataSlot_RawData, data));
        }

        if (menuTus.AddItem("TUS Request Data", TUSIsReady))
        {
			// Request some data from TUS.
			m_TUSDataRequestType = TUSDataRequestType.LoadRawData;
			ErrorHandler(Sony.NP.TusTss.RequestTusData(kTUS_DataSlot_RawData));
        }

#if (UNITY_5_2 || UNITY_5_3) && (UNITY_PS4 || UNITY_PSP2)
		if (menuTus.AddItem("TUS Save PlayerPrefs", TUSIsReady))
		{
			// Setup some test data in PlayerPrefs.
			PlayerPrefs.DeleteAll();
			PlayerPrefs.SetInt("keyA", 1);
			PlayerPrefs.SetString("keyB", "Hello");
			PlayerPrefs.SetInt("keyC", 3);
			PlayerPrefs.SetInt("keyD", 4);

			// Save the PlayerPrefs to a byte array.
	#if UNITY_PS4
			byte[] buffer = UnityEngine.PS4.PS4PlayerPrefs.SaveToByteArray();
	#elif UNITY_PSP2
			byte[] buffer = UnityEngine.PSVita.PSVitaPlayerPrefs.SaveToByteArray();
	#endif

			// Send the byte array to TUS.
			m_TUSDataRequestType = TUSDataRequestType.SavePlayerPrefs;
			ErrorHandler(Sony.NP.TusTss.SetTusData(kTUS_DataSlot_PlayerPrefs, buffer));
		}

		if (menuTus.AddItem("TUS Load PlayerPrefs", TUSIsReady))
		{
			// Request data from the TUS player prefs data slot.
			m_TUSDataRequestType = TUSDataRequestType.LoadPlayerPrefs;
			ErrorHandler(Sony.NP.TusTss.RequestTusData(kTUS_DataSlot_PlayerPrefs));
		}
#endif

		if (menuTus.AddItem("TUS Set Variables", TUSIsReady))
		{
			// Setup an array of variables to set, each variable consists of a slot ID and a value.
			Sony.NP.TusTss.TusVariable[] variables = new Sony.NP.TusTss.TusVariable[4];
			variables[0] = new Sony.NP.TusTss.TusVariable(1, 110);
			variables[1] = new Sony.NP.TusTss.TusVariable(2, 220);
			variables[2] = new Sony.NP.TusTss.TusVariable(3, 330);
			variables[3] = new Sony.NP.TusTss.TusVariable(4, 440);

			// Set the variables.
			ErrorHandler(Sony.NP.TusTss.SetTusVariables(variables));
		}

		if (menuTus.AddItem("TUS Request Variables", TUSIsReady))
		{
			// Setup up an array of slot IDs for the variables we want to request.
			int[] slotIDs = { 1, 2, 3, 4 };

			// Request the variables.
			ErrorHandler(Sony.NP.TusTss.RequestTusVariables(slotIDs));
		}
		
		if (menuTus.AddItem("TUS Set Variables VU", TUSIsReady))
		{
			// Setup an array of variables to set, each variable consists of a slot ID and a value.
			Sony.NP.TusTss.TusVariable[] variables = new Sony.NP.TusTss.TusVariable[1];
			variables[0] = new Sony.NP.TusTss.TusVariable(5, 12345);

			// Set the variables, note that TUS data for VirtualUsers is accessible for all users.
			ErrorHandler(Sony.NP.TusTss.SetTusVariablesForVirtualUser(virtualUserOnlineID, variables));
		}

		if (menuTus.AddItem("TUS Request Variables VU", TUSIsReady))
		{
			// Setup up an array of slot IDs for the variables we want to request.
			int[] slotIDs = { 5 };

			// Request the variables.
			ErrorHandler(Sony.NP.TusTss.RequestTusVariablesForVirtualUser(virtualUserOnlineID, slotIDs));
		}

		if (menuTus.AddItem("TUS Modify Variables VU", TUSIsReady))
		{
			// Setup an array of variables to set, each variable consists of a slot ID and a value.
			Sony.NP.TusTss.TusVariable[] variables = new Sony.NP.TusTss.TusVariable[1];
			variables[0] = new Sony.NP.TusTss.TusVariable(5, 1);

			// Modify TUS Variables, adds the values specified in the variable list to each of the corresponding variables on the server.
			ErrorHandler(Sony.NP.TusTss.ModifyTusVariablesForVirtualUser(virtualUserOnlineID, variables));
		}

		if (menuTus.AddBackIndex("Back"))
		{
			menuStack.PopMenu();
		}
    }

	
	// Event handler; called when an async TUS request fails due to some error.
	void OnTusTssError(Sony.NP.Messages.PluginMessage msg)
    {
		ErrorHandler();
    }

	// Event handler; called when the async request to set TUS data has completed.
	void OnSetTusData(Sony.NP.Messages.PluginMessage msg)
	{
		switch(m_TUSDataRequestType)
		{
			case TUSDataRequestType.SavePlayerPrefs:
				OnScreenLog.Add("Sent PlayerPrefs to TUS");
				break;

			case TUSDataRequestType.SaveRawData:
				OnScreenLog.Add("Sent data to TUS");
				break;
		}
	}

	// Event handler; called when the async request to get TUS data has completed.
    void OnGotTusData(Sony.NP.Messages.PluginMessage msg)
    {
		byte[] data;

		switch (m_TUSDataRequestType)
		{
#if (UNITY_5_2 || UNITY_5_3) && (UNITY_PS4 || UNITY_PSP2)
			case TUSDataRequestType.LoadPlayerPrefs:
				OnScreenLog.Add("Got PlayerPrefs from TUS...");

				// Get the data that was just received from TUS.
				data = Sony.NP.TusTss.GetTusData();

				// Initialise PlayerPrefs using the received data.
#if UNITY_PS4
				UnityEngine.PS4.PS4PlayerPrefs.LoadFromByteArray(data);
#elif UNITY_PSP2
				UnityEngine.PSVita.PSVitaPlayerPrefs.LoadFromByteArray(data);
#endif

				OnScreenLog.Add(" keyA = " + PlayerPrefs.GetInt("keyA"));
				OnScreenLog.Add(" keyB = " + PlayerPrefs.GetString("keyB"));
				OnScreenLog.Add(" keyC = " + PlayerPrefs.GetInt("keyC"));
				OnScreenLog.Add(" keyD = " + PlayerPrefs.GetInt("keyD"));
				break;
#endif
			case TUSDataRequestType.LoadRawData:
				OnScreenLog.Add("Got TUS Data");

				data = Sony.NP.TusTss.GetTusData();

				OnScreenLog.Add(" Data size: " + data.Length);
				string dataString = "";
				for (int i = 0; i < 16 && i < data.Length; i++)
				{
					dataString += data[i].ToString() + ", ";
				}
				OnScreenLog.Add(" Data: " + dataString);
				break;
		}
    }

	// Event handler; called when the async request to set TUS variables has completed.
	void OnSetTusVariables(Sony.NP.Messages.PluginMessage msg)
	{
		OnScreenLog.Add("Sent TUS variables");
	}

	// Event handler; called when the async request to get TUS variables has completed.
    void OnGotTusVariables(Sony.NP.Messages.PluginMessage msg)
    {
        OnScreenLog.Add("Got TUS Variables");

		// If we only want the values we could do long[] values = Sony.NP.TusTss.GetTusVariablesValue()
		// But lets get more info...

		Sony.NP.TusTss.TusRetrievedVariable[] variables = Sony.NP.TusTss.GetTusVariables();
		for (int i = 0; i < variables.Length; i++)
		{
			string ownerNpID = System.Text.Encoding.Default.GetString(variables[i].ownerNpID);
			string lastChangeAuthorNpID = System.Text.Encoding.Default.GetString(variables[i].lastChangeAuthorNpID);
			DateTime nowVar = new DateTime(variables[i].lastChangedDate, DateTimeKind.Utc);
			
			OnScreenLog.Add(" HasData: " + variables[i].hasData);
			OnScreenLog.Add(" Value: " + variables[i].variable);
			OnScreenLog.Add(" OwnerNpID: " + ownerNpID);
			OnScreenLog.Add(" lastChangeNpID: " + lastChangeAuthorNpID);
			OnScreenLog.Add(" lastChangeTime: " + nowVar.ToLongDateString() + " - " + nowVar.ToLongTimeString());
		}
	}

	// Event handler; called when the async request to modify TUS variables has completed.
	void OnModifiedTusVariables(Sony.NP.Messages.PluginMessage msg)
	{
		OnScreenLog.Add("Modified TUS Variables");

		// If we only want the values we could do long[] values = Sony.NP.TusTss.GetTusVariablesValue()
		// But lets get more info...

		Sony.NP.TusTss.TusRetrievedVariable[] variables = Sony.NP.TusTss.GetTusVariables();
		for (int i = 0; i < variables.Length; i++)
		{
			string ownerNpID = System.Text.Encoding.Default.GetString(variables[i].ownerNpID);
			string lastChangeAuthorNpID = System.Text.Encoding.Default.GetString(variables[i].lastChangeAuthorNpID);
			DateTime nowVar = new DateTime(variables[i].lastChangedDate, DateTimeKind.Utc);

			OnScreenLog.Add(" HasData: " + variables[i].hasData);
			OnScreenLog.Add(" Value: " + variables[i].variable);
			OnScreenLog.Add(" OwnerNpID: " + ownerNpID);
			OnScreenLog.Add(" lastChangeNpID: " + lastChangeAuthorNpID);
			OnScreenLog.Add(" lastChangeTime: " + nowVar.ToLongDateString() + " - " + nowVar.ToLongTimeString());
		}
	}
}
