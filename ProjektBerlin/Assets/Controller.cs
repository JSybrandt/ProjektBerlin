using UnityEngine;
using UnityEngine.UI;
using System.Collections;

enum TurnStage{
	None,
	Moving,
	InBetween
}

public class Controller : MonoBehaviour {

	private GameObject[] squads;
	private int selectedSquadIndex;
	private GameObject selectedLight;
	private Camera mainCamera;
	private Rigidbody selectedRB; //used to move selected squad

	private Text debugText;

	//TODO: this needs to be changeable
	private Vector3 cameraOffset = new Vector3(0,20,-5);
	private Vector3 cameraTarget = new Vector3(0,0,0);
	private Vector3 lightOffset = new Vector3(0,2,0);

	private TurnStage currentStage = TurnStage.None;

	void Start(){
		selectedLight = GameObject.Find ("SelectedLight");
		if(selectedLight==null) throw new MissingReferenceException("Need SelectedLight");
		mainCamera = Camera.main;
		if(mainCamera==null) throw new MissingReferenceException("Need Camera.main");
		GameObject g = GameObject.Find("DebugText");
		if(g==null) throw new MissingReferenceException("Need Debug text");
		debugText = g.GetComponent<Text> ();
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
		mainCamera.transform.position = cameraTarget + cameraOffset;
		mainCamera.transform.LookAt (cameraTarget);
	}

	private void setLight(){
		selectedLight.transform.position = squads[selectedSquadIndex].transform.position+lightOffset;
	}

	private void checkChangeSquad(){
		if (Input.GetButtonUp ("R2")) {
			selectedSquadIndex++;
			selectedSquadIndex %= squads.Length;
			if(selectedRB!=null)selectedRB.velocity=Vector3.zero;
		}
		if (Input.GetButtonUp ("L2")) {
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
	}

	// Update is called once per frame
	void Update () {

		switch (currentStage) {
		case TurnStage.None: debugText.text="None";break;
		case TurnStage.Moving: debugText.text="Moving";break;
		case TurnStage.InBetween: debugText.text="In Between";break;
		};

		debugText.text = debugText.text + " : Action #" + getSelectedManager ().numActions;

		if (squads.Length > 0) {

			if(currentStage==TurnStage.None){
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
					selectedRB.velocity = new Vector3(h,0,v);
				}
			}
			setCamera();
			setLight();
		}
	}
}
