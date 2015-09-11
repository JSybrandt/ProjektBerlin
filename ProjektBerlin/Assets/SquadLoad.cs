using UnityEngine;
using System.Collections;

public class SquadLoad : MonoBehaviour {

	public int size = 5;

	//TODO: This might need to become a collection of game objects
	private Transform[] unitTargets;
	private GameObject[] units;

	// Use this for initialization
	void Start () {

		unitTargets = new Transform[size];

		for (int i = 0; i < size; i++) {
			unitTargets[i] = new GameObject().transform;
			unitTargets[i].parent = this.transform;
			unitTargets[i].localPosition = Quaternion.Euler(0,i*360/size,0)*Vector3.forward;
		}

		GameObject unitPrefab = (GameObject)Resources.Load("Unit");
		if(unitPrefab == null)
			throw new MissingReferenceException("Failed to find Unit Prefab.");

		units = new GameObject[size];
		for (int i=0; i<size; i++) {
			units[i] = (GameObject)Instantiate(unitPrefab,unitTargets[i].position,Quaternion.identity);
			units[i].transform.position = unitTargets[i].position;
		}
	}
	
	// Update is called once per frame
	void Update () {
		for (int i = 0; i < size; i++) {
			units[i].transform.position = unitTargets[i].position;
			//TODO: This should be targetting instead of teleporting
		}
	
	}
}
