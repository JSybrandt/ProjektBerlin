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
    private List<GameObject> targetsInRange;
    public LayerMask detectLayersAttack;
    private GameObject attackProj;
    private int selectedSquadIndex;
    private int selectedTargetIndex;
    private GameObject selectedLight;

    private Rigidbody selectedRB; //used to move selected squad

    private Text debugText;

    private Vector3 lightOffset = new Vector3(0, 2, 0);

    private TurnStage currentStage = TurnStage.None;

    //FoV
    private Mesh mesh;
    private Material materialFov;
    private const int fovQuality = 15;

    private int currentPlayersTurn = 0;

    //called by loadgame
    public void init()
    {
        selectedLight = GameObject.Find("SelectedLight");
        if (selectedLight == null) throw new MissingReferenceException("Need SelectedLight");
        targetsInRange = new List<GameObject>();

        attackProj = GameObject.Find("AttackRadius");

        GameObject g = GameObject.Find("DebugText");
        if (g == null) throw new MissingReferenceException("Need Debug text");
        debugText = g.GetComponent<Text>();

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

                    //Should be unit layer and squad layer
                    //int mask = 1 << 8 | 1 << 15;

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
        if (squads.Length == 0) throw new UnityException("Failed to find squad.");
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
			do{
				selectedSquadIndex++;
				selectedSquadIndex %= squads.Length;
			}while (!squads[selectedSquadIndex].activeInHierarchy);
            
            if (selectedRB != null) selectedRB.velocity = Vector3.zero;
			Camera.main.GetComponent<CameraController>().setCameraTarget(squads[selectedSquadIndex].transform.position);

        }
        if (Input.GetButtonUp("L1"))
        {

            do{
                selectedSquadIndex--;
                if (selectedSquadIndex < 0) selectedSquadIndex = squads.Length - 1;
			}while (!squads[selectedSquadIndex].activeInHierarchy);

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
            }
        }
        //start start combat
        if (Input.GetAxis("DpadH") == 1)
        {
            if (getSelectedManager().numActions > 0)
            {
                currentStage = TurnStage.Combat;
                targetsInRange = getTargets(selectedRB.position, 20, currentPlayersTurn);
                selectedTargetIndex = 0;
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

	private void selectNextAvalibleSquad (){
		for(selectedSquadIndex=0; selectedSquadIndex<squads.Length; selectedSquadIndex++)
		{
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
        }
        attackProj.GetComponent<Projector>().enabled = false;


        if (getSelectedManager ().numActions == SquadManager.MAX_ACTIONS|| getSelectedManager ().numActions == 0) {
			currentStage = TurnStage.None;
			if (checkTurnComplete ())
				nextTurn ();
			else selectNextAvalibleSquad();
		}
        else
            currentStage = TurnStage.InBetween;

   

    }

    float GetSquadAngle()
    {
        return 90 - Mathf.Rad2Deg * Mathf.Atan2(transform.forward.z, transform.forward.x); // Left handed CW. z = angle 0, x = angle 90
    }

    bool checkTurnComplete()
    {
        foreach (GameObject g in squads)
        {
            if (g.GetComponent<SquadManager>().numActions > 0)
                return false;
        }

        return true;
    }

    //call at end of turn
    void nextTurn()
    {
        foreach (GameObject g in squads)
        {
            g.GetComponent<SquadManager>().resetActions();
        }
        currentPlayersTurn = (currentPlayersTurn + 1) % NUM_PLAYERS;
        updateSquadList("Player" + currentPlayersTurn + "Squad");
        Debug.Log("Player #" + currentPlayersTurn);
    }

    // Update is called once per frame
    void Update()
    {

        debugText.text = "Player:" + currentPlayersTurn;

        debugText.text += " Remaining Actions:" + getSelectedManager().numActions;

        debugText.text += " Current Stage: ";
        switch (currentStage)
        {
            case TurnStage.None: debugText.text += "None"; break;
            case TurnStage.Moving: debugText.text += "Moving"; break;
            case TurnStage.InBetween: debugText.text += "In Between"; break;
            case TurnStage.Combat: debugText.text += "Combat"; break;
        };

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
                    targetsInRange[selectedTargetIndex].SendMessage("disableLight");
                    selectedTargetIndex++;
                    selectedTargetIndex %= targetsInRange.Count;
                    targetsInRange[selectedTargetIndex].SendMessage("enableLight");
                }
                if (Input.GetButtonUp("L1") && targetsInRange.Count > 0)
                {
                    targetsInRange[selectedTargetIndex].SendMessage("disableLight");
                    selectedTargetIndex--;
                    if (selectedTargetIndex < 0) selectedTargetIndex = targetsInRange.Count - 1;
                    targetsInRange[selectedTargetIndex].SendMessage("enableLight");
                }
                if (Input.GetButtonDown("Cross") && targetsInRange.Count > 0)   //A
                {
                    //if (getSelectedManager().numActions == 2) currentStage = TurnStage.None;
                    //if (getSelectedManager().numActions == 1) currentStage = TurnStage.InBetween;
                    Debug.Log("I shot someone!");
                    targetsInRange[selectedTargetIndex].SendMessage("takeDamage", 5);
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

    Mesh setupFoV(float angle_fov = 20, float dist_max = 15)
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

        return mesh;
    }
}
