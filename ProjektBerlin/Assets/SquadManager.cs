using UnityEngine;
using System.Collections;

public class SquadManager : MonoBehaviour {

	public int size = 5;

	public float unitDistanceFromCenter = 1;

	public const int MAX_ACTIONS = 2;
	private int _numActions = MAX_ACTIONS;
	private Vector3 positionAtActionStart;
	private bool _midMovement = false;

	public bool midMovement{get{return _midMovement;}}
	public int numActions{get{return _numActions;}}

	public float movementDistance = 5;

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
		if (_midMovement) {
			if((positionAtActionStart-transform.position).magnitude >= movementDistance){
				endMovement();
			}
		}
	}

	public void startMovement(){
		if (numActions > 0) {
			positionAtActionStart = transform.position;
			_midMovement = true;
		}
		else throw new UnityException("Attempted to start an action when squad had none.");
	}
	public void endMovement(){
		if (_midMovement) {
			_midMovement = false;
			_numActions--;
			GetComponent<Rigidbody>().velocity=Vector3.zero;
		}
		else throw new UnityException("Attempted to end an actions before starting one.");
	}

	public void skipAction(){
		if (_numActions > 0) {
			_numActions--;
		}
		else throw new UnityException("Attempted to skip an action when squad had none.");
	}

	public void resetActions(){
		_numActions = MAX_ACTIONS;
		_midMovement = false;
	}

	public void undoMove(){
		if (_midMovement) {
			_midMovement = false;
			transform.position = positionAtActionStart;
		} 
		else throw new UnityException ("Attempted to undo a move when squad had not moved");
	}



}
