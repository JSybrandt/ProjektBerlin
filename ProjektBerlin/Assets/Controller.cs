using UnityEngine;
using System.Collections;

public class Controller : MonoBehaviour {

	private GameObject[] squads;
	private int selectedSquad;
	private GameObject selectedLight;
	private Camera mainCamera;
	private Rigidbody selectedRB; //used to move selected squad

	//TODO: this needs to be changeable
	private Vector3 cameraOffset = new Vector3(0,5,-5);
	private Vector3 cameraTarget = new Vector3(0,0,0);
	private Vector3 lightOffset = new Vector3(0,2,0);

	void Start(){
		selectedLight = GameObject.Find ("SelectedLight");
		if(selectedLight==null) throw new MissingReferenceException("Need SelectedLight");
		mainCamera = Camera.main;
		if(mainCamera==null) throw new MissingReferenceException("Need Camera.main");
	}

	public void updateSquadList(string tag){
		squads = GameObject.FindGameObjectsWithTag (tag);
		selectedSquad = 0;
		if(squads.Length==0)throw new UnityException("Failed to find squad.");
		setCamera ();
		setLight ();
	}

	private void setCamera(){
		cameraTarget = squads [selectedSquad].transform.position;
		mainCamera.transform.position = cameraTarget + cameraOffset;
		mainCamera.transform.LookAt (cameraTarget);
	}

	private void setLight(){
		selectedLight.transform.position = squads[selectedSquad].transform.position+lightOffset;
	}

	// Update is called once per frame
	void Update () {

		if (squads.Length > 0) {

			if (Input.GetButtonUp ("NextSquad")) {
				selectedSquad++;
				selectedSquad %= squads.Length;
				if(selectedRB!=null)selectedRB.velocity=Vector3.zero;
			}
			if (Input.GetButtonUp ("PrevSquad")) {
				selectedSquad--;
				if(selectedSquad<0)selectedSquad=squads.Length-1;

				if(selectedRB!=null)selectedRB.velocity=Vector3.zero;
			}

			selectedRB = squads[selectedSquad].GetComponent<Rigidbody>();
			float v = Input.GetAxisRaw("Vertical");
			float h = Input.GetAxisRaw("Horizontal");
			selectedRB.velocity = new Vector3(h,0,v);

			setCamera();
			setLight();
		}


	}
}
