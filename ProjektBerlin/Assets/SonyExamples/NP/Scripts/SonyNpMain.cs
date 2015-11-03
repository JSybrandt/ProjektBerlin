using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class SonyNpMain : MonoBehaviour, IScreen
{
	MenuStack menuStack = null;
	MenuLayout menuMain;
	bool npReady = false;		// Is the NP plugin initialised and ready for use.
	SonyNpUser user;
	SonyNpFriends friends;
	SonyNpTrophy trophies;
	SonyNpRanking ranking;
	SonyNpSession sessions;
    int sendCount = 0;
    float sendingInterval = 1;
    SonyNpMessaging messaging;
	SonyNpCloud cloudStorage;
    SonyNpUtilities utilities;
	SonyNpCommerce commerce;
	SonyNpRequests requests;
	
	struct Avatar
	{
		public Avatar(GameObject gameObject)
		{
			this.gameObject = gameObject;
			url = "";
			pendingDownload = false;
			texture = null;
		}
		public string url;
		public bool pendingDownload;
		public Texture2D texture;
		public GameObject gameObject;
	};

	static Avatar[] avatars = new Avatar[2];

	static public Texture2D avatarTexture = null;

    // Class containing some data and methods for reading/writing from and to a byte buffer, the data could be almost anything.
    public struct SharedSessionData
    {
        public int id;
        public string text;
        public int item1;
        public int item2;

        public byte[] WriteToBuffer()
        {
            System.IO.MemoryStream output = new MemoryStream();
            System.IO.BinaryWriter writer = new BinaryWriter(output);
            writer.Write(id);
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
            id = reader.ReadInt32();
            text = reader.ReadString();
            item1 = reader.ReadInt32();
            item2 = reader.ReadInt32();
            reader.Close();
        }
    }
 
	
    void Start()
	{
		avatars[0] = new Avatar(GameObject.Find("UserAvatar"));
		avatars[1] = new Avatar(GameObject.Find("RemoteUserAvatar"));

#if UNITY_PS4
	    menuMain = new MenuLayout(this, 600, 34);
#else
	    menuMain = new MenuLayout(this, 500, 34);
#endif
		menuStack = new MenuStack();
		menuStack.SetMenu(menuMain);

		// Register a callback for completion of NP initialization.
        Sony.NP.Main.OnNPInitialized += OnNPInitialized;

        // Initialize the NP Toolkit.
        OnScreenLog.Add("Initializing NP");

#if UNITY_PSP2
		// Some PS4 and PSP2 Np APIs require a session image, specifically for sending matching session invite messages.
		string sessionImage = Application.streamingAssetsPath + "/PSP2SessionImage.jpg";
		Sony.NP.Main.SetSessionImage(sessionImage);
#endif
#if UNITY_PS4
		// Some PS4 and PSP2 Np APIs require a session image, specifically for sending matching session invite messages.
		string sessionImage = Application.streamingAssetsPath + "/PS4SessionImage.jpg";
		Sony.NP.Main.SetSessionImage(sessionImage);
#endif
		
#if UNITY_PS4
		UnityEngine.PS4.PS4Input.OnUserServiceEvent = ((uint eventtype, uint userid) => 
		{
			int SCE_USER_SERVICE_EVENT_TYPE_LOGOUT = 1;
			if (eventtype == SCE_USER_SERVICE_EVENT_TYPE_LOGOUT)
				Sony.NP.User.LogOutUser((int)userid);
		} );
#endif
		
		// Enable/Disable internal logging, log messages are handled by the OnLog, OnLogWarning and OnLogError event handlers.
		Sony.NP.Main.enableInternalLogging = true;

		// Add NP event handlers.
		Sony.NP.Main.OnLog += OnLog;
		Sony.NP.Main.OnLogWarning += OnLogWarning;
		Sony.NP.Main.OnLogError += OnLogError;

		// Initialise with the age rating that was set in the editor player settings...
		//
		// NOTE: If your application does not make use of NP ranking then you must specify the
		// Sony.NP.Main.kNpToolkitCreate_NoRanking flag when initializing otherwise you will be
		// in violation of "TRC R3002 - Title calls NpScore",
		//
		// For example...
		//
		// int npCreationFlags = Sony.NP.Main.kNpToolkitCreate_CacheTrophyIcons | Sony.NP.Main.kNpToolkitCreate_NoRanking;
		// Sony.NP.Main.Initialize(npCreationFlags);
		//
		int npCreationFlags = Sony.NP.Main.kNpToolkitCreate_CacheTrophyIcons;
		Sony.NP.Main.Initialize(npCreationFlags);

		// Alternatively you can initialise with an age rating override.
		//Sony.NP.Main.InitializeWithNpAgeRating(npCreationFlagss, 12);

		// System events.
		Sony.NP.System.OnConnectionUp += OnSomeEvent;
		Sony.NP.System.OnConnectionDown += OnConnectionDown;
		Sony.NP.System.OnSysResume += OnSomeEvent;
        Sony.NP.System.OnSysNpMessageArrived += OnSomeEvent;
		Sony.NP.System.OnSysStorePurchase += OnSomeEvent;
		Sony.NP.System.OnSysStoreRedemption += OnSomeEvent;
		Sony.NP.System.OnSysEvent += OnSomeEvent;	// Some other event.
		
		// Messaging events.
		Sony.NP.Messaging.OnSessionInviteMessageRetrieved += OnMessagingSessionInviteRetrieved;
        Sony.NP.Messaging.OnMessageSessionInviteReceived += OnMessagingSessionInviteReceived;
        Sony.NP.Messaging.OnMessageSessionInviteAccepted += OnMessagingSessionInviteAccepted;

        // User events.
        Sony.NP.User.OnSignedIn += OnSignedIn;
		Sony.NP.User.OnSignedOut += OnSomeEvent;
		Sony.NP.User.OnSignInError += OnSignInError;

		user = new SonyNpUser();
		friends = new SonyNpFriends();
		trophies = new SonyNpTrophy();
		ranking = new SonyNpRanking();
		sessions = new SonyNpSession();
        messaging = new SonyNpMessaging();
		commerce = new SonyNpCommerce();
        cloudStorage = new SonyNpCloud();
        utilities = new SonyNpUtilities();
#if UNITY_PS4
		requests = new SonyNpRequests();
#endif		

#if UNITY_PSP2
		// Test the upgradable/trial app flag.
		// Note that this only works with packages, when running PC Hosted skuFlags always equals 'None'.
		UnityEngine.PSVita.Utility.SkuFlags skuf = UnityEngine.PSVita.Utility.skuFlags;
		if (skuf == UnityEngine.PSVita.Utility.SkuFlags.Trial)
		{
			OnScreenLog.Add("Trial Mode, purchase the full app to get extra features.");
		}
#endif
    }

	static public void SetAvatarURL(string url, int index)
	{
		avatars[index].url = url;
		avatars[index].pendingDownload = true;
	}

	IEnumerator DownloadAvatar(int index)
	{
		OnScreenLog.Add(" Downloading avatar image");

		avatars[index].gameObject.GetComponent<GUITexture>().texture = null;
		avatars[index].texture = new Texture2D(4, 4, TextureFormat.DXT1, false);

		// Start a download of the given URL
		var www = new WWW(avatars[index].url);

		// Wait until the download is done
		yield return www;

		// assign the downloaded image to the main texture of the object
		www.LoadImageIntoTexture(avatars[index].texture);

		if (www.bytesDownloaded == 0)
		{
			OnScreenLog.Add(" Error: " + www.error);
		}
		else
		{
			avatars[index].texture.Apply(true, true);	// Release non-GPU texture memory.

			System.Console.WriteLine("w " + avatars[index].texture.width + ", h " + avatars[index].texture.height + ", f " + avatars[index].texture.format);

			if (avatars[index].texture != null)
			{
				avatars[index].gameObject.GetComponent<GUITexture>().texture = avatars[index].texture;
			}
		}
		OnScreenLog.Add(" Done");
	}

	void Update()
	{
		Sony.NP.Main.Update();

        if (sessions.sendingData)
		{
            sendingInterval -= Time.deltaTime;
            if (sendingInterval <= 0)
		    {
                SendSessionData();
                sendingInterval = 1;
        	}
		}

		for (int i = 0; i < avatars.Length; i++)
		{
			if (avatars[i].pendingDownload)
			{
				avatars[i].pendingDownload = false;
				StartCoroutine(DownloadAvatar(i));
			}
		}
	}

    void OnNPInitialized(Sony.NP.Messages.PluginMessage msg)
    {
		npReady = true;

#if UNITY_PS4
		// On PS4 we dont need to request a sign in ... it always happens externally from the application ... i.e. from the home screen
#else
		// If the game relied on online features then it would make sense to automatically sign in to PSN
		// here, but for the sake of a better example we'll use a menu item to sign in.
		//OnScreenLog.Add("Begin sign in");
		//Sony.NP.User.SignIn();        // NP has been fully initialized so it's now safe to sign in etc.
#endif
    }

    void MenuMain()
    {
        menuMain.Update();

		bool signedIn = Sony.NP.User.IsSignedInPSN;

		if(npReady)
		{
			if(!signedIn)
			{
#if UNITY_PS4
				// Indicate that the user is not signed in to PSN.
				menuMain.AddItem("Not Signed In To PSN", false);
#else
				// Add a menu item for signing in to PSN.
				// Note that we could sign in automatically when OnNPInitialized is called if we
				// always require the user to be signed in, i.e. the game relies on online features.
				if (menuMain.AddItem("Sign In To PSN", npReady))
				{
					OnScreenLog.Add("Begin sign in");
					Sony.NP.User.SignIn();
				}
#endif
			}

			if (menuMain.AddItem("Trophies"))
			{
				menuStack.PushMenu(trophies.GetMenu());
			}

			if (menuMain.AddItem("User"))
			{
				menuStack.PushMenu(user.GetMenu());
			}

#if UNITY_PS3
			if (menuMain.AddItem("Utilities & Auth"))
#elif UNITY_PS4
			if (menuMain.AddItem("Utilities & Dialogs"))
#else
			if (menuMain.AddItem("Utilities, Dialogs & Auth"))
#endif
			{
				menuStack.PushMenu(utilities.GetMenu());
			}

			// The following features are only available when the user is signed into PSN.
			if (signedIn)
			{
				if (menuMain.AddItem("Friends & SNS", signedIn))
				{
					menuStack.PushMenu(friends.GetMenu());
				}
				if (menuMain.AddItem("Ranking", signedIn))
				{
					menuStack.PushMenu(ranking.GetMenu());
				}
				if (menuMain.AddItem("Matching", signedIn))
				{
					menuStack.PushMenu(sessions.GetMenu());
				}

				if (menuMain.AddItem("Messaging", signedIn))
				{
					menuStack.PushMenu(messaging.GetMenu());
				}

				if (menuMain.AddItem("Cloud Storage (TUS/TSS)", signedIn))
				{
					menuStack.PushMenu(cloudStorage.GetMenu());
				}

				if (menuMain.AddItem("Commerce", signedIn))
				{
					menuStack.PushMenu(commerce.GetMenu());
				}

	#if UNITY_PS4
				if (menuMain.AddItem("Requests", signedIn))
				{
					menuStack.PushMenu(requests.GetMenu());
				}		
	#endif
			}
			else
			{
	#if UNITY_PS4
				// On PS4 enable the user menu when not signed into PSN so that the current user can be changed.
				if (menuMain.AddItem("User"))
				{
					menuStack.PushMenu(user.GetMenu());
				}
	#endif
			}
		}
	}

	public void OnEnter()
	{
	}

	public void OnExit()
	{
	}

	public void Process(MenuStack stack)
	{
		MenuMain();
	}

	void OnGUI()
    {
		MenuLayout activeMenu = menuStack.GetMenu();
		activeMenu.GetOwner().Process(menuStack);
    }

    void OnLog(Sony.NP.Messages.PluginMessage msg)
    {
        OnScreenLog.Add(msg.Text);
    }

    void OnLogWarning(Sony.NP.Messages.PluginMessage msg)
    {
        OnScreenLog.Add("WARNING: " + msg.Text);
    }

    void OnLogError(Sony.NP.Messages.PluginMessage msg)
    {
        OnScreenLog.Add("ERROR?: " + msg.Text);
    }

    void OnSignedIn(Sony.NP.Messages.PluginMessage msg)
    {
        OnScreenLog.Add(msg.ToString());

		// Determine whether or not the Vita is in flight mode, i.e. signed in but no network connection.
		Sony.NP.ResultCode result = new Sony.NP.ResultCode();
		Sony.NP.User.GetLastSignInError(out result);
		if (result.lastError == Sony.NP.ErrorCode.NP_SIGNED_IN_FLIGHT_MODE)
		{
			OnScreenLog.Add("INFO: Signed in but flight mode is on");
		}
		else if (result.lastError != Sony.NP.ErrorCode.NP_OK)
		{
			OnScreenLog.Add("Error: " + result.className + ": " + result.lastError + ", sce error 0x" + result.lastErrorSCE.ToString("X8"));
		}
    }

    void OnSomeEvent(Sony.NP.Messages.PluginMessage msg)
    {
		OnScreenLog.Add(msg.ToString());
    }

	void OnConnectionDown(Sony.NP.Messages.PluginMessage msg)
	{
		OnScreenLog.Add("Connection Down");

		// Determining the reason for loss of connection...
		//
		// When connection is lost we can call Sony.NP.System.GetLastConnectionError() to obtain
		// the NetCtl error status and reason for loss of connection.
		//
		// ResultCode.lastError will be either NP_ERR_NOT_CONNECTED
		// or NP_ERR_NOT_CONNECTED_FLIGHT_MODE.
		//
		// For the case where ResultCode.lastError == NP_ERR_NOT_CONNECTED further information about
		// the disconnection reason can be inferred from ResultCode.lastErrorSCE which contains
		// the SCE NetCtl error code relating to the disconnection (please refer to SCE SDK docs when
		// interpreting this code).

		// Get the reason for loss of connection...
		Sony.NP.ResultCode result = new Sony.NP.ResultCode();
		Sony.NP.System.GetLastConnectionError(out result);
		OnScreenLog.Add("Reason: " + result.lastError + ", sce error 0x" + result.lastErrorSCE.ToString("X8"));
	}

	void OnSignInError(Sony.NP.Messages.PluginMessage msg)
	{
		Sony.NP.ResultCode result = new Sony.NP.ResultCode();
		Sony.NP.User.GetLastSignInError(out result);
		OnScreenLog.Add(result.className + ": " + result.lastError + ", sce error 0x" + result.lastErrorSCE.ToString("X8"));
	}

    IEnumerator DoJoinSessionFromInvite()
    {
        // Leave the current session.
        if (Sony.NP.Matching.InSession)
        {
            OnScreenLog.Add("Leaving current session...");
            Sony.NP.Matching.LeaveSession();

            // Wait for session exit.
            while (Sony.NP.Matching.SessionIsBusy)
            {
                yield return null;
            }
        }

	

		// In order for member attributes to work, we must initialise them to a value before joining from an invite
		OnScreenLog.Add("Setting invited member attributes...");
		Sony.NP.Matching.ClearSessionAttributes();

		Sony.NP.Matching.SessionAttribute attrib;
		attrib = new Sony.NP.Matching.SessionAttribute();
		attrib.name = "CAR_TYPE";
		attrib.binValue = "CATMOB";
		Sony.NP.Matching.AddSessionAttribute(attrib);
		
		 // Join the session we were invited to.
        OnScreenLog.Add("Joining invited session...");

		Sony.NP.Matching.SessionAttributeInfo passAttribute;
		if ( Sony.NP.Matching.GetSessionInviteSessionAttribute("PASSWORD", out passAttribute) == Sony.NP.ErrorCode.NP_OK)
		{
			OnScreenLog.Add("Found PASSWORD attribute ..." + passAttribute.attributeBinValue);
			if ( passAttribute.attributeBinValue == "YES" )
			{
				// we *HAVE* to pass the password in, as it isn't included in the invite message
				OnScreenLog.Add("Session requires password...");
				Sony.NP.Matching.JoinInvitedSession(sessions.sessionPassword);	
			}
			else
			{
				OnScreenLog.Add("No password required...");
				Sony.NP.Matching.JoinInvitedSession();	
			}
		}
		else
		{
			// Just try to connect without a password
			Sony.NP.Matching.JoinInvitedSession();		
		}

		

        // Reset the menu stack and go to the session menu.
        menuStack.SetMenu(menuMain);
        menuStack.PushMenu(sessions.GetMenu());
    }

    // Received a session invite.
    void OnMessagingSessionInviteRetrieved(Sony.NP.Messages.PluginMessage msg)
    {
        StartCoroutine("DoJoinSessionFromInvite");
    }

        // Received a session invite.
    void OnMessagingSessionInviteReceived(Sony.NP.Messages.PluginMessage msg)
    {
        OnScreenLog.Add(" OnMessagingSessionInviteReceived " );
    }
   
    void OnMessagingSessionInviteAccepted(Sony.NP.Messages.PluginMessage msg)
    {

        OnScreenLog.Add(" OnMessagingSessionInviteAccepted " );
    }

    //
    // Network server callbacks...
    //

    void OnServerInitialized(NetworkPlayer player)
    {
        OnScreenLog.Add("Server Initialized: " + player.ipAddress + ":" + player.port);
        //SpawnPlayer();
        OnScreenLog.Add(" Network.isServer: " + Network.isServer);
        OnScreenLog.Add(" Network.isClient: " + Network.isClient);
        OnScreenLog.Add(" Network.peerType: " + Network.peerType);
    }

    void OnPlayerConnected(NetworkPlayer player)
    {
        OnScreenLog.Add("Player connected from " + player.ipAddress + ":" + player.port);
    }

    void OnPlayerDisconnected(NetworkPlayer player)
    {
        OnScreenLog.Add("Player disconnected " + player);
        Network.RemoveRPCs(player);
        Network.DestroyPlayerObjects(player);
    }


    // Send some data using NetworkView.RPC
    void SendSessionData()
    {
        // Construct some data to attach to the message.
        SharedSessionData data = new SharedSessionData();
        data.id = sendCount++;
        data.text = "Here's some RPC data";
        data.item1 = 2;
        data.item2 = 987654321;
        byte[] bytes = data.WriteToBuffer();
        GetComponent<NetworkView>().RPC("RecieveSharedSessionData", RPCMode.Others, bytes);
    }

    // Receive some data using RPC.
    [RPC]
    void RecieveSharedSessionData(byte[] buffer)
    {
        SharedSessionData data = new SharedSessionData();
        data.ReadFromBuffer(buffer);
        OnScreenLog.Add("RPC Rec: id " + data.id + " - " + data.text + " item1: " + data.item1 + " item2: " + data.item2);
    }

    //
    // Network client callbacks...
    //

    void OnConnectedToServer()
    {
        OnScreenLog.Add("Connected to server...");
        OnScreenLog.Add(" Network.isServer: " + Network.isServer);
        OnScreenLog.Add(" Network.isClient: " + Network.isClient);
        OnScreenLog.Add(" Network.peerType: " + Network.peerType);
        //SpawnPlayer();
    }

    void OnDisconnectedFromServer(NetworkDisconnection info)
    {
        OnScreenLog.Add("Disconnected from server " + info);
        sessions.sendingData = false;
        sendCount = 0;
    }

    void OnFailedToConnect(NetworkConnectionError error)
    {
        OnScreenLog.Add("Could not connect to server: " + error);
    }
}
