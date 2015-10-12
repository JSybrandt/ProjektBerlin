﻿using UnityEngine;
using System.Collections;

public class CombatIndicatorController : MonoBehaviour {

	[HideInInspector]
	public enum Type{
		HIT,
		MISS
	}

	private const float DEFAULT_LIFESPAN = 3;

	[HideInInspector]
	public float lifespan = DEFAULT_LIFESPAN;

	[HideInInspector]
	public float verticalSpeed = 1;

	[HideInInspector]
	public Type iconType = Type.MISS;



	public void set(Vector3 pos, Type t){
		switch (t) {
		case Type.HIT:
			GetComponent<MeshRenderer>().material = (Material) Resources.Load("CombatIndicator/Materials/hit");;
			break;
		case Type.MISS:
			GetComponent<MeshRenderer>().material  = (Material) Resources.Load("CombatIndicator/Materials/miss");
			break;
		}
		lifespan = DEFAULT_LIFESPAN;
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
			Destroy (this.gameObject);
	}

}