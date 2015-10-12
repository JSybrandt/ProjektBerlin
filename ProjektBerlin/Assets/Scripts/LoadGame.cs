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

        GameObject SquadPrefab = (GameObject)Resources.Load("Squad");
        if (SquadPrefab == null)
            throw new MissingReferenceException("Failed to find squad prefab");
		
        for (int i = 0; i < 20; i += 5)
        {
            GameObject newSquad = (GameObject)Instantiate(SquadPrefab, new Vector3(i, 1, -70), Quaternion.identity);
            newSquad.tag = "Player0Squad";
			newSquad.GetComponent<SquadManager>().init();
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
            NetworkServer.Spawn(newSquad);
            allSquads.Add(newSquad);
        }

		for (int i = 0; i < 20; i += 5)
		{
			GameObject newSquad = (GameObject)Instantiate(SquadPrefab, new Vector3(i, 1, 60), Quaternion.identity);
			newSquad.tag = "Player1Squad";
			newSquad.GetComponent<SquadManager>().init();
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
            NetworkServer.Spawn(newSquad);
            allSquads.Add(newSquad);
        }

        Controller controllerScript = GetComponent<Controller>();
		controllerScript.init ();
        controllerScript.updateSquadList("Player0Squad");

    }

}
