﻿using UnityEngine;
using System.Collections;

public class BaseManager : MonoBehaviour {

	//Added lights for showing targeted.
	private GameObject myLight;
	[HideInInspector]
	public Light lightPiece;

	public int maxHealth = 100;
	public int currentHealth;

	public void enableLight()
	{
		lightPiece.enabled = true;
	}
	
	public void disableLight()
	{
		lightPiece.enabled = false;
	}

	public void init(float atkRadius = 20, float mvRadius = 50)
	{
		currentHealth = maxHealth;

		myLight = new GameObject ();
		myLight.transform.position = transform.position;
		lightPiece = myLight.AddComponent<Light> ();
		lightPiece.color = Color.red;
		lightPiece.intensity = 8;

		lightPiece.enabled = false;
	}

	public void takeDamage(damageInfo dmgInfo)
	{
		currentHealth -= dmgInfo.getDamage();
	}

	void Update(){
		//Updates associated light
		//myLight.transform.position = transform.position;
	}
}
