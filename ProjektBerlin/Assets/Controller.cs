using UnityEngine;
using UnityEngine.UI;
using System.Collections;

enum TurnStage{
	None,
	Moving,
	Combat,
	InBetween
}

public class Controller : MonoBehaviour {

	public const int NUM_PLAYERS = 2;

	public float turnSpeed = 20;
	public float zoomSpeed=20;

	private GameObject[] squads;
	private int selectedSquadIndex;
	private GameObject selectedLight;
	private Rigidbody selectedRB; //used to move selected squad
	private float theta;
	private float distance;
	private Text debugText;

	private Vector3 defaultCameraOffset = new Vector3(0,10,-5);
	private Vector3 cameraTarget = new Vector3(0,0,0);
	private Vector3 lightOffset = new Vector3(0,2,0);

	private TurnStage currentStage = TurnStage.None;

    private Mesh mesh;
    private Material materialFov;
    private MeshCollider fovCollider;
	private const int fovQuality=15;

	private int currentPlayersTurn = 0;

	//called by loadgame
    public void init(){
		selectedLight = GameObject.Find ("SelectedLight");
		if(selectedLight==null) throw new MissingReferenceException("Need SelectedLight");

		GameObject g = GameObject.Find("DebugText");
		if(g==null) throw new MissingReferenceException("Need Debug text");
		debugText = g.GetComponent<Text> ();

		distance = defaultCameraOffset.y;

        //FoV
		materialFov = (Material) Resources.Load("Materials/FoV");
		if (materialFov == null)
			throw new MissingReferenceException ("Need Resources/Materials/FoV");

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

        fovCollider = new MeshCollider();
        fovCollider.sharedMesh = mesh;
    }

	public void updateSquadList(string tag){
		squads = GameObject.FindGameObjectsWithTag (tag);
		selectedSquadIndex = 0;
		if(squads.Length==0)throw new UnityException("Failed to find squad.");
		setCamera ();
		setLight ();
	}

	private void setCamera(){
		cameraTarget = squads [selectedSquadIndex].transform.position;
		Camera.main.transform.position = cameraTarget + defaultCameraOffset;
		Camera.main.transform.LookAt (cameraTarget);

        if(Input.GetAxis("L2")==1)
        {
            distance -= Input.GetAxisRaw ("JoystickRV") * zoomSpeed * Time.deltaTime;
            if (distance < 1)
                distance = 1;
			if(distance > 40)
				distance = 40;
			theta -= Input.GetAxisRaw ("JoystickRH")* turnSpeed * Time.deltaTime;
		}
		
		Vector3 newCameraOffset = Quaternion.Euler (0, theta, 0) * defaultCameraOffset;
		newCameraOffset *= distance;

		Camera.main.transform.position = cameraTarget + newCameraOffset;
		Camera.main.transform.LookAt (cameraTarget);

	}

	private void setLight(){
		selectedLight.transform.position = squads[selectedSquadIndex].transform.position+lightOffset;
	}

	private void checkChangeSquad(){
		if (Input.GetButtonUp ("R1")) {
			selectedSquadIndex++;
			selectedSquadIndex %= squads.Length;
			if(selectedRB!=null)selectedRB.velocity=Vector3.zero;
		}
		if (Input.GetButtonUp ("L1")) {
			selectedSquadIndex--;
			if(selectedSquadIndex<0)selectedSquadIndex=squads.Length-1;
			
			if(selectedRB!=null)selectedRB.velocity=Vector3.zero;
		}
	}

	private SquadManager getSelectedManager(){
		return squads[selectedSquadIndex].GetComponent<SquadManager>();
	}

	private void checkNewAction(){
		//start move
		if(Input.GetAxis("DpadH")==-1){
			if(getSelectedManager().numActions>0){
				currentStage=TurnStage.Moving;
				getSelectedManager().startMovement();
			}
		}
		//start start combat
		if(Input.GetAxis("DpadH")==1){
			if(getSelectedManager().numActions>0){
				currentStage=TurnStage.Combat;
			}
		}
		//skip
		if(Input.GetAxis("DpadV")==-1){
			if(getSelectedManager().numActions>0){
				currentStage=TurnStage.InBetween;
				getSelectedManager().skipAction();
			}
			if(getSelectedManager().numActions==0){
				currentStage=TurnStage.None;
			}
		}

	}

	private void checkStateEndOfAction(){
		if(getSelectedManager().numActions==SquadManager.MAX_ACTIONS
		   ||getSelectedManager().numActions==0)
			currentStage=TurnStage.None;
		else
			currentStage=TurnStage.InBetween;
		if (checkTurnComplete ())
			nextTurn ();
	}

    float GetSquadAngle()
    {
        return 90 - Mathf.Rad2Deg * Mathf.Atan2(transform.forward.z, transform.forward.x); // Left handed CW. z = angle 0, x = angle 90
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

	bool checkTurnComplete(){
		foreach(GameObject g in squads){
			if(g.GetComponent<SquadManager>().numActions>0)
				return false;
		}

		return true;
	}

	//call at end of turn
	void nextTurn(){
		foreach(GameObject g in squads){
			g.GetComponent<SquadManager>().resetActions();
		}
		currentPlayersTurn = (currentPlayersTurn+1) % NUM_PLAYERS;
		updateSquadList("Player"+currentPlayersTurn+"Squad");
		Debug.Log ("Player #" + currentPlayersTurn);
	}

	// Update is called once per frame
	void Update () {

		debugText.text = "Player:"+currentPlayersTurn;

		debugText.text += " Remaining Actions:" + getSelectedManager ().numActions;

		debugText.text += " Current Stage: ";
		switch (currentStage) {
		case TurnStage.None: debugText.text+="None";break;
		case TurnStage.Moving: debugText.text+="Moving";break;
		case TurnStage.InBetween: debugText.text+="In Between";break;
		case TurnStage.Combat: debugText.text+="Combat";break;
		};



		if (squads.Length > 0) {


			if(currentStage==TurnStage.None){
				//skip turn button
				if(Input.GetButtonDown("Select")){nextTurn ();}
				checkChangeSquad();
				checkNewAction();
			}
			else if (currentStage==TurnStage.InBetween)
			{
				checkNewAction();
			}
			else if(currentStage==TurnStage.Moving){

				//if the squad is no longer moving (triggered if max distance is met)
				if(!getSelectedManager().midMovement){
					//if we have another action
					if(getSelectedManager().numActions>0){
						currentStage=TurnStage.InBetween;
					}
					else currentStage=TurnStage.None;
				}
				//user undo
				else if(Input.GetButtonDown("Circle")){
					getSelectedManager().undoMove();
					checkStateEndOfAction();
				}
				//user ends early
				else if(Input.GetButtonDown("Cross")){
					getSelectedManager().endMovement();
					checkStateEndOfAction();
				}
				else{
					selectedRB = squads[selectedSquadIndex].GetComponent<Rigidbody>();
					float v = Input.GetAxisRaw("JoystickLV");
					float h = Input.GetAxisRaw("JoystickLH");
					selectedRB.velocity = new Vector3(h,0,v) * 20;
				}
			}
			else if (currentStage==TurnStage.Combat)
			{
				//TODO: enable combat in squad
				//skip
				if(Input.GetAxis("DpadV")==-1 || Input.GetButtonDown("Cross")){
						getSelectedManager().skipAction();
						checkStateEndOfAction();
				}
				else{

					float vert = Input.GetAxis("JoystickRV");
					float horz = Input.GetAxis("JoystickRH");

					if(vert != 0f || horz != 0f)
					{
						var angle = Mathf.Atan2(horz, vert) * Mathf.Rad2Deg;
						drawFoV(Quaternion.Euler(0, angle, 0));
					}

				}
			}
			setCamera();
			setLight();
		}
	}
}
