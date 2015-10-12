using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class LoadGame : NetworkBehaviour
{

	private ArrayList allSquads = new ArrayList();

	public ArrayList getAllSquads(){
		return allSquads;
	}


    // Use this for initialization
    void Start()
    {
        Debug.Log("Started Load Game");

        GameObject SquadPrefab = (GameObject)Resources.Load("Squad");
        if (SquadPrefab == null)
            throw new MissingReferenceException("Failed to find squad prefab");
        GameObject unitPrefab = (GameObject)Resources.Load("Unit");
        GameObject sniperPrefab = (GameObject)Resources.Load("UnitSniper");
        ClientScene.RegisterPrefab(SquadPrefab);
        ClientScene.RegisterPrefab(unitPrefab);
        ClientScene.RegisterPrefab(sniperPrefab);

        if (isServer)
            for (int i = 0; i < 20; i += 5)
            {
                GameObject newSquad = (GameObject)Instantiate(SquadPrefab, new Vector3(i, 1, -70), Quaternion.identity);
                newSquad.tag = "Player0Squad";
                NetworkServer.Spawn(newSquad);
                newSquad.GetComponent<SquadManager>().CmdInit();
                if (i <= 10)
                {
                    newSquad.AddComponent<BasicSquad>();
                    newSquad.GetComponent<BasicSquad>().init();
                }
                else
                {
                    newSquad.AddComponent<SniperSquad>();
                    newSquad.GetComponent<SniperSquad>().init();
                }
                newSquad.GetComponent<SquadManager>().setColor(Color.red);
                
                allSquads.Add(newSquad);
                Debug.Log("Spawned Red Dude");
            }

        if (!isServer)
        {
            
            for (int i = 0; i < 20; i += 5)
            {
                GameObject newSquad = (GameObject)Instantiate(SquadPrefab, new Vector3(i, 1, 60), Quaternion.identity);
                newSquad.tag = "Player1Squad";
                NetworkServer.Spawn(newSquad);

                newSquad.GetComponent<SquadManager>().CmdInit();
                if (i <= 10)
                {
                    newSquad.AddComponent<BasicSquad>();
                    newSquad.GetComponent<BasicSquad>().init();
                }
                else
                {
                    newSquad.AddComponent<SniperSquad>();
                    newSquad.GetComponent<SniperSquad>().init();
                }
                newSquad.GetComponent<SquadManager>().setColor(Color.blue);
                
                allSquads.Add(newSquad);
                Debug.Log("Spawned Blue Dude");
            }
        }

        Controller controllerScript = GetComponent<Controller>();
		controllerScript.init ();
        controllerScript.updateSquadList("Player0Squad");

    }

}
