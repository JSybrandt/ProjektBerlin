using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

	private float movementSpeed=50;
	private float zoomSpeed=25;
	private float rotateSpeed=180;

	private const float EPS = 0.5f;
	//unity won't let this be const, treat as CONST
	private Vector3 UNIT_CAMERA_OFFSET = new Vector3(0f, 0.894427191f, -0.4472135955f);//normalized (0,10,-5)
	private const float MAX_DISTANCE = 75f;
	private const float MIN_DISTANCE = 10f;
	private const float DEFAULT_DISTANCE = 35f;

	private Vector3 targetLocation; //where we were told to look
	private Vector3 currentLocation; //where we actually look

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
	

	public void setCameraTarget(Vector3 pos, bool snap = false){
		if (snap) {
			targetLocation=currentLocation=pos;
			isPathingToTarget=false;
			updateCamera();
		} else {
			isPathingToTarget = true;
			targetLocation = pos;
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
		if(!isPathingToTarget) {//move based on controller
			if (Input.GetAxis("L2") == 1)
			{
				distance -= Input.GetAxisRaw("JoystickRV") * zoomSpeed * Time.deltaTime;
				angle -= Input.GetAxisRaw("JoystickRH") * rotateSpeed * Time.deltaTime;
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
