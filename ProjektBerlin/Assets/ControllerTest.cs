using UnityEngine;
using System.Collections;

public class ControllerTest : MonoBehaviour {

	// Update is called once per frame
	void Update () {
		if (Input.GetButtonDown ("Triangle")) {
			Debug.Log("Triangle");
		}
		if (Input.GetButtonDown ("Circle")) {
			Debug.Log("Circle");
		}
		if (Input.GetButtonDown ("Cross")) {
			Debug.Log("Cross");
		}
		if (Input.GetButtonDown ("Square")) {
			Debug.Log("Square");
		}
		if (Input.GetButtonDown ("L1")) {
			Debug.Log("L1");
		}
		if (Input.GetButtonDown ("L2")) {
			Debug.Log("L2");
		}
		if (Input.GetButtonDown ("L3")) {
			Debug.Log("L3");
		}
		if (Input.GetButtonDown ("R1")) {
			Debug.Log("R1");
		}
		if (Input.GetButtonDown ("R2")) {
			Debug.Log("R2");
		}
		if (Input.GetButtonDown ("R3")) {
			Debug.Log("R3");
		}
		if (Input.GetButtonDown ("Select")) {
			Debug.Log("Select");
		}
		if (Input.GetButtonDown ("Start")) {
			Debug.Log("Start");
		}
		float t;
		t = Input.GetAxisRaw ("JoystickLV");
		if (t != 0) {
			Debug.Log("JoystickLV:"+t);
		}
		t = Input.GetAxisRaw ("JoystickLH");
		if (t != 0) {
			Debug.Log("JoystickLH:"+t);
		}
		t = Input.GetAxisRaw ("JoystickRV");
		if (t != 0) {
			Debug.Log("JoystickRV:"+t);
		}
		t = Input.GetAxisRaw ("JoystickRH");
		if (t != 0) {
			Debug.Log("JoystickRH:"+t);
		}
		t = Input.GetAxisRaw ("JoystickRV");
		if (t != 0) {
			Debug.Log("JoystickRV:"+t);
		}
		t = Input.GetAxisRaw ("DpadH");
		if (t != 0) {
			Debug.Log("DpadH:"+t);
		}
		t = Input.GetAxisRaw ("DpadV");
		if (t != 0) {
			Debug.Log("DpadV:"+t);
		}
	}
}
