using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SonyNpFriends : IScreen
{
	MenuLayout menuFriends;

	public SonyNpFriends()
	{
		Initialize();
	}
    
	public MenuLayout GetMenu()
	{
		return menuFriends;
	}

	public void OnEnter()
	{
	}

	public void OnExit()
	{
	}

	Sony.NP.ErrorCode ErrorHandlerFriends(Sony.NP.ErrorCode errorCode=Sony.NP.ErrorCode.NP_ERR_FAILED)
	{
		if (errorCode != Sony.NP.ErrorCode.NP_OK)
		{
			Sony.NP.ResultCode result = new Sony.NP.ResultCode();
			Sony.NP.Friends.GetLastError(out result);
			if(result.lastError != Sony.NP.ErrorCode.NP_OK)
			{
				OnScreenLog.Add("Error: " + result.className + ": " + result.lastError + ", sce error 0x" + result.lastErrorSCE.ToString("X8"));
				return result.lastError;
			}
		}

		return errorCode;
	}

	Sony.NP.ErrorCode ErrorHandlerPresence(Sony.NP.ErrorCode errorCode=Sony.NP.ErrorCode.NP_ERR_FAILED)
	{
		if (errorCode != Sony.NP.ErrorCode.NP_OK)
		{
			Sony.NP.ResultCode result = new Sony.NP.ResultCode();
			Sony.NP.User.GetLastPresenceError(out result);
			if(result.lastError != Sony.NP.ErrorCode.NP_OK)
			{
				OnScreenLog.Add("Error: " + result.className + ": " + result.lastError + ", sce error 0x" + result.lastErrorSCE.ToString("X8"));
				return result.lastError;
			}
		}

		return errorCode;
	}

#if UNITY_PSP2
	Sony.NP.ErrorCode ErrorHandlerTwitter(Sony.NP.ErrorCode errorCode = Sony.NP.ErrorCode.NP_ERR_FAILED)
	{
		if (errorCode != Sony.NP.ErrorCode.NP_OK)
		{
			Sony.NP.ResultCode result = new Sony.NP.ResultCode();
			Sony.NP.Twitter.GetLastError(out result);
			if (result.lastError != Sony.NP.ErrorCode.NP_OK)
			{
				OnScreenLog.Add("Error: " + result.className + ": " + result.lastError + ", sce error 0x" + result.lastErrorSCE.ToString("X8"));
				return result.lastError;
			}
		}

		return errorCode;
	}
#endif

	Sony.NP.ErrorCode ErrorHandlerFacebook(Sony.NP.ErrorCode errorCode = Sony.NP.ErrorCode.NP_ERR_FAILED)
	{
		if (errorCode != Sony.NP.ErrorCode.NP_OK)
		{
			Sony.NP.ResultCode result = new Sony.NP.ResultCode();
			Sony.NP.Facebook.GetLastError(out result);
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
		MenuFriends(stack);
	}

	public void Initialize()
	{
	    menuFriends = new MenuLayout(this, 450, 34);
		
		Sony.NP.Friends.OnFriendsListUpdated += OnFriendsListUpdated;
		Sony.NP.Friends.OnFriendsPresenceUpdated += OnFriendsListUpdated;
		Sony.NP.Friends.OnGotFriendsList += OnFriendsGotList;
		Sony.NP.Friends.OnFriendsListError += OnFriendsListError;
		Sony.NP.User.OnPresenceSet += OnSomeEvent;
		Sony.NP.User.OnPresenceError += OnPresenceError;

        Sony.NP.Facebook.OnFacebookDialogStarted += OnSomeEvent;
        Sony.NP.Facebook.OnFacebookDialogFinished += OnSomeEvent;
        Sony.NP.Facebook.OnFacebookMessagePosted += OnSomeEvent;
		Sony.NP.Facebook.OnFacebookMessagePostFailed += OnFacebookMessagePostFailed;

        Sony.NP.Twitter.OnTwitterDialogStarted += OnSomeEvent;
        Sony.NP.Twitter.OnTwitterDialogCanceled += OnSomeEvent;
        Sony.NP.Twitter.OnTwitterDialogFinished += OnSomeEvent;
        Sony.NP.Twitter.OnTwitterMessagePosted += OnSomeEvent;
#if UNITY_PSP2
		Sony.NP.Twitter.OnTwitterMessagePostFailed += OnTwitterMessagePostFailed;
#endif
    }

	public void MenuFriends(MenuStack menuStack)
	{
        menuFriends.Update();

		if (menuFriends.AddItem("Friends", Sony.NP.User.IsSignedInPSN && !Sony.NP.Friends.FriendsListIsBusy()))
		{
			ErrorHandlerFriends(Sony.NP.Friends.RequestFriendsList());
		}

        if (menuFriends.AddItem("Set Presence", Sony.NP.User.IsSignedInPSN && !Sony.NP.User.OnlinePresenceIsBusy()))
		{
			ErrorHandlerPresence(Sony.NP.User.SetOnlinePresence("Testing UnityNpToolkit"));
		}

        if (menuFriends.AddItem("Clear Presence", Sony.NP.User.IsSignedInPSN && !Sony.NP.User.OnlinePresenceIsBusy()))
		{
			ErrorHandlerPresence(Sony.NP.User.SetOnlinePresence(""));
		}

        if (menuFriends.AddItem("Post On Facebook", Sony.NP.User.IsSignedInPSN && !Sony.NP.Facebook.IsBusy()))
        {
            // To use Facebook integration you need to visit https://developers.facebook.com/ and create
            // an app which binds to your Sony applications title ID, this gets you an app ID which is used below.

            Sony.NP.Facebook.PostFacebook message = new Sony.NP.Facebook.PostFacebook();
            message.appID = 701792156521339;
            message.userText = "I'm testing Unity's facebook integration !";
            message.photoURL = "http://uk.playstation.com/media/RZXT_744/159/PlayStationNetworkFeaturedImage.jpg";
            message.photoTitle = "Title";
            message.photoCaption = "This is the caption";
            message.photoDescription = "This is the description";
            message.actionLinkName = "Go To Unity3d.com";
            message.actionLinkURL = "http://unity3d.com/";
            ErrorHandlerFacebook(Sony.NP.Facebook.PostMessage(message));
        }

#if UNITY_PSP2
        if (menuFriends.AddItem("Post On Twitter", Sony.NP.User.IsSignedInPSN && !Sony.NP.Twitter.IsBusy()))
        {
            // NOTE: For Twitter integration to work the "Use Tw Dialog" flag must be set in the "ATTRIBUTE" member
            // of the param.sfx
            //
            // e.g.
            // <param key="ATTRIBUTE">33554432</param>
            //
            // Or if not using a param.sfx file make sure that "Allow Twitter Dialog" is ticked in the PS Vita player settings.
            //
            // Also, for the setting to work you must build a PSVita package and install it on the Vita, once this has been done
            // Twitter will work with normal 'PC Hosted' builds.

            Sony.NP.Twitter.PostTwitter message = new Sony.NP.Twitter.PostTwitter();
            message.userText = "I'm testing Unity's Twitter integration !";
            message.imagePath = Application.streamingAssetsPath + "/TweetUnity.png";
            message.forbidAttachPhoto = false;
            message.disableEditTweetMsg = true;
            message.forbidOnlyImageTweet = false;
            message.forbidNoImageTweet = false;
            message.disableChangeImage = false;
            message.limitToScreenShot = true;
            ErrorHandlerTwitter(Sony.NP.Twitter.PostMessage(message));
        }
#endif

        if (menuFriends.AddBackIndex("Back"))
		{
			menuStack.PopMenu();
		}
	}

	void OnFacebookMessagePostFailed(Sony.NP.Messages.PluginMessage msg)
	{
		ErrorHandlerFacebook();
	}

#if UNITY_PSP2
	void OnTwitterMessagePostFailed(Sony.NP.Messages.PluginMessage msg)
	{
		ErrorHandlerTwitter();
	}
#endif

	void OnSomeEvent(Sony.NP.Messages.PluginMessage msg)
	{
		OnScreenLog.Add("Event: " + msg.type);
	}

	void OnFriendsListUpdated(Sony.NP.Messages.PluginMessage msg)
	{
		OnScreenLog.Add("Requesting Friends List for Event: " + msg.type);
		ErrorHandlerFriends(Sony.NP.Friends.RequestFriendsList());
	}

	string OnlinePresenceType(Sony.NP.Friends.EnumNpPresenceType type)
	{
		switch (type)
		{
			case Sony.NP.Friends.EnumNpPresenceType.IN_GAME_PRESENCE_TYPE_UNKNOWN:
				return "unknown";
			case Sony.NP.Friends.EnumNpPresenceType.IN_GAME_PRESENCE_TYPE_NONE:
				return "none";
			case Sony.NP.Friends.EnumNpPresenceType.IN_GAME_PRESENCE_TYPE_DEFAULT:
				return "default";
			case Sony.NP.Friends.EnumNpPresenceType.IN_GAME_PRESENCE_TYPE_GAME_JOINING:
				return "joining";
			case Sony.NP.Friends.EnumNpPresenceType.IN_GAME_PRESENCE_TYPE_GAME_JOINING_ONLY_FOR_PARTY:
				return "joining party";
			case Sony.NP.Friends.EnumNpPresenceType.IN_GAME_PRESENCE_TYPE_JOIN_GAME_ACK:
				return "join ack";
		}

		return "unknown";
	}

	string OnlineStatus(Sony.NP.Friends.EnumNpOnlineStatus status)
	{
		switch (status)
		{
			case Sony.NP.Friends.EnumNpOnlineStatus.ONLINE_STATUS_OFFLINE:
				return "offline";
			case Sony.NP.Friends.EnumNpOnlineStatus.ONLINE_STATUS_AFK:
				return "afk";
			case Sony.NP.Friends.EnumNpOnlineStatus.ONLINE_STATUS_ONLINE:
				return "online";
		}
		return "unknown";
	}

	void OnFriendsGotList(Sony.NP.Messages.PluginMessage msg)
	{
		OnScreenLog.Add("Got Friends List!");
		Sony.NP.Friends.Friend[] friends = Sony.NP.Friends.GetCachedFriendsList();
		foreach (Sony.NP.Friends.Friend friend in friends)
		{
#if UNITY_PS3
			OnScreenLog.Add(friend.npOnlineID + " - " + " - " + friend.npComment + " - " + friend.npPresenceTitle + " - " + friend.npPresenceStatus);
#else
			string npID = System.Text.Encoding.Default.GetString(friend.npID);
			OnScreenLog.Add(friend.npOnlineID
						+ ", np(" + npID + ")"
						+ ", os(" + OnlineStatus(friend.npOnlineStatus) + ")"
						+ ", pt(" + OnlinePresenceType(friend.npPresenceType) + ")"
						+ ", prsc(" + friend.npPresenceTitle + ", " + friend.npPresenceStatus + ")"
						+ "," + friend.npComment );
#endif
		}
	}

	void OnFriendsListError(Sony.NP.Messages.PluginMessage msg)
	{
		ErrorHandlerFriends();
	}

	void OnPresenceError(Sony.NP.Messages.PluginMessage msg)
	{
		ErrorHandlerPresence();
	}
}
