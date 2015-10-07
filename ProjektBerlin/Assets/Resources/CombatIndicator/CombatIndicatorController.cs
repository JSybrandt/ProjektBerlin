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

	private Material hitMat;
	private Material missMat;

	public void Start(){
		hitMat = (Material) Resources.Load("/CombatIndicator/Materials/Hit.mat");
		missMat = (Material) Resources.Load("/CombatIndicator/Materials/Miss.mat");

		if(hitMat==null)
			throw new MissingReferenceException("Missing hit.mat");
		if(missMat==null)
			throw new MissingReferenceException("Missing miss.mat");

		if(GetComponent<MeshRenderer> ()==null)
			throw new MissingReferenceException("CombatIndicator prefab missing mesh renderer.");
	}
	
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
		Color c = GetComponent<Material> ().color;
		c.a = Mathf.Max (1, lifespan);//fade out
		GetComponent<Material> ().color = c;

		transform.position += Vector3.up * verticalSpeed * Time.deltaTime;
	}

	public void Update(){
		//totally ripped off a wiki
		transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.back,
		                 Camera.main.transform.rotation * Vector3.up);
	}

}
