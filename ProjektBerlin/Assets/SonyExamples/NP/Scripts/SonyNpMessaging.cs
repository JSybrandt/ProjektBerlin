using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class SonyNpMessaging : IScreen
{
	MenuLayout menuMessaging;

	public SonyNpMessaging()
	{
		Initialize();
	}

	public MenuLayout GetMenu()
	{
		return menuMessaging;
	}

	public void OnEnter()
	{
	}

	public void OnExit()
	{
	}

	public void Process(MenuStack stack)
	{
		MenuMessaging(stack);
	}
	
	public void Initialize()
    {
	    menuMessaging = new MenuLayout(this, 500, 34);
		// Initialize event handlers.
        Sony.NP.Messaging.OnMessageSent += OnSomeEvent;
        Sony.NP.Messaging.OnMessageNotSent += OnSomeEvent;
        Sony.NP.Messaging.OnMessageCanceled += OnSomeEvent;
        Sony.NP.Messaging.OnCustomDataMessageRetrieved += OnMessagingGotGameMessage;
        Sony.NP.Messaging.OnCustomInviteMessageRetrieved += OnMessagingGotGameInvite;
        Sony.NP.Messaging.OnInGameDataMessageRetrieved += OnMessagingGotInGameDataMessage;
        Sony.NP.Messaging.OnMessageNotSentFreqTooHigh += OnSomeEvent;
		Sony.NP.Messaging.OnMessageError += OnMessageError;
    }

    // Class containing game invite data and methods for reading/writing from and to a byte buffer, the data could be almost anything.
    class GameInviteData
    {
        public string taunt;
        public int level;
        public int score;

        public byte[] WriteToBuffer()
        {
            System.IO.MemoryStream output = new MemoryStream(16);
            System.IO.BinaryWriter writer = new BinaryWriter(output);
            writer.Write(taunt);
            writer.Write(level);
            writer.Write(score);
            writer.Close();
            return output.GetBuffer();
        }

        public void ReadFromBuffer(byte[] buffer)
        {
            System.IO.MemoryStream input = new MemoryStream(buffer);
            System.IO.BinaryReader reader = new BinaryReader(input);
            taunt = reader.ReadString();
            level = reader.ReadInt32();
            score = reader.ReadInt32();
            reader.Close();
        }
    }

    // Class containing message attachment data and methods for reading/writing from and to a byte buffer, the data could be almost anything.
    struct GameData
    {
        public string text;
        public int item1;
        public int item2;
    
        public byte[] WriteToBuffer()
        {
            System.IO.MemoryStream output = new MemoryStream();
            System.IO.BinaryWriter writer = new BinaryWriter(output);
            writer.Write(text);
            writer.Write(item1);
            writer.Write(item2);
            writer.Close();
            return output.ToArray();
        }

        public void ReadFromBuffer(byte[] buffer)
        {
            System.IO.MemoryStream input = new MemoryStream(buffer);
            System.IO.BinaryReader reader = new BinaryReader(input);
            text = reader.ReadString();
            item1 = reader.ReadInt32();
            item2 = reader.ReadInt32();
            reader.Close();
        }
    }
    
    public void MenuMessaging(MenuStack menuStack)
    {
        menuMessaging.Update();

#if UNITY_PSP2
		// For Vita npToolkit uses data messages for session invites
		if (menuMessaging.AddItem("Show Messages & Invites", Sony.NP.User.IsSignedInPSN && !Sony.NP.Messaging.IsBusy()))
		{
			// Bring up the system message dialog.
			Sony.NP.Messaging.ShowRecievedDataMessageDialog();
		}
#elif UNITY_PS4
		if (menuMessaging.AddItem("Show All Invites", Sony.NP.User.IsSignedInPSN && !Sony.NP.Messaging.IsBusy()))
		{
			// Bring up the system invite message dialog.
			Sony.NP.Messaging.ShowRecievedInviteDialog();
		}
#else // PS3
		if (menuMessaging.AddItem("Show All Invites", Sony.NP.User.IsSignedInPSN && !Sony.NP.Messaging.IsBusy()))
		{
			// Bring up the system invite message dialog.
			Sony.NP.Messaging.ShowRecievedInviteDialog();
		}

		if (menuMessaging.AddItem("Show Messages", Sony.NP.User.IsSignedInPSN && !Sony.NP.Messaging.IsBusy()))
		{
			// Bring up the system message dialog.
			Sony.NP.Messaging.ShowRecievedDataMessageDialog();
		}
#endif

        if (menuMessaging.AddItem("Send Session Invite", Sony.NP.User.IsSignedInPSN && Sony.NP.Matching.InSession))
        {
            // Send a session invite message, you must be in an active session to do this (see Matching).
			string messageText = "Join my session";		// Text to send.
			int npIDCount = 8;			// PS4 only, the number of npIDs that can be invited.
            Sony.NP.Matching.InviteToSession(messageText, npIDCount);
        }

#if !UNITY_PS4 // PSVita and PS3 only.
        if (menuMessaging.AddItem("Send Game Invite", Sony.NP.User.IsSignedInPSN && !Sony.NP.Messaging.IsBusy()))
        {
            // Construct some game data to attach to the invite.
            GameInviteData data = new GameInviteData();

            data.taunt = "I got an awesome score, can you do better?";
            data.level = 1;
            data.score = 123456789;
            byte[] bytes = data.WriteToBuffer();

            // Send the invite.
			Sony.NP.Messaging.MsgRequest request = new Sony.NP.Messaging.MsgRequest();
			request.body = "Game invite";
			request.expireMinutes = 30;
			request.data = bytes;
			request.npIDCount = 8;			// PS4 & PSP2 only.
			string desc = "Some data to test invite messages";
			string name = "Test data";
			//string desc = "テスト.";	
			//string name = "テスト.";	
			request.dataDescription = desc;		// PS4 & PSP2 only.
			request.dataName =  name;	// PS4 & PSP2 only.
#if UNITY_PSP2
			request.iconPath = Application.streamingAssetsPath + "/PSP2SessionImage.jpg";	// PS4 & PSP2 only.
#else
			request.iconPath = Application.streamingAssetsPath + "/PS4SessionImage.jpg";	// PS4 & PSP2 only.
#endif
			Sony.NP.Messaging.SendMessage(request);
        }
#endif

		if (menuMessaging.AddItem("Send Data Message", Sony.NP.User.IsSignedInPSN && !Sony.NP.Messaging.IsBusy()))
        {
            // Construct some game data to attach to the message.
            GameData data = new GameData();

            data.text = "Here's some data";
            data.item1 = 2;
            data.item2 = 987654321;
            byte[] bytes = data.WriteToBuffer();

            // Send the message.
			Sony.NP.Messaging.MsgRequest request = new Sony.NP.Messaging.MsgRequest();
			request.body = "Data message";
			request.expireMinutes = 0;
			request.data = bytes;
			request.npIDCount = 8;			// PS4 only & PSP2 only.
			string desc = "Some data to test messages";
			string name = "Test data";
			//string desc = "テスト.";
			//string name = "テスト.";
			request.dataDescription = desc;		// PS4 & PSP2 only.
			request.dataName = name;	// PS4 & PSP2 only.
#if UNITY_PSP2
			request.iconPath = Application.streamingAssetsPath + "/PSP2SessionImage.jpg";	// PS4 & PSP2 only.
#else
			request.iconPath = Application.streamingAssetsPath + "/PS4SessionImage.jpg";	// PS4 & PSP2 only.
#endif
			Sony.NP.Messaging.SendMessage(request);
        }

        if (menuMessaging.AddItem("Send In Game Data (Session)", Sony.NP.Matching.InSession && !Sony.NP.Messaging.IsBusy()))
        {
            Sony.NP.Matching.Session session = Sony.NP.Matching.GetSession();
            Sony.NP.Matching.SessionMemberInfo[] members = session.members;

            if (members == null)
            {
                return;
            }

            // First find a session member to send the message to, just find the first member that isn't 'myself'.
            int found = -1;
            for (int i = 0; i < members.Length; i++)
            {
                if((members[i].memberFlag & Sony.NP.Matching.FlagMemberType.MEMBER_MYSELF) == 0)
                {
                    found = i;
                    break;
                }
            }

            if(found >= 0)
            {
                OnScreenLog.Add("Sending in game data message to " + members[found].npOnlineID);

                GameData data = new GameData();
                data.text = "Here's some data";
                data.item1 = 2;
                data.item2 = 987654321;
                byte[] bytes = data.WriteToBuffer();

                // Send the data message.
                // NOTE: If you send data messages too frequently the send will fail even though SendInGameDataMessage returns true, this is
                // a limitation imposed by Sony, when this happens the Sony.NP.Messaging.OnMessageNotSentFreqTooHigh callback will be called.
                // If you need to send data frequently you may want to use a sockets based solution instead.
                Sony.NP.Messaging.SendInGameDataMessage(members[found].npID, bytes);
            }
            else
            {
                OnScreenLog.Add("No session member to send to.");
            }
        }

        if (menuMessaging.AddItem("Send In Game Message (Friend)", Sony.NP.User.IsSignedInPSN && !Sony.NP.Messaging.IsBusy()))
        {
            // First find a friend who is also running this app.
            Sony.NP.Friends.Friend[] friends = Sony.NP.Friends.GetCachedFriendsList();
            if (friends.Length > 0)
            {
				int found = 0;	// Assume the first friend is playing the same game.
                if (found >= 0)
                {
                    OnScreenLog.Add("Sending in game data message to " + friends[found].npOnlineID);

                    GameData data = new GameData();
                    data.text = "Here's some data";
                    data.item1 = 2;
                    data.item2 = 987654321;
                    byte[] bytes = data.WriteToBuffer();

                    // Send the data message.
                    // NOTE: If you send data messages too frequently the send will fail even though SendInGameDataMessage returns true, this is
                    // a limitation imposed by Sony, when this happens the Sony.NP.Messaging.OnMessageNotSentFreqTooHigh callback will be called.
                    // If you need to send data frequently you may want to use a sockets based solution instead.
					Sony.NP.Messaging.SendInGameDataMessage(friends[found].npID, bytes);
				}
                else
                {
                    OnScreenLog.Add("No friends in this context.");
                }
            }
            else
            {
                OnScreenLog.Add("No friends cached.");
                OnScreenLog.Add("refresh the friends list then try again.");
            }
        }

        if (menuMessaging.AddBackIndex("Back"))
        {
            menuStack.PopMenu();
        }
    }

    void OnSomeEvent(Sony.NP.Messages.PluginMessage msg)
    {
        OnScreenLog.Add("Event: " + msg.type);
    }

    // A game invite attachment has been retrieved so do something with it.
    void OnMessagingGotGameInvite(Sony.NP.Messages.PluginMessage msg)
    {
        OnScreenLog.Add("Got game invite...");

        GameInviteData data = new GameInviteData();

        byte[] buffer = Sony.NP.Messaging.GetGameInviteAttachment();
        data.ReadFromBuffer(buffer);

        OnScreenLog.Add(" taunt: " + data.taunt);
        OnScreenLog.Add(" level: " + data.level);
        OnScreenLog.Add(" score: " + data.score);
    }

    // A message attachment has been retrieved so do something with it.
    void OnMessagingGotGameMessage(Sony.NP.Messages.PluginMessage msg)
    {
        OnScreenLog.Add("Got message...");

        GameData data = new GameData();

        byte[] buffer = Sony.NP.Messaging.GetMessageAttachment();
        data.ReadFromBuffer(buffer);

        OnScreenLog.Add(" text: " + data.text);
        OnScreenLog.Add(" item1: " + data.item1);
        OnScreenLog.Add(" item2: " + data.item2);
    }

    // Received an in-game data message.
    void OnMessagingGotInGameDataMessage(Sony.NP.Messages.PluginMessage msg)
    {
        GameData data = new GameData();
        
        OnScreenLog.Add("Got in-game data message...");

        // Consume all the messages in the received message cache.
        while(Sony.NP.Messaging.InGameDataMessagesRecieved())
        {
            Sony.NP.Messaging.InGameDataMessage igm = Sony.NP.Messaging.GetInGameDataMessage();

            data.ReadFromBuffer(igm.data);

            OnScreenLog.Add(" ID: " + igm.messageID + " text: " + data.text + " item1: " + data.item1 + " item2: " + data.item2);
        }
    }
	
	
    void OnMessageError(Sony.NP.Messages.PluginMessage msg)
    {
		int errorcode = Sony.NP.Messaging.GetErrorFromMessage(msg);
        OnScreenLog.Add(" OnMessageError error code: " + errorcode.ToString("X"));
	}
}
