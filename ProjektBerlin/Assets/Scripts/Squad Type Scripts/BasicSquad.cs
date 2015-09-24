using UnityEngine;
using System.Collections;

public class BasicSquad : MonoBehaviour {

	// Use this for initialization
	void Start () {

    }

    public void init(out Transform[] unitTargets, out GameObject[] units)
    {
        const int size = 5;
        unitTargets = new Transform[size];

        for (int i = 0; i < size; i++)
        {
            unitTargets[i] = new GameObject().transform;
            unitTargets[i].parent = this.transform;
            unitTargets[i].localPosition = Quaternion.Euler(0, i * 360 / size, 0) * Vector3.forward * 1.5f;
        }

        GameObject unitPrefab = (GameObject)Resources.Load("Unit");
        if (unitPrefab == null)
            throw new MissingReferenceException("Failed to find Unit Prefab.");

        units = new GameObject[size];
        for (int i = 0; i < size; i++)
        {
            units[i] = (GameObject)Instantiate(unitPrefab, unitTargets[i].position, Quaternion.identity);
            units[i].transform.position = unitTargets[i].position;
        }

        units[units.Length - 1].GetComponent<UnitManager>().power = 4;
        units[units.Length - 1].transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
    }
	
	// Update is called once per frame
	//void Update () {
	
	//}
}
