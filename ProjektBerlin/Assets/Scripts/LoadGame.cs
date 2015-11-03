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

        GameObject movePrefab = (GameObject)Resources.Load("MoveRadius");
        movePrefab.name = "MoveRadius";
        if (movePrefab == null)
            throw new MissingReferenceException("Failed to find move prefab");
        GameObject spawnedMove = (GameObject)Instantiate(movePrefab, Vector3.zero, Quaternion.Euler(90, 0, 0));
        spawnedMove.name = movePrefab.name;

        GameObject attackPrefab = (GameObject)Resources.Load("AttackRadius");

        if (attackPrefab == null)
            throw new MissingReferenceException("Failed to find attack prefab");
        GameObject spawnedAttack = (GameObject)Instantiate(attackPrefab, Vector3.zero, Quaternion.Euler(90, 0, 0));
        spawnedAttack.name = attackPrefab.name;

        GameObject changePrefab = (GameObject)Resources.Load("ChangeUnit");

        if (changePrefab == null)
            throw new MissingReferenceException("Failed to find change prefab");
        GameObject changeProj = (GameObject)Instantiate(changePrefab, Vector3.zero, Quaternion.Euler(90, 0, 0));
        changeProj.name = changePrefab.name;

        if (Network.isServer) {
            GameObject netLogic = (GameObject)Network.Instantiate(netPrefab, new Vector3(0, 0, 0), Quaternion.identity, 0);
            netLogic.GetComponent<NetworkView>().RPC("init", RPCMode.AllBuffered);

            for (int i = 0; i < 20; i += 5) {
				GameObject newSquad = (GameObject)Network.Instantiate (SquadPrefab, new Vector3 (i, 1, -70), Quaternion.identity,0);
				string sTag = "Player0Squad";
                NetworkView sView = newSquad.GetComponent<NetworkView>();
                sView.RPC("init", RPCMode.AllBuffered, sTag);

                newSquad.GetComponent<SquadManager>().setColor(Color.green);

                Debug.Log("Server spawning");
                if (i <= 5)
                    sView.RPC("rifleInit", RPCMode.AllBuffered);
                else if (i == 10)
                    sView.RPC("sniperInit", RPCMode.AllBuffered);
                else
                    sView.RPC("shotgunInit", RPCMode.AllBuffered);

                allSquads.Add (newSquad);
			}
		}
		if (Network.isClient) {
			for (int i = 0; i < 20; i += 5) {
				GameObject newSquad = (GameObject)Network.Instantiate (SquadPrefab, new Vector3 (i, 1, 60), Quaternion.identity,0);
                string sTag = "Player1Squad";
                NetworkView sView = newSquad.GetComponent<NetworkView>();
                newSquad.GetComponent<SquadManager>().setColor(Color.green);
                sView.RPC("init", RPCMode.AllBuffered, sTag);

                Debug.Log("Client spawning");
                if (i <= 5)
                    sView.RPC("rifleInit", RPCMode.AllBuffered);
                else if(i == 10)
                    sView.RPC("sniperInit", RPCMode.AllBuffered);
                else
                    sView.RPC("shotgunInit", RPCMode.AllBuffered);
                
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
            if (GUILayout.Button("Start Server", customButtonStyle) || Input.GetButtonUp("Triangle"))
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
                Debug.Log("Host data recieved");
                hostData = MasterServer.PollHostList();
            }
        }
    }
}
