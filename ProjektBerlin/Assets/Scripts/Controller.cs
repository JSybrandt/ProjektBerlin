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

enum AttackType
{
    Basic,
    Unit,
    Squad
}

public class Controller : MonoBehaviour
{

    public const int NUM_PLAYERS = 2;
    [HideInInspector]
    public int numPlayers = NUM_PLAYERS;

    public float turnSpeed = 20;
    public float zoomSpeed = 20;

    private GameObject[] squads;

    //Attacking variables
    [HideInInspector]
    public List<GameObject> targetsInRange;

    //DON'T HIDE LAYER MASKS: Uses inspector.
    public LayerMask detectCover;
    public LayerMask detectPartial;
    public LayerMask detectWall;
    [HideInInspector]
    public GameObject attackProj;
    [HideInInspector]
    public int selectedTargetIndex;

    private int selectedSquadIndex;
    private GameObject selectedLight;

    private Rigidbody selectedRB; //used to move selected squad

    private Vector3 lightOffset = new Vector3(0, 2, 0);

    private TurnStage currentStage = TurnStage.None;
    private AttackType currentAttack = AttackType.Basic;

    [HideInInspector]
    public GameObject marker;

    //FoV
    private Mesh mesh;
    private Material materialFov;
    private const int fovQuality = 15;

    [HideInInspector]
    public int currentPlayersTurn = 0;
    private bool isRoundOver = false;

	private ArrayList allSquads;//taken on init from LoadGame

    //called by loadgame
    public void init()
    {
        Combat.gameLogic = this;

        marker = new GameObject();
        marker.transform.position = Vector3.zero;

		allSquads = GetComponent<LoadGame> ().getAllSquads();

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
                targetsInRange = selectedRB.GetComponent<SquadManager>().getTargets(currentPlayersTurn,NUM_PLAYERS,detectCover,detectPartial);
                Combat.findTargets(selectedRB.gameObject);
                updateUI();
                //Debug.Log("Number of targets within range: " + targetsInRange.Count.ToString());
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

    public void checkStateEndOfAction()
    {
        if (targetsInRange.Count > 0)
        {
            foreach (GameObject target in targetsInRange)
            {
                if(target.activeInHierarchy)
                    target.SendMessage("disableLight");
            }
            Combat.reset();
            currentAttack = AttackType.Basic;
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
   
		updateUI();
    }

    float GetSquadAngle()
    {
        return 90 - Mathf.Rad2Deg * Mathf.Atan2(transform.forward.z, transform.forward.x); // Left handed CW. z = angle 0, x = angle 90
    }

    bool checkRoundComplete()
    {
        foreach (GameObject g in allSquads)
        {
            if (g.GetComponent<SquadManager>().numActions > 0 && g.activeInHierarchy)
                return isRoundOver = false;
        }
        return isRoundOver = true;
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
            if(g.activeInHierarchy)
                g.GetComponent<SquadManager>().resetActions();
        }
        isRoundOver = false;
        currentPlayersTurn = (currentPlayersTurn + 1) % NUM_PLAYERS;
        updateSquadList("Player" + currentPlayersTurn + "Squad");
        currentStage = TurnStage.None;
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
                if (isRoundOver) nextTurn();
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
                if (currentAttack == AttackType.Basic)
                {
                    bool fight = false;
                    Combat.UpdateTarget(selectedRB.GetComponent<SquadManager>(), ref fight);
                    if (fight)   //A
                    {
                        Debug.Log("I shot someone!");
                        Combat.fightTarget(selectedRB.gameObject);
                        getSelectedManager().skipAction();
                        checkStateEndOfAction();
                        updateUI();
                    }
                    if (Input.GetButtonDown("Circle"))  //B
                    {
                        if (getSelectedManager().numActions == 2) currentStage = TurnStage.None;
                        if (getSelectedManager().numActions == 1) currentStage = TurnStage.InBetween;
                        //getSelectedManager().skipAction();
                        checkStateEndOfAction();
                        getMainCamController().setCameraTarget(squads[selectedSquadIndex].transform.position, true);
                    }
                    if (Input.GetButtonUp("Square"))
                    {
                        currentAttack = AttackType.Unit;
                        Combat.reset();
                        attackProj.GetComponent<Projector>().enabled = false;
                        Debug.Log("Unit Ability");
                        getSelectedManager().unitAbility();
                    }
                    if (Input.GetButtonUp("Triangle"))
                    {
                        currentAttack = AttackType.Squad;
                        Combat.reset();
                        attackProj.GetComponent<Projector>().enabled = false;
                        getSelectedManager().squadAbility();
                        Debug.Log("Squad Ability");
                    }
                }
                else if(currentAttack == AttackType.Squad && getSelectedManager() != null) // && Input.GetButtonUp("Triangle")
                {
                    getSelectedManager().squadAbilityUpdate();
                }
                else if(currentAttack == AttackType.Unit && getSelectedManager() != null) // && Input.GetButtonUp("Square")
                {
                    getSelectedManager().unitAbilityUpdate();
                }

                if(Input.GetButtonUp("Circle")) //Reset to basic ability
                {
                    
                    currentAttack = AttackType.Basic;
                    Combat.findTargets(selectedRB.gameObject);
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

	public void updateUI(){
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
