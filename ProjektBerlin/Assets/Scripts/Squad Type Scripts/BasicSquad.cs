using UnityEngine;
using System.Collections;

public class BasicSquad : MonoBehaviour {

	// Use this for initialization
	void Start () {

    }

    public void init()
    {
        SquadManager squad = GetComponent<SquadManager>();

        squad.size = 5;
        squad.attackDistance = 20;
        squad.movementDistance = 50;

        squad.unitTargets = new Transform[squad.size];

        for (int i = 0; i < squad.size; i++)
        {
            squad.unitTargets[i] = new GameObject().transform;
            squad.unitTargets[i].parent = transform;
            squad.unitTargets[i].localPosition = Quaternion.Euler(0, i * 360 / squad.size, 0) * Vector3.forward * 1.5f;
        }

        GameObject unitPrefab = (GameObject)Resources.Load("Unit");
        if (unitPrefab == null)
            throw new MissingReferenceException("Failed to find Unit Prefab.");

        squad.units = new GameObject[squad.size];
        for (int i = 0; i < squad.size; i++)
        {
            squad.units[i] = (GameObject)Instantiate(unitPrefab, squad.unitTargets[i].position, Quaternion.identity);
            squad.units[i].transform.position = squad.unitTargets[i].position;
        }

        squad.units[squad.units.Length - 1].GetComponent<UnitManager>().power = 4;
        squad.units[squad.units.Length - 1].transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
    }
	
	// Update is called once per frame
	//void Update () {
	
	//}
}
