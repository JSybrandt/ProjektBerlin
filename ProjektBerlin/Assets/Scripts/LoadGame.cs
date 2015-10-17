using UnityEngine;
using System.Collections;

public class LoadGame : MonoBehaviour
{
	private const float MAX_WAIT_TIME = 5;
	private float timeSpentWaiting = 0;

	private ArrayList allSquads = new ArrayList();

	private string gameName = "com.blackngoldgames.projektberlin";
    GUIStyle customButtonStyle;
    private HostData[] hostData;
    private bool refreshingHostList = false;
    private bool hostDataFound = false;

    public ArrayList getAllSquads(){
		return allSquads;
	}

	void Awake() {
		MasterServer.RequestHostList(gameName);
	}

    // Use this for initialization
    void Start()
    {
		GameObject.Find("MovementCanvas").GetComponent<Canvas>().enabled=false;
		GameObject.Find("CombatCanvas").GetComponent<Canvas>().enabled=false;
		GameObject.Find("FirstActionCanvas").GetComponent<Canvas>().enabled=false;
		GameObject.Find("SecondActionCanvas").GetComponent<Canvas>().enabled=false;
		GameObject.Find ("WaitingCanvas").GetComponent<Canvas> ().enabled=false;
		GameObject.Find ("NetworkCanvas").GetComponent<Canvas> ().enabled = true;

		GameObject p0Base = GameObject.Find ("Team0Base");
		p0Base.GetComponent<BaseManager>().init();


		GameObject p1Base = GameObject.Find ("Team1Base");
		p1Base.GetComponent<BaseManager>().init();

    }

	private void makeGame(){
		NetworkConnectionError err = Network.InitializeServer (2, 25001, !Network.HavePublicAddress());
		if(err == NetworkConnectionError.NoError)
			MasterServer.RegisterHost(gameName,"Projekt Berlin");
	}

	//Messages
	void OnServerInitialized ()
	{
		Debug.Log ("Server initialized");
		spawnPlayer ();
	}
	
	void OnConnectedToServer ()
	{
		Debug.Log ("Connected to server");
		spawnPlayer ();
	}
	
	void OnMasterServerEvent (MasterServerEvent mse)
	{
		if (mse == MasterServerEvent.RegistrationSucceeded) {
			Debug.Log ("Server registered");
		}
	}

	void spawnPlayer(){
		GameObject SquadPrefab = (GameObject)Resources.Load("Squad");
		if (SquadPrefab == null)
			throw new MissingReferenceException("Failed to find squad prefab");

        GameObject netPrefab = (GameObject)Resources.Load("NetLogic");
        if (netPrefab == null)
            throw new MissingReferenceException("Failed to find network prefab");

        if (Network.isServer) {
            GameObject netLogic = (GameObject)Network.Instantiate(netPrefab, new Vector3(0, 0, 0), Quaternion.identity, 0);
            netLogic.GetComponent<NetworkView>().RPC("init", RPCMode.AllBuffered);

            for (int i = 0; i < 20; i += 5) {
				GameObject newSquad = (GameObject)Network.Instantiate (SquadPrefab, new Vector3 (i, 1, -70), Quaternion.identity,0);
				string sTag = "Player0Squad";
                NetworkView sView = newSquad.GetComponent<NetworkView>();
                sView.RPC("init", RPCMode.AllBuffered, sTag,i/5);
                newSquad.GetComponent<SquadManager>().setColor(Color.green);
                //newSquad.GetComponent<SquadManager> ().init ();
                Debug.Log("Server spawning");
				if (i <= 10) {
					//newSquad.AddComponent<BasicSquad> ();
					//newSquad.GetComponent<BasicSquad> ().basicInit ();
                    sView.RPC("basicInit", RPCMode.AllBuffered);
                } else {
					//newSquad.AddComponent<SniperSquad> ();
					//newSquad.GetComponent<SniperSquad> ().sniperInit ();
                    sView.RPC("sniperInit", RPCMode.AllBuffered);
                }
				
				allSquads.Add (newSquad);
			}
		}
		if (Network.isClient) {
			for (int i = 0; i < 20; i += 5) {
				GameObject newSquad = (GameObject)Network.Instantiate (SquadPrefab, new Vector3 (i, 1, 60), Quaternion.identity,0);
                string sTag = "Player1Squad";
                NetworkView sView = newSquad.GetComponent<NetworkView>();
                newSquad.GetComponent<SquadManager>().setColor(Color.green);
                sView.RPC("init", RPCMode.AllBuffered, sTag, i / 5);
                //newSquad.GetComponent<SquadManager> ().init ();
                Debug.Log("Client spawning");
                if (i <= 10)
                {
                    //newSquad.AddComponent<BasicSquad>();
                    //newSquad.GetComponent<BasicSquad> ().basicInit ();
                    sView.RPC("basicInit", RPCMode.AllBuffered);
                }
                else
                {
                    //newSquad.AddComponent<SniperSquad>();
                    //newSquad.GetComponent<SniperSquad> ().sniperInit ();
                    sView.RPC("sniperInit", RPCMode.AllBuffered);
                }
                
				allSquads.Add (newSquad);
			}
		}
		
		Controller controllerScript = GetComponent<Controller>();
		controllerScript.init ();
        if (Network.isServer)
        {
            controllerScript.setTurn(true);
            controllerScript.updateSquadList("Player0Squad");
			controllerScript.updateUI();
        }
        if (Network.isClient)
        {
            controllerScript.setTurn(false);
            controllerScript.updateSquadList("Player1Squad");
            controllerScript.currentPlayersTurn = 1;
			controllerScript.updateUI();
            //controllerScript.nLogicView.RPC("begin", RPCMode.AllBuffered);
        }

	}

    void OnGUI()
    {
        if (customButtonStyle == null)
        {
            customButtonStyle = new GUIStyle(GUI.skin.button);
            customButtonStyle.fontSize = 15;
        }
        if (!Network.isClient && !Network.isServer)
        {
            GUILayout.BeginArea(new Rect(Screen.width * .05f, Screen.height * .05f, Screen.width * 0.2f, Screen.height * 0.2f));
            if (GUILayout.Button("Start Server", customButtonStyle))
            {
                Debug.Log("Starting Server");
                makeGame();
            }

            if (GUILayout.Button("Refresh Host List", customButtonStyle))
            {
                Debug.Log("Refreshing...");
                refreshHostList();
            }

            if (hostDataFound)
            {
                Debug.Log("Host data recieved");
                for (int i = 0; i < hostData.Length; i++)
                {
                    if (GUILayout.Button(hostData[i].gameName, customButtonStyle))
                    {
                        Network.Connect(hostData[i]);
                    }
                }
            }
            GUILayout.EndArea();
        }
    }

    void refreshHostList()
    {
        MasterServer.RequestHostList(gameName);
        refreshingHostList = true;
        Debug.Log("Getting Host List");
    }

    public void Update(){
        //If we have started to look for available servers, look every frame until we find one.
        if (refreshingHostList)
        {
            if (MasterServer.PollHostList().Length > 0)
            {
                refreshingHostList = false;
                hostDataFound = true;
                hostData = MasterServer.PollHostList();
            }
        }
    }
}
