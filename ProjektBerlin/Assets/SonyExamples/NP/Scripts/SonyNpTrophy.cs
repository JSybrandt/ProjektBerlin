using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SonyNpTrophy : IScreen
{
    MenuLayout menuTrophies;
    int nextTrophyIndex = 1;    // trophy 0 is the platinum trophy which we can't award, it gets awarded automatically when all other trophies have been awarded.
	Sony.NP.Trophies.GameInfo gameInfo;
	Texture2D trophyIcon = null;
	Texture2D trophyGroupIcon = null;

	public SonyNpTrophy()
	{
		Initialize();
	}

	public MenuLayout GetMenu()
	{
		return menuTrophies;
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
			Sony.NP.Trophies.GetLastError(out result);
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
		MenuTrophies(stack);
	}

	public void Initialize()
	{
		menuTrophies = new MenuLayout(this, 450, 34);
		
		Sony.NP.Trophies.OnGotGameInfo += OnTrophyGotGameInfo;
		Sony.NP.Trophies.OnGotGroupInfo += OnTrophyGotGroupInfo;
		Sony.NP.Trophies.OnGotTrophyInfo += OnTrophyGotTrophyInfo;
		Sony.NP.Trophies.OnGotProgress += OnTrophyGotProgress;
		Sony.NP.Trophies.OnAwardedTrophy += OnSomeEvent;
		Sony.NP.Trophies.OnAwardTrophyFailed += OnSomeEvent;
		Sony.NP.Trophies.OnAlreadyAwardedTrophy += OnSomeEvent;
		Sony.NP.Trophies.OnUnlockedPlatinum += OnSomeEvent;
	}

	public void MenuTrophies(MenuStack menuStack)
	{
        menuTrophies.Update();

		bool trophiesAvailable = Sony.NP.Trophies.TrophiesAreAvailable;

		if (menuTrophies.AddItem("Game Info", trophiesAvailable))
		{
			DumpGameInfo();
		}

#if !UNITY_PS3
		if (menuTrophies.AddItem("Group Info", trophiesAvailable && !Sony.NP.Trophies.RequestGroupInfoIsBusy()))
		{
			ErrorHandler(Sony.NP.Trophies.RequestGroupInfo());
		}
#endif

		if (menuTrophies.AddItem("Trophy Info", trophiesAvailable && !Sony.NP.Trophies.RequestTrophyInfoIsBusy()))
		{
			ErrorHandler(Sony.NP.Trophies.RequestTrophyInfo());
		}

		if (menuTrophies.AddItem("Trophy Progress", trophiesAvailable && !Sony.NP.Trophies.RequestTrophyProgressIsBusy()))
		{
			ErrorHandler(Sony.NP.Trophies.RequestTrophyProgress());
		}

		if (menuTrophies.AddItem("Award Trophy", trophiesAvailable))
		{
			if(ErrorHandler(Sony.NP.Trophies.AwardTrophy(nextTrophyIndex)) == Sony.NP.ErrorCode.NP_OK)
			{
				nextTrophyIndex++;
				if (nextTrophyIndex == gameInfo.numTrophies)
				{
					nextTrophyIndex = 1;
				}
			}
		}
		
		if (menuTrophies.AddItem("Award All Trophies", trophiesAvailable))
		{
			for (int i = 1; i < gameInfo.numTrophies; i++)
			{
				ErrorHandler(Sony.NP.Trophies.AwardTrophy(i));
			}
		}

#if UNITY_PS4
		if (menuTrophies.AddItem("Register Trophy pack", trophiesAvailable))
		{
			Sony.NP.Trophies.RegisterTrophyPack();	// register the trophy pack to the current user
		}		
#endif
        if (menuTrophies.AddBackIndex("Back"))
		{
			menuStack.PopMenu();
		}

		//if (trophyIcon != null)
		//{
		//    GUI.DrawTexture(new Rect(0, 0, trophyIcon.width, trophyIcon.height), trophyIcon);
		//}
		//if (trophyGroupIcon != null)
		//{
		//    GUI.DrawTexture(new Rect(0, 240, trophyGroupIcon.width, trophyGroupIcon.height), trophyGroupIcon);
		//}
	}
	
	void OnSomeEvent(Sony.NP.Messages.PluginMessage msg)
	{
		OnScreenLog.Add("Event: " + msg.type);
	}

	void DumpGameInfo()
	{
		OnScreenLog.Add("title: " + gameInfo.title);
		OnScreenLog.Add("desc: " + gameInfo.description);
		OnScreenLog.Add("numTrophies: " + gameInfo.numTrophies);
		OnScreenLog.Add("numGroups: " + gameInfo.numGroups);
		OnScreenLog.Add("numBronze: " + gameInfo.numBronze);
		OnScreenLog.Add("numSilver: " + gameInfo.numSilver);
		OnScreenLog.Add("numGold: " + gameInfo.numGold);
		OnScreenLog.Add("numPlatinum: " + gameInfo.numPlatinum);
	}

	void OnTrophyGotGameInfo(Sony.NP.Messages.PluginMessage msg)
	{
		gameInfo = Sony.NP.Trophies.GetCachedGameInfo();
		DumpGameInfo();
	}

	void OnTrophyGotGroupInfo(Sony.NP.Messages.PluginMessage msg)
	{
		OnScreenLog.Add("Got Group List!");

		Sony.NP.Trophies.GroupDetails[] details = Sony.NP.Trophies.GetCachedGroupDetails();
		Sony.NP.Trophies.GroupData[] data = Sony.NP.Trophies.GetCachedGroupData();

		OnScreenLog.Add("Groups: " + details.Length);
		for (int i = 0; i < details.Length; i++)
		{
			if (details[i].hasIcon && trophyGroupIcon == null)
			{
				trophyGroupIcon = details[i].icon;
				OnScreenLog.Add("Found icon: " + trophyGroupIcon.width + ", " + trophyGroupIcon.height);
			}
			OnScreenLog.Add(" " + i + ": " +
				details[i].groupId + ", " +
				details[i].title + ", " +
				details[i].description + ", " +
				details[i].numTrophies + ", " +
				details[i].numPlatinum + ", " +
				details[i].numGold + ", " +
				details[i].numSilver + ", " +
				details[i].numBronze);

			OnScreenLog.Add(" " + i + ": " +
				data[i].groupId + ", " +
				data[i].unlockedTrophies + ", " +
				data[i].unlockedPlatinum + ", " +
				data[i].unlockedGold + ", " +
				data[i].unlockedSilver + ", " +
				data[i].unlockedBronze + ", " +
				data[i].progressPercentage +
				data[i].userId.ToString( "X" ) );
		}
	}

	void OnTrophyGotTrophyInfo(Sony.NP.Messages.PluginMessage msg)
	{
		OnScreenLog.Add("Got Trophy List!");

		Sony.NP.Trophies.TrophyDetails[] details = Sony.NP.Trophies.GetCachedTrophyDetails();
		Sony.NP.Trophies.TrophyData[] data = Sony.NP.Trophies.GetCachedTrophyData();

		OnScreenLog.Add("Trophies: " + details.Length);
		for (int i = 0; i < details.Length; i++)
		{
			if (data[i].hasIcon && trophyIcon == null)
			{
				trophyIcon = data[i].icon;
				OnScreenLog.Add("Found icon: " + trophyIcon.width + ", " + trophyIcon.height);
			}
			OnScreenLog.Add(" " + i + ": " +
			details[i].name + ", " + //details[i].description + ", " + 
			details[i].trophyId + ", " + details[i].trophyGrade + ", " + details[i].groupId + ", " + details[i].hidden + ", " + data[i].unlocked + ", " + data[i].timestamp + ", " + data[i].userId.ToString( "X" ));
		}
	}

	void OnTrophyGotProgress(Sony.NP.Messages.PluginMessage msg)
	{
		Sony.NP.Trophies.TrophyProgress progress = Sony.NP.Trophies.GetCachedTrophyProgress();

		OnScreenLog.Add("Progress for userId: 0x" + progress.userId.ToString( "X" ));
		OnScreenLog.Add("progressPercentage: " + progress.progressPercentage);
		OnScreenLog.Add("unlockedTrophies: " + progress.unlockedTrophies);
		OnScreenLog.Add("unlockedPlatinum: " + progress.unlockedPlatinum);
		OnScreenLog.Add("unlockedGold: " + progress.unlockedGold);
		OnScreenLog.Add("unlockedSilver: " + progress.unlockedSilver);
		OnScreenLog.Add("unlockedBronze: " + progress.unlockedBronze);
	}

}
