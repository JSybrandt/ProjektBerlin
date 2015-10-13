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
		
		if (Network.isServer) {
			for (int i = 0; i < 20; i += 5) {
				GameObject newSquad = (GameObject)Network.Instantiate (SquadPrefab, new Vector3 (i, 1, -70), Quaternion.identity,0);
				newSquad.tag = "Player0Squad";
                NetworkView sView = newSquad.GetComponent<NetworkView>();
                sView.RPC("init", RPCMode.AllBuffered);
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
				newSquad.GetComponent<SquadManager> ().setColor(Color.red);
				allSquads.Add (newSquad);
			}
		}
		if (Network.isClient) {
			for (int i = 0; i < 20; i += 5) {
				GameObject newSquad = (GameObject)Network.Instantiate (SquadPrefab, new Vector3 (i, 1, 60), Quaternion.identity,0);
				newSquad.tag = "Player1Squad";
                NetworkView sView = newSquad.GetComponent<NetworkView>();
                sView.RPC("init", RPCMode.AllBuffered);
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
                newSquad.GetComponent<SquadManager> ().setColor (Color.blue);
				allSquads.Add (newSquad);
			}
		}
		
		Controller controllerScript = GetComponent<Controller>();
		controllerScript.init ();
		if(Network.isServer)
			controllerScript.updateSquadList("Player0Squad");
		if(Network.isClient)
			controllerScript.updateSquadList("Player1Squad");

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
