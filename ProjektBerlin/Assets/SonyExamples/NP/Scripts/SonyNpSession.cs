using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using System.Runtime.InteropServices; 

public class SonyNpSession : IScreen
{
	MenuLayout menuSession;
	MenuLayout menuInSessionHosting;
	MenuLayout menuInSessionClient;
	bool matchingIsReady = false;
	int gameDetails = 100;
	int cartype = 0;
	int serverPort = 25001;

	int serverMaxConnections = 32;
	
	int appVersion = 200;		// change this number so you only find games created with this script
	
	public string sessionPassword = "password";
	public bool sendingData = false;
	Sony.NP.Matching.Session[] availableSessions = null;
	Nullable<Sony.NP.Matching.SessionMemberInfo> host = null;       // Session member who is the host.
	Nullable<Sony.NP.Matching.SessionMemberInfo> myself = null;     // Session member who is me.
	Nullable<Sony.NP.Matching.SessionMemberInfo> connected = null;  // Session member that I'm connected to, should = host.
	Sony.NP.Matching.FlagSessionCreate SignallingType = Sony.NP.Matching.FlagSessionCreate.CREATE_SIGNALING_MESH_SESSION;

	public SonyNpSession()
	{
		Initialize();
	}

	public MenuLayout GetMenu()
	{
		return menuSession;
	}

	public void OnEnter()
	{
		int userId = 0;		
		int deviceInId = 0;
		int deviceOutId = 0;
	}

	public void OnExit()
	{

	}

	Sony.NP.ErrorCode ErrorHandler(Sony.NP.ErrorCode errorCode = Sony.NP.ErrorCode.NP_ERR_FAILED)
	{
		if (errorCode != Sony.NP.ErrorCode.NP_OK)
		{
			Sony.NP.ResultCode result = new Sony.NP.ResultCode();
			Sony.NP.Matching.GetLastError(out result);
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
		MenuSession(stack);
	}
	
	public void Initialize()
	{
		menuSession = new MenuLayout(this, 550, 34);
		menuInSessionHosting = new MenuLayout(this, 450, 34);
		menuInSessionClient = new MenuLayout(this, 450, 34);
		
		Sony.NP.Matching.OnCreatedSession += OnMatchingCreatedSession;
		Sony.NP.Matching.OnFoundSessions += OnMatchingFoundSessions;
		Sony.NP.Matching.OnJoinedSession += OnMatchingJoinedSession;
		Sony.NP.Matching.OnJoinInvalidSession += OnMatchingJoinInvalidSession;
		Sony.NP.Matching.OnUpdatedSession += OnMatchingUpdatedSession;
		Sony.NP.Matching.OnLeftSession += OnMatchingLeftSession;
		Sony.NP.Matching.OnSessionDestroyed += OnMatchingSessionDestroyed;
		Sony.NP.Matching.OnKickedOut += OnMatchingKickedOut;
		Sony.NP.Matching.OnSessionError += OnSessionError;
		
		// First Initialize the session attribute definitions.
		Sony.NP.Matching.ClearAttributeDefinitions();
		Sony.NP.Matching.AddAttributeDefinitionInt("LEVEL", Sony.NP.Matching.EnumAttributeType.SESSION_SEARCH_ATTRIBUTE);
		Sony.NP.Matching.AddAttributeDefinitionBin("RACE_TRACK", Sony.NP.Matching.EnumAttributeType.SESSION_EXTERNAL_ATTRIBUTE, Sony.NP.Matching.EnumAttributeMaxSize.SESSION_ATTRIBUTE_MAX_SIZE_12);
		Sony.NP.Matching.AddAttributeDefinitionBin("CAR_TYPE", Sony.NP.Matching.EnumAttributeType.SESSION_MEMBER_ATTRIBUTE, Sony.NP.Matching.EnumAttributeMaxSize.SESSION_ATTRIBUTE_MAX_SIZE_28);
		Sony.NP.Matching.AddAttributeDefinitionInt("GAME_DETAILS", Sony.NP.Matching.EnumAttributeType.SESSION_INTERNAL_ATTRIBUTE);
		Sony.NP.Matching.AddAttributeDefinitionInt("APP_VERSION", Sony.NP.Matching.EnumAttributeType.SESSION_SEARCH_ATTRIBUTE);
		Sony.NP.Matching.AddAttributeDefinitionBin("TEST_BIN_SEARCH", Sony.NP.Matching.EnumAttributeType.SESSION_SEARCH_ATTRIBUTE, Sony.NP.Matching.EnumAttributeMaxSize.SESSION_ATTRIBUTE_MAX_SIZE_60);
		Sony.NP.Matching.AddAttributeDefinitionBin("PASSWORD", Sony.NP.Matching.EnumAttributeType.SESSION_INTERNAL_ATTRIBUTE, Sony.NP.Matching.EnumAttributeMaxSize.SESSION_ATTRIBUTE_MAX_SIZE_12);
		
		ErrorHandler(Sony.NP.Matching.RegisterAttributeDefinitions());	
	}

	public void MenuSession(MenuStack menuStack)
	{
		bool matchingAvailable = Sony.NP.User.IsSignedInPSN;
		bool inSession = Sony.NP.Matching.InSession;

		if (matchingIsReady == false && matchingAvailable)
		{
			matchingIsReady = true;
		}

		if (inSession)
		{
			MenuInSession(menuStack);
		}
		else
		{
			MenuSetupSession(menuStack);
		}
	}

	public void MenuSetupSession(MenuStack menuStack)
	{
		bool matchingAvailable = Sony.NP.User.IsSignedInPSN;
		bool inSession = Sony.NP.Matching.InSession;
		bool sessionBusy = Sony.NP.Matching.SessionIsBusy;

		menuSession.Update();

		if (menuSession.AddItem("Create & Join Session", matchingAvailable && !inSession && !sessionBusy))
		{
			OnScreenLog.Add("Creating session...");

			// First setup the session attributes that the session will be created with.
			Sony.NP.Matching.ClearSessionAttributes();

			Sony.NP.Matching.SessionAttribute attrib;
			attrib = new Sony.NP.Matching.SessionAttribute();
			attrib.name = "APP_VERSION";
			attrib.intValue = appVersion;
			Sony.NP.Matching.AddSessionAttribute(attrib);

			attrib = new Sony.NP.Matching.SessionAttribute();
			attrib.name = "PASSWORD";
			attrib.binValue = "NO";
			Sony.NP.Matching.AddSessionAttribute(attrib);

			attrib = new Sony.NP.Matching.SessionAttribute();
			attrib.name = "CAR_TYPE";
			attrib.binValue = "CATMOB";
			Sony.NP.Matching.AddSessionAttribute(attrib);

			attrib = new Sony.NP.Matching.SessionAttribute();
			attrib.name = "LEVEL";
			attrib.intValue = 1;
			Sony.NP.Matching.AddSessionAttribute(attrib);

			attrib = new Sony.NP.Matching.SessionAttribute();
			attrib.name = "RACE_TRACK";
			attrib.binValue = "TURKEY";
			Sony.NP.Matching.AddSessionAttribute(attrib);

			attrib = new Sony.NP.Matching.SessionAttribute();
			attrib.name = "GAME_DETAILS";
			attrib.intValue = gameDetails;
			Sony.NP.Matching.AddSessionAttribute(attrib);

			attrib = new Sony.NP.Matching.SessionAttribute();
			attrib.name = "TEST_BIN_SEARCH";
			attrib.binValue = "BIN_VALUE";
			Sony.NP.Matching.AddSessionAttribute(attrib);
			
			
			string name = "Test Session";
			int serverID = 0;
			int worldID = 0;
			int numSlots = 8;
			string password = "";
			string sessionStatus = "Toolkit Sample Session";	// Only used on PS4 and PSP2.

			ErrorHandler(Sony.NP.Matching.CreateSession(name, serverID, worldID, numSlots, password,
										SignallingType,		// creation flags
										Sony.NP.Matching.EnumSessionType.SESSION_TYPE_PUBLIC,	// type flags
										sessionStatus));
		}

		// Private sessions MUST have passwords. If using with invites, the password must be known by the person invited, it isn't passed with the invite
		if (menuSession.AddItem("Create & Join Private Session", matchingAvailable && !inSession && !sessionBusy))
		{
			OnScreenLog.Add("Creating private session... password is required");
			
			// First setup the session attributes that the session will be created with.
			Sony.NP.Matching.ClearSessionAttributes();

			Sony.NP.Matching.SessionAttribute attrib;
			attrib = new Sony.NP.Matching.SessionAttribute();
			attrib.name = "APP_VERSION";
			attrib.intValue = appVersion;
			Sony.NP.Matching.AddSessionAttribute(attrib);

			attrib = new Sony.NP.Matching.SessionAttribute();
			attrib.name = "PASSWORD";
			attrib.binValue = "YES";
			Sony.NP.Matching.AddSessionAttribute(attrib);

			attrib = new Sony.NP.Matching.SessionAttribute();
			attrib.name = "CAR_TYPE";
			attrib.binValue = "CATMOB";
			Sony.NP.Matching.AddSessionAttribute(attrib);

			attrib = new Sony.NP.Matching.SessionAttribute();
			attrib.name = "LEVEL";
			attrib.intValue = 1;
			Sony.NP.Matching.AddSessionAttribute(attrib);

			attrib = new Sony.NP.Matching.SessionAttribute();
			attrib.name = "RACE_TRACK";
			attrib.binValue = "TURKEY";
			Sony.NP.Matching.AddSessionAttribute(attrib);

			attrib = new Sony.NP.Matching.SessionAttribute();
			attrib.name = "GAME_DETAILS";
			attrib.intValue = gameDetails;
			Sony.NP.Matching.AddSessionAttribute(attrib);

			attrib = new Sony.NP.Matching.SessionAttribute();
			attrib.name = "TEST_BIN_SEARCH";
			attrib.binValue = "BIN_VALUE";
			Sony.NP.Matching.AddSessionAttribute(attrib);
			
			
			string name = "Test Session";
			int serverID = 0;
			int worldID = 0;
			int numSlots = 8;
			string password = sessionPassword;
			string sessionStatus = "Toolkit Sample Session";	// Only used on PS4 and PSP2.

			ErrorHandler(Sony.NP.Matching.CreateSession(name, serverID, worldID, numSlots, password,
										SignallingType | Sony.NP.Matching.FlagSessionCreate.CREATE_PASSWORD_SESSION,  // creation flags
										Sony.NP.Matching.EnumSessionType.SESSION_TYPE_PRIVATE,	// type flags
										sessionStatus));
		}
		
		// friend sessions also must have passwords
		if (menuSession.AddItem("Create & Join Friend Session", matchingAvailable && !inSession && !sessionBusy))
		{
			OnScreenLog.Add("Creating Friend session...");
			
			// First setup the session attributes that the session will be created with.
			Sony.NP.Matching.ClearSessionAttributes();

			Sony.NP.Matching.SessionAttribute attrib;
			attrib = new Sony.NP.Matching.SessionAttribute();
			attrib.name = "APP_VERSION";
			attrib.intValue = appVersion;
			Sony.NP.Matching.AddSessionAttribute(attrib);

			attrib = new Sony.NP.Matching.SessionAttribute();
			attrib.name = "PASSWORD";
			attrib.binValue = "YES";
			Sony.NP.Matching.AddSessionAttribute(attrib);

			attrib = new Sony.NP.Matching.SessionAttribute();
			attrib.name = "CAR_TYPE";
			attrib.binValue = "CATMOB";
			Sony.NP.Matching.AddSessionAttribute(attrib);

			attrib = new Sony.NP.Matching.SessionAttribute();
			attrib.name = "LEVEL";
			attrib.intValue = 1;
			Sony.NP.Matching.AddSessionAttribute(attrib);

			attrib = new Sony.NP.Matching.SessionAttribute();
			attrib.name = "RACE_TRACK";
			attrib.binValue = "TURKEY";
			Sony.NP.Matching.AddSessionAttribute(attrib);

			attrib = new Sony.NP.Matching.SessionAttribute();
			attrib.name = "GAME_DETAILS";
			attrib.intValue = gameDetails;
			Sony.NP.Matching.AddSessionAttribute(attrib);

			attrib = new Sony.NP.Matching.SessionAttribute();
			attrib.name = "TEST_BIN_SEARCH";
			attrib.binValue = "BIN_VALUE";
			Sony.NP.Matching.AddSessionAttribute(attrib);
			
			
			string name = "Test Session";
			int serverID = 0;
			int worldID = 0;
			int numSlots = 8;
			int numFriendslots = 8;		// all slots for friends
			string password = sessionPassword;
			string sessionStatus = "Toolkit Sample Session";	// Only used on PS4 and PSP2.

			// A friend session uses 0 as the sessionTypeFlag, which means we are required to define the slot information
			ErrorHandler(Sony.NP.Matching.CreateFriendsSession(name, serverID, worldID, numSlots, numFriendslots, password,
										SignallingType | Sony.NP.Matching.FlagSessionCreate.CREATE_PASSWORD_SESSION ,  // creation flags
										sessionStatus));
		}		
		if (menuSession.AddItem("Find Sessions", matchingAvailable && !inSession && !sessionBusy))
		{
			OnScreenLog.Add("Finding sessions...");

			// First setup the session attributes to use for the search.
			Sony.NP.Matching.ClearSessionAttributes();

			Sony.NP.Matching.SessionAttribute attrib;
			attrib = new Sony.NP.Matching.SessionAttribute();
			attrib.name = "APP_VERSION";
			attrib.intValue = appVersion;
			attrib.searchOperator = Sony.NP.Matching.EnumSearchOperators.MATCHING_OPERATOR_EQ;
			Sony.NP.Matching.AddSessionAttribute(attrib);
			
			int serverID = 0;
			int worldID = 0;

			// Start searching.
			ErrorHandler(Sony.NP.Matching.FindSession(serverID, worldID));
		}
		if (menuSession.AddItem("Find Sessions (bin search)", matchingAvailable && !inSession && !sessionBusy))
		{
			OnScreenLog.Add("Finding sessions...");

			// First setup the session attributes to use for the search.
			Sony.NP.Matching.ClearSessionAttributes();
		
			Sony.NP.Matching.SessionAttribute attrib;
			attrib = new Sony.NP.Matching.SessionAttribute();
			attrib.name = "TEST_BIN_SEARCH";
			attrib.binValue = "BIN_VALUE";
			attrib.searchOperator = Sony.NP.Matching.EnumSearchOperators.MATCHING_OPERATOR_EQ;
			Sony.NP.Matching.AddSessionAttribute(attrib);
		
			int serverID = 0;
			int worldID = 0;

			// Start searching for binary ... use regional session flag as workaround to make binary search work (also use EnumAttributeMaxSize.SESSION_ATTRIBUTE_MAX_SIZE_60 in RegisterAttributeDefinitions() )
			// see https://ps4.scedev.net/support/issue/60812
			ErrorHandler(Sony.NP.Matching.FindSession(serverID, worldID, Sony.NP.Matching.FlagSessionSearch.SEARCH_REGIONAL_SESSIONS));
		}

		if (menuSession.AddItem("Find Friend Sessions", matchingAvailable && !inSession && !sessionBusy))
		{
			OnScreenLog.Add("Finding friend sessions...");

			// First setup the session attributes to use for the search.
			Sony.NP.Matching.ClearSessionAttributes();

			Sony.NP.Matching.SessionAttribute attrib;
			attrib = new Sony.NP.Matching.SessionAttribute();
			attrib.name = "APP_VERSION";
			attrib.intValue = appVersion;
			attrib.searchOperator = Sony.NP.Matching.EnumSearchOperators.MATCHING_OPERATOR_EQ;
			Sony.NP.Matching.AddSessionAttribute(attrib);

			int serverID = 0;
			int worldID = 0;

			// Start searching.
			ErrorHandler(Sony.NP.Matching.FindSession(serverID, worldID, Sony.NP.Matching.FlagSessionSearch.SEARCH_FRIENDS_SESSIONS));
		}

		if (menuSession.AddItem("Find Regional Sessions", matchingAvailable && !inSession && !sessionBusy))
		{
			OnScreenLog.Add("Finding friend sessions...");

			// First setup the session attributes to use for the search.
			Sony.NP.Matching.ClearSessionAttributes();

			Sony.NP.Matching.SessionAttribute attrib;
			attrib = new Sony.NP.Matching.SessionAttribute();
			attrib.name = "APP_VERSION";
			attrib.intValue = appVersion;
			attrib.searchOperator = Sony.NP.Matching.EnumSearchOperators.MATCHING_OPERATOR_EQ;
			Sony.NP.Matching.AddSessionAttribute(attrib);

			int serverID = 0;
			int worldID = 0;

			// Start searching.
			ErrorHandler(Sony.NP.Matching.FindSession(serverID, worldID, Sony.NP.Matching.FlagSessionSearch.SEARCH_REGIONAL_SESSIONS));
		}

		/*
			Any of the FlagSessionSearch flags can be combined.
			
                SEARCH_FRIENDS_SESSIONS = (1 << 10),        // This flag specifies that the search is for a friend’s session.
                SEARCH_REGIONAL_SESSIONS = (1 << 12),	    // This flag specifies that the search is for a session that is hosted in your region.
                SEARCH_RECENTLY_MET_SESSIONS = (1 << 14),	// This flag specifies that the search is for a session hosted by users in the Recently Met List.
                SEARCH_RANDOM_SESSIONS = (1 << 18),	        //This flag specifies that the search is for a session with whom a P2P session can be established.
                SEARCH_NAT_RESTRICTED_SESSIONS = (1 << 20),	// This flag specifies that users who cannot establish P2P connections are not allowed to join the session. 
			
		*/
		if (menuSession.AddItem("Find Random Sessions", matchingAvailable && !inSession && !sessionBusy))
		{
			OnScreenLog.Add("Finding sessions in a random order...");

			// First setup the session attributes to use for the search.
			Sony.NP.Matching.ClearSessionAttributes();

			Sony.NP.Matching.SessionAttribute attrib;
			attrib = new Sony.NP.Matching.SessionAttribute();
			attrib.name = "APP_VERSION";
			attrib.intValue = appVersion;
			attrib.searchOperator = Sony.NP.Matching.EnumSearchOperators.MATCHING_OPERATOR_EQ;
			Sony.NP.Matching.AddSessionAttribute(attrib);

			int serverID = 0;
			int worldID = 0;

			// Start searching.
			ErrorHandler(Sony.NP.Matching.FindSession(serverID, worldID, Sony.NP.Matching.FlagSessionSearch.SEARCH_RANDOM_SESSIONS));
		}		
		
		// We would normally present a list of found sessions to the user and let them select which one to join but for the sake of a simple example
		// lets just give the option to join the first one.
		bool foundSessions = (availableSessions != null) && (availableSessions.Length > 0);
		if (menuSession.AddItem("Join 1st Found Session", matchingAvailable && foundSessions && !inSession && !sessionBusy))
		{
			OnScreenLog.Add("Joining PSN session: " + availableSessions[0].sessionInfo.sessionName);

			// First setup the session member attributes.
			Sony.NP.Matching.ClearSessionAttributes();

			Sony.NP.Matching.SessionAttribute attrib;
			attrib = new Sony.NP.Matching.SessionAttribute();
			attrib.name = "CAR_TYPE";
			attrib.binValue = "CATMOB";
			Sony.NP.Matching.AddSessionAttribute(attrib);
			
			ErrorHandler(Sony.NP.Matching.JoinSession(availableSessions[0].sessionInfo.sessionID, sessionPassword));
		}

		if (menuSession.AddBackIndex("Back"))
		{
			menuStack.PopMenu();
		}
	}

	public void MenuInSession(MenuStack menuStack)
	{
		bool matchingAvailable = Sony.NP.User.IsSignedInPSN;
		bool inSession = Sony.NP.Matching.InSession;
		bool sessionBusy = Sony.NP.Matching.SessionIsBusy;
		bool isHosting = Sony.NP.Matching.IsHost;

		MenuLayout layout = isHosting ? menuInSessionHosting : menuInSessionClient;
		layout.Update();

		if (isHosting)
		{
			if (layout.AddItem("Modify Session", matchingAvailable && inSession && !sessionBusy))
			{
				OnScreenLog.Add("Modifying session...");

				// First setup the session attributes to modify.
				Sony.NP.Matching.ClearModifySessionAttributes();

				gameDetails += 100;

				Sony.NP.Matching.ModifySessionAttribute attrib;
				attrib = new Sony.NP.Matching.ModifySessionAttribute();
				attrib.name = "GAME_DETAILS";
				attrib.intValue = gameDetails;
				Sony.NP.Matching.AddModifySessionAttribute(attrib);

				ErrorHandler(Sony.NP.Matching.ModifySession(Sony.NP.Matching.EnumAttributeType.SESSION_INTERNAL_ATTRIBUTE));
			}
		}
		
		if (layout.AddItem("Modify Member Attribute", matchingAvailable && inSession && !sessionBusy))
		{
			OnScreenLog.Add("Modifying Member Attribute...");

			// First setup the session attributes to modify.
			Sony.NP.Matching.ClearModifySessionAttributes();

			

			Sony.NP.Matching.ModifySessionAttribute attrib;
			attrib = new Sony.NP.Matching.ModifySessionAttribute();
			attrib.name = "CAR_TYPE";
			cartype++;
			if (cartype>3) cartype=0;
			switch(cartype)
			{
				case 0:
				attrib.binValue = "CATMOB";			
				break;
				case 1:
				attrib.binValue = "CARTYPE1";			
				break;
				case 2:
				attrib.binValue = "CARTYPE2";			
				break;
				case 3:
				attrib.binValue = "CARTYPE3";			
				break;
				
			}
			
			attrib.intValue = gameDetails;
			Sony.NP.Matching.AddModifySessionAttribute(attrib);

			ErrorHandler(Sony.NP.Matching.ModifySession(Sony.NP.Matching.EnumAttributeType.SESSION_MEMBER_ATTRIBUTE));
		}

		if (sendingData == false)
		{
			// Start sending shared session data via NetworkView.RPC
			if (layout.AddItem("Start Sending Data", matchingAvailable && inSession && !sessionBusy))
			{
				sendingData = true;
			}
		}
		else
		{
			// Start sending shared session data via NetworkView.RPC
			if (layout.AddItem("Stop Sending Data", matchingAvailable && inSession && !sessionBusy))
			{
				sendingData = false;
			}
		}

		if (layout.AddItem("Leave Session", matchingAvailable && inSession && !sessionBusy))
		{
			OnScreenLog.Add("Leaving session...");
			ErrorHandler(Sony.NP.Matching.LeaveSession());
		}

		if (layout.AddItem("List session members", matchingAvailable && inSession && !sessionBusy))
		{
		    Sony.NP.Matching.Session session = Sony.NP.Matching.GetSession();
			Sony.NP.Matching.SessionMemberInfo[] members = session.members;
			for (int i = 0; i < members.Length; i++)
			{
				Sony.NP.Matching.SessionMemberInfo member = members[i];
				string log = i + "/memberId:" + member.memberId 
					+ "/memberFlag:" + member.memberFlag 
					+ "/addr:" + member.addr 
					+ "/natType:" + member.natType
					+ "/port:" + member.port;
				OnScreenLog.Add(log);
			}
		}

		
		if (layout.AddItem("Invite Friend", matchingAvailable && inSession && !sessionBusy))
		{
			OnScreenLog.Add("Invite Friend...");
			ErrorHandler(Sony.NP.Matching.InviteToSession("Invite Test", 8));
		}
		
		
		if (layout.AddBackIndex("Back"))
		{
			menuStack.PopMenu();
		}

		if (Sony.NP.Matching.IsHost)
		{
			NetworkPlayer[] players = Network.connections;
			GUI.Label(new Rect(Screen.width - 200, Screen.height - 200, 200, 64), players.Length.ToString());
		}
	}

	// Find the host in the sessions member list.
	Nullable<Sony.NP.Matching.SessionMemberInfo> FindHostMember(Sony.NP.Matching.Session session)
	{
		Sony.NP.Matching.SessionMemberInfo[] members = session.members;

		for (int i = 0; i < members.Length; i++)
		{
			if((members[i].memberFlag & Sony.NP.Matching.FlagMemberType.MEMBER_OWNER) != 0)
			{
				return members[i];
			}
		}

		return null;
	}

	// Find myself in the sessions member list.
	Nullable<Sony.NP.Matching.SessionMemberInfo> FindSelfMember(Sony.NP.Matching.Session session)
	{
		Sony.NP.Matching.SessionMemberInfo[] members = session.members;

		for (int i = 0; i < members.Length; i++)
		{
			if ((members[i].memberFlag & Sony.NP.Matching.FlagMemberType.MEMBER_MYSELF) != 0)
			{
				return members[i];
			}
		}

		return null;
	}

	// Store member info for the host and client (myself).
	bool InitializeHostAndSelf(Sony.NP.Matching.Session session)
	{
		host = FindHostMember(session);
		if (host == null)
		{
			OnScreenLog.Add("Host member not found!");
			return false;
		}

		myself = FindSelfMember(session);
		if (myself == null)
		{
			OnScreenLog.Add("Self member not found!");
			return false;
		}

		return true;
	}

	// OnFoundSessions event handler; called when a session search has completed.
	void OnMatchingFoundSessions(Sony.NP.Messages.PluginMessage msg)
	{
		Sony.NP.Matching.Session[] sessions = Sony.NP.Matching.GetFoundSessionList();

		OnScreenLog.Add("Found " + sessions.Length + " sessions");
		for (int i = 0; i < sessions.Length; i++)
		{
		    DumpSessionInfo(sessions[i]);
		}

		availableSessions = sessions;
	}

	string IntIPToIPString(int ip)
	{
		int a = ip & 255;
		int b = (ip >> 8) & 255;
		int c = (ip >> 16) & 255;
		int d = (ip >> 24) & 255;
		string sip = a.ToString() + "." + b.ToString() + "." + c.ToString() + "." + d.ToString();
		return sip;
	}

	void DumpSessionInfo(Sony.NP.Matching.Session session)
	{
		Sony.NP.Matching.SessionInfo info = session.sessionInfo;
		Sony.NP.Matching.SessionAttributeInfo[] attributes = session.sessionAttributes;
		Sony.NP.Matching.SessionMemberInfo[] members = session.members;

		OnScreenLog.Add("session: " + info.sessionName
			+ ", " + info.numMembers
			+ ", " + info.maxMembers
			+ ", " + info.openSlots
			+ ", " + info.reservedSlots
			+ ", " + info.worldId
			+ ", " + info.roomId);

		for (int i = 0; i < attributes.Length; i++)
		{
			string attr = " Attribute " + i + ": " + attributes[i].attributeName;
			switch (attributes[i].attributeValueType)
			{
				case Sony.NP.Matching.EnumAttributeValueType.SESSION_ATTRIBUTE_VALUE_INT:
					attr += " = " + attributes[i].attributeIntValue;
					break;

				case Sony.NP.Matching.EnumAttributeValueType.SESSION_ATTRIBUTE_VALUE_BINARY:
					attr += " = " + attributes[i].attributeBinValue;
					break;

				default:
					attr += ", has bad value type";
					break;
			}
			attr += ", " + attributes[i].attributeType;
			OnScreenLog.Add(attr);
		}

		if (members == null)
		{
			return;
		}

		for (int i = 0; i < members.Length; i++)
		{
			OnScreenLog.Add(" Member " + i + ": " + members[i].npOnlineID + ", " +"Type: " + members[i].memberFlag);
			if (members[i].addr != 0)
			{
				OnScreenLog.Add("  IP: " + IntIPToIPString(members[i].addr) + " port " + members[i].port + " 0x" + members[i].port.ToString("X") );
			}
			else
			{
				OnScreenLog.Add("  IP: unknown " );
			}
			attributes = session.memberAttributes[i];
			if (attributes.Length == 0)
			{
				OnScreenLog.Add("  No Member Attributes" );
			}
			for (int j = 0; j < attributes.Length; j++)
			{
				string attr = "  Attribute " + j + ": " + attributes[j].attributeName;
				switch (attributes[j].attributeValueType)
				{
					case Sony.NP.Matching.EnumAttributeValueType.SESSION_ATTRIBUTE_VALUE_INT:
						attr += " = " + attributes[j].attributeIntValue;
						break;

					case Sony.NP.Matching.EnumAttributeValueType.SESSION_ATTRIBUTE_VALUE_BINARY:
						attr += " = " + attributes[j].attributeBinValue;
						break;

					default:
						attr += ", has bad value type";
						break;
				}
				OnScreenLog.Add(attr);
			}
		}
	}

	// OnCreatedSession event handler; called when a session has been created.
	void OnMatchingCreatedSession(Sony.NP.Messages.PluginMessage msg)
	{
		OnScreenLog.Add("Created session...");
		Sony.NP.Matching.Session session = Sony.NP.Matching.GetSession();
		DumpSessionInfo(session);

		if(InitializeHostAndSelf(session) == false)
		{
			OnScreenLog.Add("ERROR: Expected members not found!");
		}

		// Initialize the server.
#if UNITY_PS4		
		UnityEngine.PS4.Networking.enableUDPP2P = true;		
#endif		
		NetworkConnectionError err = Network.InitializeServer(serverMaxConnections, serverPort, false);
		if (err != NetworkConnectionError.NoError)
		{
			OnScreenLog.Add("Server err: " + err);
		}
		else
		{
			OnScreenLog.Add("Started Server");
		}
	}

	// OnJoinedSession event handler; called when a session has been successfully joined.
	void OnMatchingJoinedSession(Sony.NP.Messages.PluginMessage msg)
	{
		OnScreenLog.Add("Joined PSN matching session... waiting on session info in OnMatchingUpdatedSession()");
//        Sony.NP.Matching.Session session = Sony.NP.Matching.GetSession();
//        DumpSessionInfo(session);
	   // Note session member info is not complete (missing connection data) until the system calls OnMatchingUpdatedSession()
		// so we'll defer setting up the network connection and do it in OnMatchingUpdatedSession().
	}

	// OnUpdatedSession event handler; called when the current sessions data has been modified.
	void OnMatchingUpdatedSession(Sony.NP.Messages.PluginMessage msg)
	{
		
		IntPtr sessionInfo = Sony.NP.Matching.GetSessionInformationPtr();
		
		OnScreenLog.Add("Session info updated...");
		Sony.NP.Matching.Session session = Sony.NP.Matching.GetSession();
		DumpSessionInfo(session);

		if (InitializeHostAndSelf(session) == false)
		{
			OnScreenLog.Add("ERROR: Expected members not found!");
		}

		// If not already connected do it now.
		if ((Sony.NP.Matching.IsHost == false) && (connected == null))
		{
			// Connect to the host.
			
			if (host.Value.addr == 0)
			{
				OnScreenLog.Add("Unable to retrieve host IP address");
				ErrorHandler(Sony.NP.Matching.LeaveSession());
				return;
			}
			
			string hostIP = IntIPToIPString(host.Value.addr);
		
#if UNITY_PS4		
			UnityEngine.PS4.Networking.enableUDPP2P = true;
			// if required pass the port value from the host to the networking code
//			UnityEngine.PS4.Networking.NpSignalingPort = host.Value.port;		// Port number actually opened by the recipient as seen from the internet. The port number actually opened by the recipient as seen from the internet can be obtained using the NpMatching library (refer to the "NpMatching2 System Overview" document), for example
#endif
		
			OnScreenLog.Add("Connecting to " + hostIP + ":" + serverPort + " using signalling port:" + host.Value.port);
			
			NetworkConnectionError err = Network.Connect(hostIP, serverPort);		// important to pass the server port in
			if (err != NetworkConnectionError.NoError)
			{
				OnScreenLog.Add("Connection failed: " + err);
			}
			else
			{
				OnScreenLog.Add("Connected to host " + hostIP + " : " + serverPort);
				connected = host;
			}
		}
	}

	// OnJoinInvalidSession event handler; called if an attempt to join a session failed.
	void OnMatchingJoinInvalidSession(Sony.NP.Messages.PluginMessage msg)
	{
		OnScreenLog.Add("Failed to join session...");
		OnScreenLog.Add(" Session search results may be stale.");
	}

	// OnLeftSession event handler; called when the player has left the current session.
	void OnMatchingLeftSession(Sony.NP.Messages.PluginMessage msg)
	{
		OnScreenLog.Add("Left the session");
		
		Network.Disconnect(1);
		host = null;
		connected = null;
		myself = null;
	}

	// OnSessionDestroyed even handler; called when the current session has been destroyed.
	void OnMatchingSessionDestroyed(Sony.NP.Messages.PluginMessage msg)
	{
		OnScreenLog.Add("Session Destroyed");
		
		Network.Disconnect(1);
		host = null;
		connected = null;
		myself = null;
	}

	// OnKickedOut event handler; called if the player has been kicked out of the current session.
	void OnMatchingKickedOut(Sony.NP.Messages.PluginMessage msg)
	{
		OnScreenLog.Add("Kicked out of session");
		
		Network.Disconnect(1);
		host = null;
		connected = null;
		myself = null;
	}

	void OnSessionError(Sony.NP.Messages.PluginMessage msg)
	{
		ErrorHandler();
	}
}
