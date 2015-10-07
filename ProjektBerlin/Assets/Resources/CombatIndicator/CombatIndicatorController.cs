using UnityEngine;
using System.Collections;

public class CombatIndicatorController : MonoBehaviour {

	[HideInInspector]
	public enum Type{
		HIT,
		MISS
	}

	[HideInInspector]
	public float lifespan = 3;

	[HideInInspector]
	public float verticalSpeed = 1;

	[HideInInspector]
	public Type iconType = Type.MISS;

	private static Material hitMat = (Material) Resources.Load("CombatIndicator/Materials/hit");
	private static Material missMat = (Material) Resources.Load("CombatIndicator/Materials/miss");
	

	public void set(Vector3 pos, Type t){
		switch (t) {
		case Type.HIT:
			GetComponent<MeshRenderer>().material = hitMat;
			break;
		case Type.MISS:
			GetComponent<MeshRenderer>().material  = missMat;
			break;
		}

		transform.position = pos;
	}

	public void FixedUpdate(){
		lifespan -= Time.deltaTime;
		Color c = GetComponent<MeshRenderer>().material.color;
		c.a = Mathf.Min (1, lifespan);//fade out
		GetComponent<MeshRenderer>().material.color = c;

		transform.position += Vector3.up * verticalSpeed * Time.deltaTime;
	}

	public void Update(){
		//totally ripped off a wiki
		transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward,
		                 Camera.main.transform.rotation * Vector3.up);
		if (lifespan <= 0)
			Destroy (this);
	}

}
