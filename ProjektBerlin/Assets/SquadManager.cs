using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SquadManager : MonoBehaviour {

	public int size = 5;

	private float unitDistanceFromCenter = 1.5f;

	public const int MAX_ACTIONS = 2;
	private int _numActions = MAX_ACTIONS;
	private Vector3 positionAtActionStart;
	private bool _midMovement = false;

	public bool midMovement{get{return _midMovement;}}
	public int numActions{get{return _numActions;}}

    //Added lights for showing targeted.
    private GameObject myLight;
    private Light lightPiece;
    private GameObject moveProj;
    private GameObject attackProj;

    public float movementDistance;
    public float attackDistance;
    private float attackRadius;

	//TODO: This might need to become a collection of game objects
	private Transform[] unitTargets;
	private GameObject[] units;

	private const float MAX_UNIT_HEIGHT = 0.5f;
	private const float FLOOR_DISPACEMENT = 1f;
	private Vector3 prevPosition; //used to revert after colliding w/ terrain. 

    private Rigidbody rb;

    //Circle
    float theta_scale = 0.01f;        //Set lower to add more points
    int moveSize; //Total number of points in circle
    LineRenderer lineRenderer;


	// Use this for initialization
	public void init () {

        myLight = new GameObject();
        myLight.transform.position = transform.position;
        lightPiece = myLight.AddComponent<Light>();
        lightPiece.color = Color.red;
        lightPiece.intensity = 8;

        moveProj = GameObject.Find("MoveRadius");
        attackProj = GameObject.Find("AttackRadius");

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

		units [units.Length - 1].GetComponent<UnitManager> ().power = 4;
		units [units.Length - 1].transform.localScale = new Vector3 (1.5f, 1.5f, 1.5f);

		rb = GetComponent<Rigidbody> ();
		if(rb==null) throw new MissingComponentException("Need Rigidbody");
		prevPosition = transform.position;


        //Circle Stuff
        float sizeValue = (2.0f * Mathf.PI) / theta_scale;
        moveSize = (int)sizeValue;
        moveSize++;
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
        lineRenderer.SetWidth(0.02f, 0.02f); //thickness of line
        lineRenderer.SetVertexCount(moveSize);
        lineRenderer.enabled = false;
    }

	//once every physics step
	void FixedUpdate(){

		if (_midMovement && rb.velocity.magnitude>0) {
			float h = Terrain.activeTerrain.SampleHeight (transform.position) + FLOOR_DISPACEMENT;
			transform.position = new Vector3 (transform.position.x, h, transform.position.z);
			if ((positionAtActionStart - transform.position).magnitude >= movementDistance) {
				//endMovement();
				transform.position = prevPosition;
			}
		} else {
			rb.Sleep ();
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
            moveProj.transform.position = new Vector3(transform.position.x,9,transform.position.z);
            moveProj.GetComponent<Projector>().orthographicSize = movementDistance+3;
            moveProj.SetActive(true);
            _midMovement = true;
        }
		else throw new UnityException("Attempted to start an action when squad had none.");
	}
	public void endMovement(){
		if (_midMovement) {
			_midMovement = false;
			_numActions--;
			GetComponent<Rigidbody>().velocity=Vector3.zero;
            moveProj.SetActive(false);
        }
		else throw new UnityException("Attempted to end an actions before starting one.");
	}

	public void skipAction(){
		if (_numActions > 0) {
			_numActions--;
            moveProj.SetActive(false);
        }
		else throw new UnityException("Attempted to skip an action when squad had none.");
	}

	public void resetActions(){
		_numActions = MAX_ACTIONS;
		_midMovement = false;
        moveProj.SetActive(false);
    }

	public void undoMove(){
		if (_midMovement) {
			_midMovement = false;
			transform.position = positionAtActionStart;
            moveProj.SetActive(false);
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

    public List<GameObject> getTargets(Vector3 targetCenter, int activePlayer, int numPlayers, LayerMask detectLayersAttack)
    {

        Vector3 center = gameObject.transform.position;

        attackProj.GetComponent<Projector>().orthographicSize = attackRadius; //Should be set by unit
        attackProj.transform.position = new Vector3(targetCenter.x, 9, targetCenter.z);
        attackProj.GetComponent<Projector>().enabled = true;

        Collider[] hitColliders = Physics.OverlapSphere(center, attackRadius); //Needs to figure out layers
        List<GameObject> targets = new List<GameObject>();

        Debug.Log("Number of objects in range: " + hitColliders.Length);

        int i = 0;
        while (i < hitColliders.Length)
        {
            for (int j = 0; j < numPlayers; j++)
            {
                string playerTarget = "Player" + j.ToString() + "Squad";
                if (j != activePlayer && hitColliders[i].tag == playerTarget)
                {
                    Vector3 myPos = targetCenter;
                    Vector3 targetPos = hitColliders[i].gameObject.transform.position;
                    Vector3 dir = (targetPos - myPos).normalized;
                    float distance = Vector3.Distance(myPos, targetPos);

                    if (!Physics.Raycast(myPos, dir, distance, detectLayersAttack))
                        targets.Add(hitColliders[i].gameObject);
                }
            }
            i++;
        }

        return targets;
    }

	public int getPower(){
		int sum = 0;
		foreach (GameObject u in units) {
			if(u.activeInHierarchy){
				sum += u.GetComponent<UnitManager>().power;
			}
		}
		return sum;
	}

	public void setColor(Color c){
		foreach (GameObject g in units) {
			g.GetComponent<Renderer>().material.color = c;
		}
	}
}