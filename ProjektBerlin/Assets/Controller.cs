using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

enum TurnStage
{
    None,
    Moving,
    Combat,
    InBetween
}

public class Controller : MonoBehaviour
{

    public const int NUM_PLAYERS = 2;

    public float turnSpeed = 20;
    public float zoomSpeed = 20;

    private GameObject[] squads;

    //Attacking variables
    private List<GameObject> targetsInRange;
    public LayerMask detectLayersAttack;
    private GameObject attackProj;
    private int selectedTargetIndex;

    private int selectedSquadIndex;
    private GameObject selectedLight;

    private Rigidbody selectedRB; //used to move selected squad

    private Vector3 lightOffset = new Vector3(0, 2, 0);

    private TurnStage currentStage = TurnStage.None;

    //FoV
    private Mesh mesh;
    private Material materialFov;
    private const int fovQuality = 15;

    private int currentPlayersTurn = 0;

	private ArrayList allSquads;//taken on init from LoadGame

    //called by loadgame
    public void init()
    {
		allSquads = GetComponent<LoadGame> ().getAllSquads ();

		updateUI ();
        selectedLight = GameObject.Find("SelectedLight");
        if (selectedLight == null) throw new MissingReferenceException("Need SelectedLight");
        targetsInRange = new List<GameObject>();
        selectedTargetIndex = -1;

        attackProj = GameObject.Find("AttackRadius");

        //FoV
        materialFov = (Material)Resources.Load("Materials/FoV");
        if (materialFov == null)
            throw new MissingReferenceException("Need Resources/Materials/FoV");

        mesh = new Mesh();
        mesh.vertices = new Vector3[4 * fovQuality];   // Could be of size [2 * quality + 2] if circle segment is continuous
        mesh.triangles = new int[3 * 2 * fovQuality];

        Vector3[] normals = new Vector3[4 * fovQuality];
        Vector2[] uv = new Vector2[4 * fovQuality];

        for (int i = 0; i < uv.Length; i++)
            uv[i] = new Vector2(0, 0);
        for (int i = 0; i < normals.Length; i++)
            normals[i] = new Vector3(0, 1, 0);

        mesh.uv = uv;
        mesh.normals = normals;

		//needed for unit movement to work out, no one should fly away
		Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Squad"),LayerMask.NameToLayer("Squad"));
		Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Unit"),LayerMask.NameToLayer("Squad"));
		Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Unit"),LayerMask.NameToLayer("Unit"));
		
    }

    /// <summary>
    /// function for getting targets within the range of an object
    /// </summary>
    /// <param name="center">origin of attack</param>
    /// <param name="radius">radius of attack</param>
    /// <param name="target">the player being targeted</param>
    /// <param name="layer">The layer mask is a bit shifted number</param>
    /// <returns></returns>
    public List<GameObject> getTargets(Vector3 center, float radius, int activePlayer, int layer = 0)
    {
        if (layer == 0)
        {
            layer = 1 << 12; //Layer 8 being "Squad layer"
            layer = ~layer;
        }

        attackProj.GetComponent<Projector>().orthographicSize = radius+2; //Should be set by unit
        attackProj.transform.position = new Vector3(selectedRB.transform.position.x, 9, selectedRB.transform.position.z);
        attackProj.GetComponent<Projector>().enabled = true;

        Collider[] hitColliders = Physics.OverlapSphere(center, radius); //Needs to figure out layers
        List<GameObject> targets = new List<GameObject>();

        Debug.Log("Number of objects in range: " + hitColliders.Length);

        int i = 0;
        while (i < hitColliders.Length)
        {
            for(int j = 0; j < NUM_PLAYERS; j++)
            {
                string playerTarget = "Player" + j.ToString() + "Squad";
                if (j != activePlayer && hitColliders[i].tag == playerTarget)
                {
                    Vector3 myPos = selectedRB.transform.position;
                    Vector3 targetPos = hitColliders[i].gameObject.transform.position;
                    Vector3 dir = (targetPos - myPos).normalized;
                    float distance = Vector3.Distance(myPos, targetPos);

                    if (!Physics.Raycast(myPos,dir,distance, detectLayersAttack))
                        targets.Add(hitColliders[i].gameObject);
                }
            }
            i++;
        }

        return targets;

    }

    public void updateSquadList(string tag)
    {
        squads = GameObject.FindGameObjectsWithTag(tag);
        selectedSquadIndex = 0;
        selectNextAvailableSquad();
        if (squads.Length == 0) throw new UnityException("Failed to find squad.");
		getMainCamController ().setCameraTarget (squads [selectedSquadIndex].transform.position);
        setLight();
    }

    private void setLight()
    {
        selectedLight.transform.position = squads[selectedSquadIndex].transform.position + lightOffset;
    }

    private void checkChangeSquad()
    {
        if (Input.GetButtonUp("R1"))
        {
            selectNextAvailableSquad();
            
            if (selectedRB != null) selectedRB.velocity = Vector3.zero;
			Camera.main.GetComponent<CameraController>().setCameraTarget(squads[selectedSquadIndex].transform.position);

        }
        if (Input.GetButtonUp("L1"))
        {

            selectPrevAvailableSquad();

            if (selectedRB != null) selectedRB.velocity = Vector3.zero;
			Camera.main.GetComponent<CameraController>().setCameraTarget(squads[selectedSquadIndex].transform.position);

        }

        selectedRB = squads[selectedSquadIndex].GetComponent<Rigidbody>();

        if (selectedRB != null) selectedRB.velocity = Vector3.zero;

    }

	private CameraController getMainCamController(){
		return Camera.main.GetComponent<CameraController> ();
	}

    private SquadManager getSelectedManager()
    {
        return squads[selectedSquadIndex].GetComponent<SquadManager>();
    }

    private void checkNewAction()
    {
        //start move
        if (Input.GetAxis("DpadH") == -1)
        {
            if (getSelectedManager().numActions > 0)
            {
                currentStage = TurnStage.Moving;
                getSelectedManager().startMovement();
				updateUI();
            }
        }
        //start start combat
        if (Input.GetAxis("DpadH") == 1)
        {
            if (getSelectedManager().numActions > 0)
            {
                currentStage = TurnStage.Combat;
                targetsInRange = getTargets(selectedRB.position, selectedRB.GetComponent<SquadManager>().attackDistance, currentPlayersTurn);
				updateUI();
                Debug.Log("Number of targets within range: " + targetsInRange.Count.ToString());
            }
        }
        //skip
        if (Input.GetAxis("DpadV") == -1)
        {
            if (getSelectedManager().numActions > 0)
            {
                currentStage = TurnStage.InBetween;
                getSelectedManager().skipAction();
            }
            if (getSelectedManager().numActions == 0)
            {
                currentStage = TurnStage.None;
            }
			checkStateEndOfAction();
        }

    }

	private void selectNextAvailableSquad(){
		for(int i =0; i<squads.Length; i++)
		{
			selectedSquadIndex++;
			selectedSquadIndex%=squads.Length;
			if(squads[selectedSquadIndex].activeInHierarchy && getSelectedManager().numActions>0)
			{
				getMainCamController().setCameraTarget(squads[selectedSquadIndex].transform.position);
				break;
			}
		}
	}
	private void selectPrevAvailableSquad(){
		for(int i =0; i<squads.Length; i++)
		{
			selectedSquadIndex--;
			if(selectedSquadIndex<0)selectedSquadIndex=squads.Length-1;
			if(squads[selectedSquadIndex].activeInHierarchy && getSelectedManager().numActions>0)
			{
				getMainCamController().setCameraTarget(squads[selectedSquadIndex].transform.position);
				break;
			}
		}
	}

    private void checkStateEndOfAction()
    {
        if (targetsInRange.Count > 0)
        {
            foreach (GameObject target in targetsInRange)
            {
                if(target.activeInHierarchy)
                    target.SendMessage("disableLight");
            }
            targetsInRange.Clear();
            selectedTargetIndex = -1;
        }
        attackProj.GetComponent<Projector>().enabled = false;

		if (GameObject.FindGameObjectsWithTag ("Player" + ((currentPlayersTurn + 1) % NUM_PLAYERS) + "Squad").Length == 0) {
			Debug.Log("GAME OVER! PLAYER "+ (currentPlayersTurn+1) +" victory!");
			Application.Quit();
		}

        if (getSelectedManager ().numActions == SquadManager.MAX_ACTIONS) {
			currentStage = TurnStage.None;
		}
		else if(getSelectedManager ().numActions == 0) {
			nextTurn();
		}
        else
            currentStage = TurnStage.InBetween;

   
		updateUI ();

    }

    float GetSquadAngle()
    {
        return 90 - Mathf.Rad2Deg * Mathf.Atan2(transform.forward.z, transform.forward.x); // Left handed CW. z = angle 0, x = angle 90
    }

    bool checkRoundComplete()
    {
        foreach (GameObject g in allSquads)
        {
            if (g.GetComponent<SquadManager>().numActions > 0)
                return false;
        }
        return true;
    }

	void nextTurn(){
		if (checkRoundComplete ())
			nextRound ();
		else {
			currentPlayersTurn = (currentPlayersTurn + 1) % NUM_PLAYERS;
			updateSquadList ("Player" + currentPlayersTurn + "Squad");
            selectNextAvailableSquad();
			currentStage=TurnStage.None;
		}
	}

    //call at end of turn
    void nextRound()
    {
        foreach (GameObject g in allSquads)
        {
            g.GetComponent<SquadManager>().resetActions();
        }
        currentPlayersTurn = (currentPlayersTurn + 1) % NUM_PLAYERS;
        updateSquadList("Player" + currentPlayersTurn + "Squad");
    }

	int calculateDamage(int power){
		int hits = 0;
		for (int i =0; i< power; i++) {
			if(Random.Range(1,6)<=2)hits++;
		}
		int damage = 0;
		for (int i = 0; i< hits; i++) {
			if(Random.Range(1,6)<=4)damage++;
		}

		Debug.Log ("Attack:" + power + " Hits:" + hits + " Damage:" + damage);
		return damage;
	}

    // Update is called once per frame
    void Update()
    {
	
        if (squads.Length > 0)
        {


            if (currentStage == TurnStage.None)
            {
                //skip turn button
                if (Input.GetButtonDown("Select")) { nextTurn(); }
                checkChangeSquad();
                checkNewAction();
            }
            else if (currentStage == TurnStage.InBetween)
            {
                checkNewAction();
            }
            else if (currentStage == TurnStage.Moving)
            {

                //if the squad is no longer moving (triggered if max distance is met)
                if (!getSelectedManager().midMovement)
                {
                    //if we have another action
                    if (getSelectedManager().numActions > 0)
                    {
                        currentStage = TurnStage.InBetween;
                    }
                    else currentStage = TurnStage.None;
                }
                //user undo
                else if (Input.GetButtonDown("Circle")) //B
                {
                    getSelectedManager().undoMove();
                    checkStateEndOfAction();
					getMainCamController().setCameraTarget(squads[selectedSquadIndex].transform.position);
                }
                //user ends early
                else if (Input.GetButtonDown("Cross"))  //A
                {
                    getSelectedManager().endMovement();
                    checkStateEndOfAction();
                }
                else
                {
                    selectedRB = squads[selectedSquadIndex].GetComponent<Rigidbody>();
                    float v = Input.GetAxis("JoystickLV");
                    float h = Input.GetAxis("JoystickLH");
                    selectedRB.velocity = (Quaternion.Euler(0, getMainCamController().angle, 0) * new Vector3(h, 0, v).normalized) * 20;
					getMainCamController().setCameraTarget(squads[selectedSquadIndex].transform.position,true);
                }
            }
            else if (currentStage == TurnStage.Combat)
            {
                //TODO: enable combat in squad
                //skip

                if (Input.GetAxis("DpadV") == -1)
                {
                    getSelectedManager().skipAction();
                    checkStateEndOfAction();
                }
                if (Input.GetButtonUp("R1") && targetsInRange.Count > 0)
                {
                    attackProj.GetComponent<Projector>().enabled = false;
                    if (selectedTargetIndex >= 0)
                        targetsInRange[selectedTargetIndex].SendMessage("disableLight");

                    selectedTargetIndex++;
                    selectedTargetIndex %= targetsInRange.Count;
                    targetsInRange[selectedTargetIndex].SendMessage("enableLight");


                }
                if (Input.GetButtonUp("L1") && targetsInRange.Count > 0)
                {
                    attackProj.GetComponent<Projector>().enabled = false;
                    if (selectedTargetIndex >= 0)
                        targetsInRange[selectedTargetIndex].SendMessage("disableLight");
                    else
                        selectedTargetIndex = 0;

                    selectedTargetIndex--;
                    if (selectedTargetIndex < 0) selectedTargetIndex = targetsInRange.Count - 1;
                    targetsInRange[selectedTargetIndex].SendMessage("enableLight");
                }
                if (Input.GetButtonDown("Cross") && targetsInRange.Count > 0 && selectedTargetIndex >= 0)   //A
                {
                    //if (getSelectedManager().numActions == 2) currentStage = TurnStage.None;
                    //if (getSelectedManager().numActions == 1) currentStage = TurnStage.InBetween;
                    Debug.Log("I shot someone!");
					int power = getSelectedManager().getPower();
					targetsInRange[selectedTargetIndex].SendMessage("takeDamage",calculateDamage(power));
                    getSelectedManager().skipAction();
                    checkStateEndOfAction();

                }
                if (Input.GetButtonDown("Circle"))  //B
                {
                    if (getSelectedManager().numActions == 2) currentStage = TurnStage.None;
                    if (getSelectedManager().numActions == 1) currentStage = TurnStage.InBetween;
                    //getSelectedManager().skipAction();
                    checkStateEndOfAction();
                }
                else
                {
					//this is where aiming would happen

                }
            }
            setLight();
        }
    }

    private void drawFoV(Quaternion fovRotation, float angle_fov = 20, float dist_max = 15)
    {
        const float dist_min = 5.0f;

        float angle_lookat = GetSquadAngle();

        float angle_start = angle_lookat - angle_fov;
        float angle_end = angle_lookat + angle_fov;
        float angle_delta = (angle_end - angle_start) / fovQuality;

        float angle_curr = angle_start;
        float angle_next = angle_start + angle_delta;

        Vector3 pos_curr_min = Vector3.zero;
        Vector3 pos_curr_max = Vector3.zero;

        Vector3 pos_next_min = Vector3.zero;
        Vector3 pos_next_max = Vector3.zero;

        Vector3[] vertices = new Vector3[4 * fovQuality];   // Could be of size [2 * quality + 2] if circle segment is continuous
        int[] triangles = new int[3 * 2 * fovQuality];

        for (int i = 0; i < fovQuality; i++)
        {
            Vector3 sphere_curr = new Vector3(
            Mathf.Sin(Mathf.Deg2Rad * (angle_curr)), 0,   // Left handed CW
            Mathf.Cos(Mathf.Deg2Rad * (angle_curr)));

            Vector3 sphere_next = new Vector3(
            Mathf.Sin(Mathf.Deg2Rad * (angle_next)), 0,
            Mathf.Cos(Mathf.Deg2Rad * (angle_next)));

            pos_curr_min = transform.position + sphere_curr * dist_min;
            pos_curr_max = transform.position + sphere_curr * dist_max;

            pos_next_min = transform.position + sphere_next * dist_min;
            pos_next_max = transform.position + sphere_next * dist_max;

            int a = 4 * i;
            int b = 4 * i + 1;
            int c = 4 * i + 2;
            int d = 4 * i + 3;

            vertices[a] = pos_curr_min;
            vertices[b] = pos_curr_max;
            vertices[c] = pos_next_max;
            vertices[d] = pos_next_min;

            triangles[6 * i] = a;       // Triangle1: abc
            triangles[6 * i + 1] = b;
            triangles[6 * i + 2] = c;
            triangles[6 * i + 3] = c;   // Triangle2: cda
            triangles[6 * i + 4] = d;
            triangles[6 * i + 5] = a;

            angle_curr += angle_delta;
            angle_next += angle_delta;

        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        Graphics.DrawMesh(mesh, squads[selectedSquadIndex].transform.position, fovRotation, materialFov, 0);
    }

	private void updateUI(){
		Canvas movement = GameObject.Find("MovementCanvas").GetComponent<Canvas>();
		Canvas combat = GameObject.Find("CombatCanvas").GetComponent<Canvas>();
		Canvas first = GameObject.Find("FirstActionCanvas").GetComponent<Canvas>();
		Canvas second = GameObject.Find("SecondActionCanvas").GetComponent<Canvas>();

		movement.enabled = combat.enabled = first.enabled = second.enabled = false;

		switch (currentStage) {
		case TurnStage.Combat: combat.enabled=true; break;
		case TurnStage.InBetween: second.enabled=true; break;
		case TurnStage.Moving: movement.enabled=true; break;
		case TurnStage.None:first.enabled=true; break;
		}

	}

}
