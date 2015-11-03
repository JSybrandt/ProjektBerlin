using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class SonyNpRequests : IScreen
{
	MenuLayout menu;

	public SonyNpRequests()
	{
		Initialize();
	}

	public MenuLayout GetMenu()
	{
		return menu;
	}

	public void OnEnter()
	{
	}

	public void OnExit()
	{
	}

	Sony.NP.ErrorCode ErrorHandler(Sony.NP.ErrorCode errorCode)
	{
		if (errorCode != Sony.NP.ErrorCode.NP_OK)
		{
			OnScreenLog.Add("Error: " + errorCode);
		}

		return errorCode;
	}

	public void Process(MenuStack stack)
	{
		menu.Update();

#if UNITY_PS4
		if (menu.AddItem("PlayStationPlus check", Sony.NP.User.IsSignedInPSN))
		{
			Sony.NP.Requests.PlusFeature features = Sony.NP.Requests.PlusFeature.REALTIME_MULTIPLAY;
			ErrorHandler(Sony.NP.Requests.CheckPlus(features));		// uses default user, result is returned in OnCheckPlusResult()
		}
		if (menu.AddItem("PlayStationPlus check all", Sony.NP.User.IsSignedInPSN))		// check all local users
		{
			Sony.NP.Requests.PlusFeature features = Sony.NP.Requests.PlusFeature.REALTIME_MULTIPLAY;

			for (int slot = 0; slot < 4; slot++)
			{
				UnityEngine.PS4.PS4Input.LoggedInUser userdata = UnityEngine.PS4.PS4Input.PadGetUsersDetails( slot );
				if (userdata.userId != -1)
				{
					ErrorHandler(Sony.NP.Requests.CheckPlus(userdata.userId, features)); // results are returned in OnCheckPlusResult()
				}
			}
		}

		if (menu.AddItem("Notify Plus realtime", Sony.NP.User.IsSignedInPSN))
		{
			// this needs to be called everyframe (or at least once a second)
			Sony.NP.Requests.PlusFeature features = Sony.NP.Requests.PlusFeature.REALTIME_MULTIPLAY;
			ErrorHandler(Sony.NP.Requests.NotifyPlusFeature(features));		// uses default user ... 
		}
		
		if (menu.AddItem("Notify Plus async", Sony.NP.User.IsSignedInPSN))
		{
			Sony.NP.Requests.PlusFeature features = Sony.NP.Requests.PlusFeature.ASYNC_MULTIPLAY;
			ErrorHandler(Sony.NP.Requests.NotifyPlusFeature(features));		// uses default user
		}
		
		if (menu.AddItem("GetAccountLanguage", Sony.NP.User.IsSignedInPSN))
		{
			ErrorHandler(Sony.NP.Requests.GetAccountLanguage(Sony.NP.User.GetCachedUserProfile().onlineID));
		}
		if (menu.AddItem("GetParentalControlInfo", Sony.NP.User.IsSignedInPSN))
		{
			ErrorHandler(Sony.NP.Requests.GetParentalControlInfo(Sony.NP.User.GetCachedUserProfile().onlineID));
		
		}
		if (menu.AddItem("CheckNpAvailability", Sony.NP.User.IsSignedInPSN))
		{
			Sony.NP.ErrorCode result = Sony.NP.Requests.CheckNpAvailability(Sony.NP.User.GetCachedUserProfile().onlineID);
			OnScreenLog.Add("CheckNpAvailability result : " +  result);
		}
		if (menu.AddItem("SetGamePresenceOnline", Sony.NP.User.IsSignedInPSN))
		{
			Sony.NP.ErrorCode result = Sony.NP.Requests.SetGamePresenceOnline(Sony.NP.User.GetCachedUserProfile().onlineID);
			OnScreenLog.Add("SetGamePresenceOnline result : " +  result);
		}		
#endif
		  
		if (menu.AddBackIndex("Back"))
		{
			stack.PopMenu();
		}
	}

	public void Initialize()
	{
		menu = new MenuLayout(this, 450, 34);
		Sony.NP.Requests.OnCheckPlusResult += OnCheckPlusResult;
		Sony.NP.Requests.OnAccountLanguageResult += OnAccountLanguageResult;
		Sony.NP.Requests.OnParentalControlResult += OnParentalControlResult;
	}


	void OnCheckPlusResult(Sony.NP.Messages.PluginMessage msg)
	{
//		byte[] result = Sony.NP.Requests.GetRequestResultData(msg);
//		OnScreenLog.Add("result as hex : " + result.Length +  " : " +  BitConverter.ToString(result));

		bool checkresult;
		int userId;
		Sony.NP.Requests.GetCheckPlusResult(msg, out checkresult, out userId);
		OnScreenLog.Add("OnPlusCheckResult  returned:" + checkresult + " userId :0x" + userId.ToString("X"));
	}

	void OnAccountLanguageResult(Sony.NP.Messages.PluginMessage msg)
	{
		OnScreenLog.Add("OnAccountLanguageResult  AccountLanguage:" + Sony.NP.Requests.GetAccountLanguageResult(msg) + " OnlineID: " + Sony.NP.Requests.GetRequestOnlineId(msg) );
	}

	void OnParentalControlResult(Sony.NP.Messages.PluginMessage msg)
	{
		int Age;
		bool chatRestriction;
		bool ugcRestriction;
		Sony.NP.Requests.GetParentalControlInfoResult(msg, out Age, out chatRestriction, out ugcRestriction);
		OnScreenLog.Add("OnParentalControlResult  Age:" + Age + " chatRestriction:" + chatRestriction + " ugcRestriction:" + ugcRestriction + " OnlineID: " + Sony.NP.Requests.GetRequestOnlineId(msg) );
	}
	
}
