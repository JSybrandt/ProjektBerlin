using UnityEngine;
using System.Collections.Generic;

public static class Combat
{

    public static List<GameObject> targetsInRange = new List<GameObject>();
    public static int numPlayers = Controller.NUM_PLAYERS;
    public static Controller gameLogic;
    private static Vector3 markerStart = new Vector3();
    private static Vector3 markerPos = new Vector3();
    private static float maxDistance = 0;
    public static bool markerMoving = false;
    private static Marker marker;

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

    static int calculateDamage(SquadManager me, ShotsFired myHits)
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
            if (!Physics.Raycast(myPos, dir, distance, gameLogic.detectPartial))
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
        targetsInRange.Clear();
        selectedTargetIndex = -1;
        marker.maxDistance = 0;
        markerMoving = false;
    }

    /// <summary>
    /// Setup for an AoE ability
    /// </summary>
    /// <param name="me">The object from which the AoE is spawning from</param>
    /// <param name="distance">The max distance the object can cast the AoE</param>
    public static void setupAoE(GameObject me, float distance)
    {
        reset();
        marker.maxDistance = distance;
        marker.markerStart = me.transform.position;
        markerMoving = true;
    }

    public static void updateAoE(SquadManager me, ref bool activated)
    {
        if (Input.GetButtonUp("Cross"))
        {
            activated = true;
        }
    }

}