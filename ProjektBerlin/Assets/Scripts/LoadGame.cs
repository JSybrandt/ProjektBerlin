using UnityEngine;
using System.Collections;

public class LoadGame : MonoBehaviour
{
	private const float MAX_WAIT_TIME = 5;
	private float timeSpentWaiting = 0;

	private ArrayList allSquads = new ArrayList();

	private string gameName = "com.blackngoldgames.projektberlin";

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
				newSquad.GetComponent<SquadManager> ().init ();
				if (i <= 10) {
					newSquad.AddComponent<BasicSquad> ();
					newSquad.GetComponent<BasicSquad> ().init ();
				} else {
					newSquad.AddComponent<SniperSquad> ();
					newSquad.GetComponent<SniperSquad> ().init ();
				}
				newSquad.GetComponent<SquadManager> ().setColor (Color.red);
				allSquads.Add (newSquad);
			}
		}
		if (Network.isClient) {
			for (int i = 0; i < 20; i += 5) {
				GameObject newSquad = (GameObject)Network.Instantiate (SquadPrefab, new Vector3 (i, 1, 60), Quaternion.identity,0);
				newSquad.tag = "Player1Squad";
				newSquad.GetComponent<SquadManager> ().init ();
				if (i <= 10) {
					newSquad.AddComponent<BasicSquad> ();
					newSquad.GetComponent<BasicSquad> ().init ();
				} else {
					newSquad.AddComponent<SniperSquad> ();
					newSquad.GetComponent<SniperSquad> ().init ();
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

	/*void OnGUI() {
		HostData[] data = MasterServer.PollHostList();
		// Go through all the hosts in the host list
		foreach (var element in data)
		{
			GUILayout.BeginHorizontal();    
			var name = element.gameName + " " + element.connectedPlayers + " / " + element.playerLimit;
			GUILayout.Label(name);  
			GUILayout.Space(5);
			string hostInfo;
			hostInfo = "[";
			foreach (var host in element.ip)
				hostInfo = hostInfo + host + ":" + element.port + " ";
			hostInfo = hostInfo + "]";
			GUILayout.Label(hostInfo);  
			GUILayout.Space(5);
			GUILayout.Label(element.comment);
			GUILayout.Space(5);
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Connect"))
			{
				// Connect to HostData struct, internally the correct method is used (GUID when using NAT).
				Network.Connect(element);           
			}
			GUILayout.EndHorizontal();  
		}
	}

*/

	public void Update(){
		if (!Network.isServer && !Network.isClient) {
			timeSpentWaiting+=Time.deltaTime;
			if(timeSpentWaiting > MAX_WAIT_TIME){
				makeGame();
			}
			else{
				if(MasterServer.PollHostList().Length>0){
					foreach (HostData d in MasterServer.PollHostList()){
						Debug.Log(d.gameName);
						Network.Connect(d);
						break;
					}
				}
				else{
					MasterServer.RequestHostList (gameName);
				}
			}

		}
	}
}
