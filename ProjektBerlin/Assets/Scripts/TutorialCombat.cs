﻿using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public static class TutorialCombat
{
    public static List<GameObject> targetsInRange = new List<GameObject>();
    public static int numPlayers = Controller.NUM_PLAYERS;
    public static Tutorial gameLogic;
    public static bool markerMoving = false;
    private static Marker marker;
    private static float markerAttack = 0;

	public static AudioSource[] shootSounds;
	public static AudioSource rifleShoot;
    public static AudioSource sniperShoot;

    //private static GameObject attackProj;
    public static int selectedTargetIndex = -1;

    static TutorialCombat()
    {
        marker = GameObject.Find("Marker").GetComponent<Marker>();
		CombatIndicationSpawner.instantiate ();
    }

    public static GameObject getTarget()
    {
        return targetsInRange[selectedTargetIndex];
    }

    public static List<GameObject> getTargets()
    {
        return targetsInRange;
    }

    public static void findTargets(this GameObject me, float attack = 0)
    {
        reset();

        //Debug.Log("Attack Radius: " + attack);

        gameLogic.changeUnit.material.color = Color.red;
        gameLogic.changeUnit.enabled = false;

        TutorialManager squad = me.GetComponent<TutorialManager>();
        float attackRange = 0;
        if (attack > 0)
            attackRange = attack;
        else
            attackRange = squad.attackDistance;
        Debug.Log("Attack Radius: " + attackRange);

        Vector3 myPos = me.transform.position;

        gameLogic.attackProj.orthographicSize = attackRange; //Should be set by unit
        gameLogic.attackProj.transform.position = new Vector3(myPos.x, 9, myPos.z);
        gameLogic.attackProj.enabled = true;

        Collider[] hitColliders = Physics.OverlapSphere(myPos, attackRange); //Needs to figure out layers
        List<GameObject> targets = new List<GameObject>();

        Debug.Log("Number of objects in range: " + hitColliders.Length);

        int i = 0;
        while (i < hitColliders.Length)
        {
            for (int j = 0; j < numPlayers; j++)
            {
                string playerTarget = "Player" + j.ToString() + "Squad";
				string playerBaseTarget = "Player" + j.ToString() + "Base";
				if ((j != gameLogic.currentPlayersTurn && hitColliders[i].tag == playerTarget)||(j != gameLogic.currentPlayersTurn && hitColliders[i].tag == playerBaseTarget))
				{
					Vector3 targetPos = hitColliders[i].gameObject.transform.position;
                    Vector3 dir = (targetPos - myPos).normalized;
                    float distance = Vector3.Distance(myPos, targetPos);
                    Debug.Log("Found a dude");

                    //Detect full cover
                    if (!Physics.Raycast(myPos, dir, distance, gameLogic.detectCover))
                    {
                        Debug.Log("Dude added");
                        targets.Add(hitColliders[i].gameObject);
                    }
                    if (Physics.Raycast(myPos, dir, distance, gameLogic.detectWall))
                    {
                        Debug.Log("Dude added, hit wall");
                        hitColliders[i].gameObject.GetComponent<TutorialManager>().behindWall = true;
                    }
                }
            }
            i++;
        }

        targetsInRange = targets;
        Debug.Log("Targets: "+targetsInRange.Count);
        if (targetsInRange.Count > 0) {
            Debug.Log("Targets found BITCHES");
            gameLogic.attackProj.enabled = false;
            gameLogic.changeUnit.enabled = false;

            selectedTargetIndex = 0;
            targetsInRange[selectedTargetIndex].SendMessage("enableTarget");

            Vector3 enemyPos = targetsInRange[selectedTargetIndex].transform.position;
        }
    }

    public static IEnumerator fightTarget(GameObject me, int power)
    {
        ShotsFired myHits = detectHits(me, power);
        int damage = calculateDamage(myHits);
        NetworkView nView = getTarget().GetComponent<NetworkView>();

		//getTarget().GetComponent<NetworkView>().RPC("takeDamage", RPCMode.AllBuffered, damage,false);
        getTarget().GetComponent<TutorialManager>().takeDamage(damage, false);
        shootSounds = me.GetComponent<TutorialManager> ().GetComponents<AudioSource> ();
        rifleShoot = shootSounds[1];
        sniperShoot = shootSounds[2];
        AudioClip shootClip = shootSounds[1].clip;
        int squadSize = me.GetComponent<TutorialManager>().size;

        if (me.GetComponent<TutorialManager> ().squadType == "Rifle") {
			float originalPitch = rifleShoot.pitch;
			for (int i = 0; i< squadSize; i++) {

				rifleShoot.pitch = Random.Range (-.3f, .1f) + originalPitch;
				rifleShoot.PlayOneShot (shootClip);
				yield return new WaitForSeconds (Random.Range (0.05f, 0.1f));
			}
		}
		else if(me.GetComponent<TutorialManager> ().squadType == "Sniper")
		{
			sniperShoot.Play();
		}
		
		
    }

    public static int calculateDamage(ShotsFired myHits)
    {
        int hits = 0;
        for (int i = 0; i < myHits.power; i++)
        {
            if (Random.value <= myHits.hitChance) hits++;
        }
		//CombatIndicationSpawner.spawnHits (myHits.source, hits);
        int damage = 0;
        for (int i = 0; i < hits; i++)
        {
			//higher dodge chance means less bullets go through
			if (Random.value >= myHits.dodgeChance) damage++;
        }
		//CombatIndicationSpawner.spawnMisses (myHits.destination, hits-damage);
        return damage;
    }

 
    public static ShotsFired detectHits(GameObject me, int power)
    {
        Vector3 myPos = me.transform.position;
        Vector3 targetPos = getTarget().transform.position;
        Vector3 dir = (targetPos - myPos).normalized;
        float distance = Vector3.Distance(myPos, targetPos);
		float hitChance=0, dodgeChance=0;
		bool hasPartialCover=false;
        

        //If not behind cover
        if (!Physics.Raycast(myPos, dir, distance, gameLogic.detectCover))
        {
			hitChance = me.GetComponent<TutorialManager>().hitChance;

            //Detect partial cover
			if(getTarget().GetComponent<TutorialManager>() != null)
			{
	            if (!getTarget().GetComponent<TutorialManager>().inCover && !Physics.Raycast(myPos, dir, distance, gameLogic.detectWall))
	            {
					dodgeChance = getTarget().GetComponent<TutorialManager>().dodgeChance; 
	            }
	            else
	            {
	                Debug.Log("I hit partial cover!");
	                hasPartialCover = true;
					dodgeChance = getTarget().GetComponent<TutorialManager>().dodgeChance*2;
            	}
			}
			else{
				dodgeChance = 0;
			}
        }

		return new ShotsFired (myPos, targetPos, power, hitChance, dodgeChance, hasPartialCover);
    }

    public static bool UpdateTarget(TutorialManager me)
    {
        if (Input.GetButtonUp("R1") && targetsInRange.Count > 0)
        {
            gameLogic.attackProj.enabled = false;
            gameLogic.changeUnit.enabled = false;

            if (selectedTargetIndex >= 0)
            {
                targetsInRange[selectedTargetIndex].SendMessage("disableTarget");
                //targetsInRange[selectedTargetIndex].GetComponent<NetworkView>().RPC("disableLight", RPCMode.All);
            }

            selectedTargetIndex++;
            selectedTargetIndex %= targetsInRange.Count;
            targetsInRange[selectedTargetIndex].SendMessage("enableTarget");
            //targetsInRange[selectedTargetIndex].GetComponent<NetworkView>().RPC("enableLight", RPCMode.All);
             
            Vector3 enemyPos = targetsInRange[selectedTargetIndex].transform.position;

            //gameLogic.changeUnit.transform.position = new Vector3(enemyPos.x, 9, enemyPos.z);
            //gameLogic.changeUnit.enabled = true;
        }
        else if (Input.GetButtonUp("L1") && targetsInRange.Count > 0)
        {
            gameLogic.attackProj.enabled = false;
            if (selectedTargetIndex >= 0)
            {
                targetsInRange[selectedTargetIndex].SendMessage("disableTarget");
                //targetsInRange[selectedTargetIndex].GetComponent<NetworkView>().RPC("disableLight", RPCMode.All);
            }
            else
                selectedTargetIndex = 0;

            selectedTargetIndex--;
            if (selectedTargetIndex < 0) selectedTargetIndex = targetsInRange.Count - 1;
            {
                targetsInRange[selectedTargetIndex].SendMessage("enableTarget");
                //targetsInRange[selectedTargetIndex].GetComponent<NetworkView>().RPC("enableLight", RPCMode.All);
            }

            Vector3 enemyPos = targetsInRange[selectedTargetIndex].transform.position;

            //gameLogic.changeUnit.transform.position = new Vector3(enemyPos.x, 9, enemyPos.z);
            //gameLogic.changeUnit.enabled = true;
        }
        if (Input.GetButtonUp("Cross") && targetsInRange.Count > 0 && selectedTargetIndex >= 0)
        {
            return true;
        }

		return false;
    }

    public static void reset()
    {
        if (selectedTargetIndex >= 0 && targetsInRange[selectedTargetIndex].activeInHierarchy)
        {
            targetsInRange[selectedTargetIndex].SendMessage("disableTarget");
            //targetsInRange[selectedTargetIndex].GetComponent<NetworkView>().RPC("disableLight", RPCMode.All);
        }
        foreach (GameObject target in targetsInRange)
        {
			if(target.GetComponent<TutorialManager>()!=null)
            	target.GetComponent<TutorialManager>().behindWall = false;
        }

        targetsInRange.Clear();
        selectedTargetIndex = -1;
        marker.maxDistance = 0;
        markerMoving = false;
        marker.gameObject.SetActive(false);
        markerAttack = 0;
    }

    /// <summary>
    /// Setup for an AoE ability
    /// </summary>
    /// <param name="me">The object from which the AoE is spawning from</param>
    /// <param name="distance">The max distance the object can cast the AoE</param>
    /// <param name="range">Custom attack range of AoE</param>
    public static void setupAoE(GameObject me, float distance, float range = 0)
    {
        reset();
        marker.gameObject.SetActive(true);
        marker.maxDistance = distance;
        marker.markerStart = me.transform.position;
        marker.transform.position = me.transform.position;
        markerMoving = true;
        markerAttack = range;
        if (range > 0)
        {
            gameLogic.attackProj.orthographicSize = markerAttack; //Should be set by unit
            gameLogic.attackProj.enabled = true;
        }
    }

    public static void updateAoE(TutorialManager me, ref bool activated)
    {      
        gameLogic.attackProj.transform.position = new Vector3(marker.transform.position.x, 9, marker.transform.position.z);   

        if (Input.GetButtonUp("Cross"))
        {
            activated = true;
            if(markerAttack > 0)
                findTargets(marker.gameObject, markerAttack);
            Camera.main.GetComponent<CameraController>().setCameraTarget(me.transform.position, true);
        }
        
        if (Input.GetButtonDown("Circle"))
        {
            reset();
            Camera.main.GetComponent<CameraController>().setCameraTarget(me.transform.position, true);
        }
    }

}