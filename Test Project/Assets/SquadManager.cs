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

    //Added lights for showing targeted.
    private GameObject myLight;
    private Light lightPiece;

	public float movementDistance = 500;

	//TODO: This might need to become a collection of game objects
	private Transform[] unitTargets;
	private GameObject[] units;

	private const float MAX_UNIT_HEIGHT = 2;
	private const float FLOOR_DISPACEMENT = 0.7f;
	private Vector3 prevPosition; //used to revert after colliding w/ terrain. 

	private Rigidbody rb;

	// Use this for initialization
	void Start () {

        myLight = new GameObject();
        myLight.transform.position = transform.position;
        lightPiece = myLight.AddComponent<Light>();
        lightPiece.color = Color.red;
        lightPiece.intensity = 8;

        lightPiece.enabled = false;

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

		rb = GetComponent<Rigidbody> ();
		if(rb==null) throw new MissingComponentException("Need Rigidbody");
		prevPosition = transform.position;
	}

	//once every physics step
	void FixedUpdate(){
		if (_midMovement && rb.velocity.magnitude>0) {
			RaycastHit hit=new RaycastHit();
			if(Physics.Raycast(transform.position,Vector3.down, out hit, 100f,LayerMask.NameToLayer("terrain"))){
				if(hit.point.y > MAX_UNIT_HEIGHT)
					transform.position = prevPosition;
				else
					transform.position = new Vector3(transform.position.x,hit.point.y+FLOOR_DISPACEMENT,transform.position.z);
			}
			if((positionAtActionStart-transform.position).magnitude >= movementDistance){
				endMovement();
			}
		}
		prevPosition = transform.position;
	}

	// Update is called once per frame
	void Update () {
		for (int i = 0; i < size; i++) {
			units[i].transform.position = unitTargets[i].position;
			transform.rotation = Quaternion.identity;
			//TODO: This should be targetting instead of teleporting
		}


        //Updates associated light
        myLight.transform.position = transform.position;

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

    public void takeDamage(int numUnitsKilled)
    {
        Debug.Log("I'm dieing!");

       //Do some damage
       int remainingToKill = numUnitsKilled;

        foreach (GameObject unit in units)
        {
            if (remainingToKill <= 0)
                break;
            else if(unit.activeInHierarchy)
            {
                unit.SetActive(false);  //Might not be correct
                remainingToKill--;
            }
        }

        if (isDead())
        {
            //Disable this squad
            gameObject.SetActive(false);
            lightPiece.enabled = false;

        }
    }

    //Checks all units of the squad to see if they are active or not.
    public bool isDead()
    {
        foreach (GameObject unit in units)
        {
            if (unit.activeInHierarchy)
            {
                return false;
            }
        }

        return true;
    }

    public void enableLight()
    {
        lightPiece.enabled = true;
    }

    public void disableLight()
    {
        lightPiece.enabled = false;
    }
}