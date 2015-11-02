using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class SonyNpRanking : IScreen
{
	MenuLayout rankingMenu;
	ulong currentScore = (ulong)0 << 32;
	int rankBoardID = 0;

	public SonyNpRanking()
	{
		Initialize();
	}

	public void OnEnter()
	{
	}

	public void OnExit()
	{
	}

	public void Process(MenuStack stack)
	{
		MenuRanking(stack);
	}

	public MenuLayout GetMenu()
	{
		return rankingMenu;
	}

	public void Initialize()
	{
		rankingMenu = new MenuLayout(this, 450, 34);
		Sony.NP.Ranking.OnCacheRegistered += OnSomeEvent;
		Sony.NP.Ranking.OnRegisteredNewBestScore += OnRegisteredNewBestScore;
		Sony.NP.Ranking.OnNotBestScore += OnSomeEvent;
		Sony.NP.Ranking.OnGotOwnRank += OnRankingGotOwnRank;
		Sony.NP.Ranking.OnGotFriendRank += OnRankingGotFriendRank;
		Sony.NP.Ranking.OnGotRankList += OnRankingGotRankList;
		Sony.NP.Ranking.OnRankingError += OnRankingError;
	}

	Sony.NP.ErrorCode ErrorHandler(Sony.NP.ErrorCode errorCode=Sony.NP.ErrorCode.NP_ERR_FAILED)
	{
		if(errorCode != Sony.NP.ErrorCode.NP_OK)
		{
			Sony.NP.ResultCode result = new Sony.NP.ResultCode();
			Sony.NP.Ranking.GetLastError(out result);
			if (result.lastError != Sony.NP.ErrorCode.NP_OK)
			{
				OnScreenLog.Add("Error: " + result.className + ": " + result.lastError + ", sce error 0x" + result.lastErrorSCE.ToString("X8"));
				return result.lastError;
			}
		}

		return errorCode;
	}

	public void MenuRanking(MenuStack menuStack)
	{
		bool rankingAvailable = Sony.NP.User.IsSignedInPSN;

		rankingMenu.Update();

		if (rankingMenu.AddItem("Register Score", rankingAvailable && !Sony.NP.Ranking.RegisterScoreIsBusy()))
		{
			OnScreenLog.Add("Registering score: " + currentScore);
			ErrorHandler(Sony.NP.Ranking.RegisterScore(rankBoardID, currentScore, "Insert comment here"));
			currentScore++;
		}

		if (rankingMenu.AddItem("Register score & data", rankingAvailable && !Sony.NP.Ranking.RegisterScoreIsBusy()))
		{
			OnScreenLog.Add("Registering score: " + currentScore);
			byte[] additionaldata = new byte[64];
			for (byte i=0;i<64;i++) { additionaldata[i]=i; }	// fill with dummy data as a test
			
			ErrorHandler(Sony.NP.Ranking.RegisterScoreWithData(rankBoardID, currentScore, "Insert comment here" , additionaldata));
			currentScore++;
		}
		
		if (rankingMenu.AddItem("Own Rank", rankingAvailable && !Sony.NP.Ranking.RefreshOwnRankIsBusy()))
		{
			ErrorHandler(Sony.NP.Ranking.RefreshOwnRank(rankBoardID));
		}

		if (rankingMenu.AddItem("Friend Rank", rankingAvailable && !Sony.NP.Ranking.RefreshFriendRankIsBusy()))
		{
			ErrorHandler(Sony.NP.Ranking.RefreshFriendRank(rankBoardID));
		}

		// repeated calls to this will go through the board
		if (rankingMenu.AddItem("Rank List", rankingAvailable && !Sony.NP.Ranking.RefreshRankListIsBusy()))
		{
			int rankRangeStart = LastRankDisplayed + 1;		// starting rank to retrieve
			int rankRangeCount =  Math.Min(10 , (LastRankingMaxCount-rankRangeStart) +1);	// number of entries to retrieve
			ErrorHandler(Sony.NP.Ranking.RefreshRankList(rankBoardID, rankRangeStart, rankRangeCount));
		}

		if (rankingMenu.AddBackIndex("Back"))
		{
			menuStack.PopMenu();
		}
	}

	void OnSomeEvent(Sony.NP.Messages.PluginMessage msg)
	{
		OnScreenLog.Add("Event: " + msg.type);
	}

	void OnRegisteredNewBestScore(Sony.NP.Messages.PluginMessage msg)
	{
		// Registering a new best score updates "Own Rank" but note that this isn't the final rank, it can take 
		// several minutes for the ranking server to update at which point you can call Sony.NP.Ranking.RefreshOwnRank() to
		// update the players rank.
		Sony.NP.Ranking.Rank rank = Sony.NP.Ranking.GetOwnRank();

		OnScreenLog.Add("New best score...");
		OnScreenLog.Add("rank #" + rank.rank + ", provisional rank #" + rank.provisional + ", online id:" + rank.onlineId + ", score:" + rank.score + ", comment:" + rank.comment);
	}

	void LogRank(Sony.NP.Ranking.Rank rank)
	{
		long sceToDotNetTicks = 10;	// sce ticks are microsecond, .net are 100 nanosecond
		DateTime rankingDateTime = new DateTime((long)rank.recordDate * sceToDotNetTicks);

		OnScreenLog.Add("#" + rank.rank + " (provisionally #" + rank.provisional + "), online id:" + rank.onlineId + ", score:" + rank.score + ", comment:" + rank.comment + ", recorded on:" + rankingDateTime.ToString() );
		if (rank.gameInfoSize > 0)
		{
			int count = 0;
			string data = "";
			foreach (byte b in rank.gameInfoData)
			{
				data = data + b.ToString() + ",";
				if (count++ > 8) break;
			}
			data = data + "...";
			OnScreenLog.Add("  dataSize: " + rank.gameInfoSize + ", data: " + data);
		}
	}

	void OnRankingGotOwnRank(Sony.NP.Messages.PluginMessage msg)
	{
		Sony.NP.Ranking.Rank rank = Sony.NP.Ranking.GetOwnRank();

		OnScreenLog.Add("Own rank...");
		if (rank.rank > 0)
		{
			LogRank(rank);
		}
		else
		{
			OnScreenLog.Add("rank #: Not Ranked, " + rank.onlineId);
		}
	}

	void OnRankingGotFriendRank(Sony.NP.Messages.PluginMessage msg)
	{
		Sony.NP.Ranking.Rank[] ranks = Sony.NP.Ranking.GetFriendRanks();

		OnScreenLog.Add("Friend ranks...");
		for (int i = 0; i < ranks.Length; i++)
		{
			LogRank(ranks[i]);
		}
	}

	int LastRankDisplayed = 0;
	int LastRankingMaxCount = 999;
	
	void OnRankingGotRankList(Sony.NP.Messages.PluginMessage msg)
	{
		Sony.NP.Ranking.Rank[] ranks = Sony.NP.Ranking.GetRankList();

		OnScreenLog.Add("Ranks...");
		OnScreenLog.Add("Showing " + ranks[0].serialRank + "-> " + (ranks[0].serialRank + ranks.Length - 1 ) + " out of " + Sony.NP.Ranking.GetRanksCountOnServer());
		for (int i = 0; i < ranks.Length; i++)
		{
			LogRank(ranks[i]);
		}
		LastRankDisplayed = ranks[0].serialRank + ranks.Length - 1;
		LastRankingMaxCount = Sony.NP.Ranking.GetRanksCountOnServer();
		System.Console.WriteLine("LastRankDisplayed:" + LastRankDisplayed + " LastRankingMaxCount:" + LastRankingMaxCount);
		if (LastRankDisplayed >= LastRankingMaxCount) { LastRankDisplayed = 0; }
	}

	void OnRankingError(Sony.NP.Messages.PluginMessage msg)
	{
		ErrorHandler();
	}
}
