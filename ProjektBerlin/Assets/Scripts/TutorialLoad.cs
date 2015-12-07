using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TutorialLoad : MonoBehaviour
{
    private const float MAX_WAIT_TIME = 5;
    private float timeSpentWaiting = 0;

    private ArrayList allSquads = new ArrayList();

    private string gameName = "com.blackngoldgames.projektberlin";
    GUIStyle customButtonStyle;
    private HostData[] hostData;
    private bool refreshingHostList = false;
    private bool hostDataFound = false;

    private Texture greenText = (Texture)Resources.Load("redTexture");

    public ArrayList getAllSquads()
    {
        return allSquads;
    }

    //void Awake() {
    //	MasterServer.RequestHostList(gameName);
    //}

    // Use this for initialization
    void Start()
    {
        GameObject.Find("MovementCanvas").GetComponent<Canvas>().enabled = false;
        GameObject.Find("CombatCanvas").GetComponent<Canvas>().enabled = false;
        GameObject.Find("FirstActionCanvas").GetComponent<Canvas>().enabled = false;
        GameObject.Find("SecondActionCanvas").GetComponent<Canvas>().enabled = false;
        //GameObject.Find("WaitingCanvas").GetComponent<Canvas>().enabled = false;
        //GameObject.Find("MainMenu").GetComponent<Canvas>().enabled = true;
        spawnPlayer();
    }

    void spawnPlayer()
    {
        GameObject SquadPrefab = (GameObject)Resources.Load("TutorialSquad");
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

        GameObject newSquad = (GameObject)Instantiate(SquadPrefab, new Vector3(0, 1, -70), Quaternion.identity);
        string sTag = "Player0Squad";
        //NetworkView sView = newSquad.GetComponent<NetworkView>();
        //sView.RPC("init", RPCMode.AllBuffered, sTag);

        newSquad.GetComponent<TutorialManager>().setColor(Color.green);
        newSquad.GetComponent<TutorialManager>().init(sTag);
        newSquad.GetComponent<TutorialSquad>().rifleInit();

        allSquads.Add(newSquad);

        GameObject[] spawners = GameObject.FindGameObjectsWithTag("Spawner");
        int x = 0;
        foreach(GameObject spawner in spawners)
        {
            newSquad = (GameObject)Instantiate(SquadPrefab, spawner.transform.position, Quaternion.identity);
            sTag = "Player1Squad";
            newSquad.GetComponent<TutorialManager>().init(sTag);
            newSquad.GetComponent<TutorialSquad>().rifleInit();
            if (x == 0)
                newSquad.GetComponent<TutorialManager>().inCover = true;
            allSquads.Add(newSquad);
            x++;
        }

        Tutorial controllerScript = GetComponent<Tutorial>();
        controllerScript.init();


        controllerScript.setTurn(true);
        controllerScript.updateSquadList("Player0Squad");
        controllerScript.updateUI();
        controllerScript.begin();

    }

    void OnGUI()
    {
        /* OLD STYLE CONNECTION MENU
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
        */
    }

    public bool hasHostData() { return hostData != null && hostData.Length > 0; }
    public void connectToHost(int i)
    {
        Network.Connect(hostData[i]);
    }
    public string[] getHostInfo()
    {
        string[] info = new string[hostData.Length];
        for (int i = 0; i < hostData.Length; i++)
        {
            info[i] = hostData[i].ip[0];
        }
        return info;
    }

    public void refreshHostList()
    {
        MasterServer.RequestHostList(gameName);
        refreshingHostList = true;
        Debug.Log("Getting Host List");
    }

    public void Update()
    {
        //If we have started to look for available servers, look every frame until we find one.
        if (!Controller.getIsRunning() && (hostData == null || refreshingHostList))
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
