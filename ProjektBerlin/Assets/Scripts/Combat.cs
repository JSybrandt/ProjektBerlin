﻿using UnityEngine;
using System.Collections.Generic;

public static class Combat
{

    public static List<GameObject> targetsInRange = new List<GameObject>();
    public static int numPlayers = Controller.NUM_PLAYERS;
    public static Controller gameLogic;
    public static bool markerMoving = false;
    private static Marker marker;
    private static float markerAttack = 0;

    //private static GameObject attackProj;
    public static int selectedTargetIndex = -1;

    static Combat()
    {
        marker = GameObject.Find("Marker").GetComponent<Marker>();
    }

    public static SquadManager getTarget()
    {
        return targetsInRange[selectedTargetIndex].GetComponent<SquadManager>();
    }

    public static List<GameObject> getTargets()
    {
        return targetsInRange;
    }

    public static void findTargets(this GameObject me, float attack = 0)
    {
        reset();

        SquadManager squad = me.GetComponent<SquadManager>();
        float attackRange = 0;
        if (attack > 0)
            attackRange = attack;
        else
            attackRange = squad.attackDistance;

        Vector3 myPos = me.transform.position;

        gameLogic.attackProj.GetComponent<Projector>().orthographicSize = attackRange; //Should be set by unit
        gameLogic.attackProj.transform.position = new Vector3(myPos.x, 9, myPos.z);
        gameLogic.attackProj.GetComponent<Projector>().enabled = true;

        Collider[] hitColliders = Physics.OverlapSphere(myPos, attackRange); //Needs to figure out layers
        List<GameObject> targets = new List<GameObject>();

        Debug.Log("Number of objects in range: " + hitColliders.Length);

        int i = 0;
        while (i < hitColliders.Length)
        {
            for (int j = 0; j < numPlayers; j++)
            {
                string playerTarget = "Player" + j.ToString() + "Squad";
                if (j != gameLogic.currentPlayersTurn && hitColliders[i].tag == playerTarget)
                {
                    Vector3 targetPos = hitColliders[i].gameObject.transform.position;
                    Vector3 dir = (targetPos - myPos).normalized;
                    float distance = Vector3.Distance(myPos, targetPos);

                    //Detect full cover
                    if (!Physics.Raycast(myPos, dir, distance, gameLogic.detectCover))
                        targets.Add(hitColliders[i].gameObject);
                    if (Physics.Raycast(myPos, dir, distance, gameLogic.detectWall))
                        hitColliders[i].gameObject.GetComponent<SquadManager>().behindWall = true;
                }
            }
            i++;
        }

        targetsInRange = targets;
    }

    public static void fightTarget(GameObject me)
    {
        ShotsFired myHits = detectHits(me);
        int damage = calculateDamage(me.GetComponent<SquadManager>(), myHits);
        getTarget().takeDamage(damage);
    }

    //public static void fightTargets(GameObject me, int )

    private static int calculateDamage(SquadManager me, ShotsFired myHits)
    {
        int hits = 0;
        for (int i = 0; i < me.getPower(); i++)
        {
            if (Random.Range(1, 6) <= myHits.hitChance) hits++;
        }
        int damage = 0;
        for (int i = 0; i < hits; i++)
        {
            if (Random.Range(1, 6) <= myHits.dodgeChance) damage++;
        }

        Debug.Log("Attack:" + me.getPower() + " Hit Chance: " + myHits.hitChance + " Hits:" + hits + " Dodge Chance: " + myHits.dodgeChance + " Damage:" + damage);
        return damage;
    }

    public static int rollDamage(ShotsFired myHits, int shots)
    {
        int hits = 0;
        for (int i = 0; i < shots; i++)
        {
            if (Random.Range(1, 6) <= myHits.hitChance) hits++;
        }
        int damage = 0;
        for (int i = 0; i < hits; i++)
        {
            if (Random.Range(1, 6) <= myHits.dodgeChance) damage++;
        }

        Debug.Log("rollDamage: "+damage);
        return damage;
    }

    public static ShotsFired detectHits(GameObject me)
    {
        Vector3 myPos = me.transform.position;
        Vector3 targetPos = getTarget().transform.position;
        Vector3 dir = (targetPos - myPos).normalized;
        float distance = Vector3.Distance(myPos, targetPos);

        ShotsFired myHits = new ShotsFired();

        //If not behind cover
        if (!Physics.Raycast(myPos, dir, distance, gameLogic.detectCover))
        {
            myHits.hitChance = me.GetComponent<SquadManager>().hitChance;

            //Detect partial cover
            if (!getTarget().inCover && !Physics.Raycast(myPos, dir, distance, gameLogic.detectWall))
            {
                myHits.dodgeChance = getTarget().dodgeChance; 
            }
            else
            {
                Debug.Log("I hit partial cover!");
                myHits.hasPartialCover = true;
                myHits.dodgeChance = getTarget().dodgeChance - 2;
            }
        }

        return myHits;
    }

    public static void UpdateTarget(SquadManager me, ref bool activated)
    {
        if (Input.GetButtonUp("R1") && targetsInRange.Count > 0)
        {
            gameLogic.attackProj.GetComponent<Projector>().enabled = false;
            if (selectedTargetIndex >= 0)
                targetsInRange[selectedTargetIndex].SendMessage("disableLight");

            selectedTargetIndex++;
            selectedTargetIndex %= targetsInRange.Count;
            targetsInRange[selectedTargetIndex].SendMessage("enableLight");
        }
        else if (Input.GetButtonUp("L1") && targetsInRange.Count > 0)
        {
            gameLogic.attackProj.GetComponent<Projector>().enabled = false;
            if (selectedTargetIndex >= 0)
                targetsInRange[selectedTargetIndex].SendMessage("disableLight");
            else
                selectedTargetIndex = 0;

            selectedTargetIndex--;
            if (selectedTargetIndex < 0) selectedTargetIndex = targetsInRange.Count - 1;
            targetsInRange[selectedTargetIndex].SendMessage("enableLight");
        }
        if (Input.GetButtonUp("Cross") && targetsInRange.Count > 0 && selectedTargetIndex >= 0)
        {
            activated = true;
        }
    }

    public static void reset()
    {
        if (selectedTargetIndex >= 0 && targetsInRange[selectedTargetIndex].activeInHierarchy)
            targetsInRange[selectedTargetIndex].GetComponent<SquadManager>().lightPiece.enabled = false;

        foreach(GameObject target in targetsInRange)
        {
            target.GetComponent<SquadManager>().behindWall = false;
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
            gameLogic.attackProj.GetComponent<Projector>().orthographicSize = markerAttack; //Should be set by unit
            gameLogic.attackProj.GetComponent<Projector>().enabled = true;
        }
    }

    public static void updateAoE(SquadManager me, ref bool activated)
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
