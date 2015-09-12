using UnityEngine;
using System.Collections;

public class Controller : MonoBehaviour {

	private GameObject[] squads;
	private int selectedSquadIndex;
	private GameObject selectedLight;
	private Camera mainCamera;
	private Rigidbody selectedRB; //used to move selected squad
	public float theta;
	public float zoomSpeed;
	private float distance;

	//TODO: this needs to be changeable
	private Vector3 defaultCameraOffset = new Vector3(0,20,-5);
	private Vector3 cameraTarget = new Vector3(0,0,0);
	private Vector3 lightOffset = new Vector3(0,2,0);

	void Start(){
		selectedLight = GameObject.Find ("SelectedLight");
		if(selectedLight==null) throw new MissingReferenceException("Need SelectedLight");
		mainCamera = Camera.main;
		if(mainCamera==null) throw new MissingReferenceException("Need Camera.main");
		distance = defaultCameraOffset.y;
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
		mainCamera.transform.position = cameraTarget + defaultCameraOffset;
		mainCamera.transform.LookAt (cameraTarget);
		
		Vector3 vec =  mainCamera.transform.position - squads [selectedSquadIndex].transform.position;
		distance += Input.GetAxisRaw ("Fire2") * zoomSpeed;
		theta += Input.GetAxisRaw ("Fire3");
		Vector3 newCameraOffset = Quaternion.Euler (0, theta, 0) * defaultCameraOffset;
		newCameraOffset *= distance;

		mainCamera.transform.position = cameraTarget + newCameraOffset;
		mainCamera.transform.LookAt (cameraTarget);

	}

	private void setLight(){
		selectedLight.transform.position = squads[selectedSquadIndex].transform.position+lightOffset;
	}

	// Update is called once per frame
	void Update () {

		if (squads.Length > 0) {

			if (Input.GetButtonUp ("NextSquad")) {
				selectedSquadIndex++;
				selectedSquadIndex %= squads.Length;
				if(selectedRB!=null)selectedRB.velocity=Vector3.zero;
			}
			if (Input.GetButtonUp ("PrevSquad")) {
				selectedSquadIndex--;
				if(selectedSquadIndex<0)selectedSquadIndex=squads.Length-1;

				if(selectedRB!=null)selectedRB.velocity=Vector3.zero;
			}

			selectedRB = squads[selectedSquadIndex].GetComponent<Rigidbody>();
			float v = Input.GetAxisRaw("Vertical");
			float h = Input.GetAxisRaw("Horizontal");
			selectedRB.velocity = new Vector3(h,0,v);

			setCamera();
			setLight();
		}


	}
}
