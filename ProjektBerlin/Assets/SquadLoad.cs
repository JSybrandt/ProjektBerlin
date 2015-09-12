using UnityEngine;
using System.Collections;

public class SquadLoad : MonoBehaviour {

	public int size = 5;

	public float unitDistanceFromCenter = 1;

	//TODO: This might need to become a collection of game objects
	private Transform[] unitTargets;
	private GameObject[] units;

	// Use this for initialization
	void Start () {

		unitTargets = new Transform[size];

		for (int i = 0; i < size; i++) {
			unitTargets[i] = new GameObject().transform;
			unitTargets[i].parent = this.transform;
			unitTargets[i].localPosition = Quaternion.Euler(0,i*360/size,0)*Vector3.forward*unitDistanceFromCenter;
		}

		GameObject unitPrefab = (GameObject)Resources.Load("Unit");
		if(unitPrefab == null)
			throw new MissingReferenceException("Failed to find Unit Prefab.");

		units = new GameObject[size];
		for (int i=0; i<size; i++) {
			units[i] = (GameObject)Instantiate(unitPrefab,unitTargets[i].position,Quaternion.identity);
			units[i].transform.position = unitTargets[i].position;
		}

		Rigidbody rb = GetComponent<Rigidbody> ();
		if(rb==null) throw new MissingComponentException("Need Rigidbody");
		//TODO:this should be dynmaic on mesh size of unit
		CapsuleCollider cc = GetComponent<CapsuleCollider> ();
		if(cc==null) throw new MissingComponentException("Need CapsuleCollider");
		cc.radius = unitDistanceFromCenter;
		cc.height = 1;
		for(int i = 0 ; i < size; i++){
			Physics.IgnoreCollision(cc,units[i].GetComponent<BoxCollider>());
		}
	}
	
	// Update is called once per frame
	void Update () {
		for (int i = 0; i < size; i++) {
			units[i].transform.position = unitTargets[i].position;
			transform.rotation = Quaternion.identity;
			//TODO: This should be targetting instead of teleporting
		}
	
	}
}
