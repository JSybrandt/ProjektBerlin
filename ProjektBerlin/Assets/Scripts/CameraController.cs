using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

	private float movementSpeed=50;
	private float zoomSpeed=25;
	private float rotateSpeed=180;

	private const float EPS = 1f;
	//unity won't let this be const, treat as CONST
	private Vector3 UNIT_CAMERA_OFFSET = new Vector3(0f, 0.894427191f, -0.4472135955f);//normalized (0,10,-5)
	private const float MAX_DISTANCE = 75f;
	private const float MIN_DISTANCE = 10f;
	private const float DEFAULT_DISTANCE = 35f;

	private Vector3 targetLocation; //where we were told to look
	private Vector3 currentLocation; //where we actually look
	private Vector3 previouslyAssignedTarget;

	private float timeDelay;
	private Vector3 delayTarget;
	private bool isDelaySnap;

	private float timeFrozen;

	private float _angle;
	public float angle{
		get{return _angle;}
		private set{_angle=value; 
			while(_angle>=360){_angle-=360;}
			while(_angle<0){_angle+=360;}
		}
	}

	private float _distance=DEFAULT_DISTANCE;
	public float distance{
		get{return _distance;}
		private set{_distance=Mathf.Max(MIN_DISTANCE,Mathf.Min(MAX_DISTANCE,value));}
		}

	private bool isPathingToTarget=false;
	
	public void freezeCamera(float time){
		if(time>0)
			timeFrozen = time;
	}

	public void setCameraTarget(Vector3 pos, bool snap = false, float delay=0){
		if (delay > 0 || timeFrozen > 0) {
			timeDelay = Mathf.Max (delay, timeFrozen);
			delayTarget = pos;
			isDelaySnap = snap;
		} else {
			if (snap) {
				targetLocation = currentLocation = pos;
				isPathingToTarget = false;
				updateCamera ();
			} else {
				isPathingToTarget = true;
				targetLocation = pos;
			}
			previouslyAssignedTarget = pos;
		}
	}

	private void updateCamera(){
		transform.position = currentLocation + (Quaternion.Euler (0, angle, 0) * UNIT_CAMERA_OFFSET) * distance;
		Camera.main.transform.LookAt(currentLocation);
	}

	void Start(){
		updateCamera ();
	}

	void FixedUpdate(){
		timeFrozen = Mathf.Max (timeFrozen - Time.deltaTime, 0);
		if (timeDelay > 0) {
			timeDelay-=Time.deltaTime;
			if(timeDelay<=0) 
				setCameraTarget(delayTarget,isDelaySnap,0);
		}
		if(!isPathingToTarget) {//move based on controller
			if (Input.GetAxis("L2") == 1)
			{
				distance -= Input.GetAxisRaw("JoystickRV") * zoomSpeed * Time.deltaTime;
				angle -= Input.GetAxisRaw("JoystickRH") * rotateSpeed * Time.deltaTime;

				Vector3 panInput = new Vector3(Input.GetAxisRaw("JoystickLH"),0,Input.GetAxisRaw("JoystickLV"));
				if(panInput.sqrMagnitude>0){
					if(panInput.sqrMagnitude>1)panInput.Normalize();
					panInput = Quaternion.Euler(0,angle,0)*panInput;
					 Vector3 del = panInput*movementSpeed*Time.deltaTime;
					currentLocation += del;
					targetLocation += del;
				}

				if(Input.GetButton("L3")){
					setCameraTarget(previouslyAssignedTarget);
				}
			}
		}

	}

	// Update is called once per frame
	void Update () {
		//move auto
		if (isPathingToTarget) {
			if((currentLocation-targetLocation).magnitude>EPS){

				//if(distance!=DEFAULT_DISTANCE){
					//move zoom to default
				//	if(distance>DEFAULT_DISTANCE){distance-=zoomSpeed * Time.deltaTime;}
				//	if(distance<DEFAULT_DISTANCE){distance+=zoomSpeed * Time.deltaTime;}
				//	if(Mathf.Abs(distance-DEFAULT_DISTANCE)<EPS){distance=DEFAULT_DISTANCE;}
				//}

				currentLocation += (targetLocation-currentLocation).normalized * movementSpeed * Time.deltaTime;
			}
			else{
				currentLocation=targetLocation;
				isPathingToTarget=false;
			}
		} 
		updateCamera ();
	}
}
