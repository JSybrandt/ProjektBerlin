using UnityEngine;
using System.Collections.Generic;

public static class Combat
{

    public static List<GameObject> targetsInRange = new List<GameObject>();
    public static int numPlayers = Controller.NUM_PLAYERS;
    public static Controller gameLogic;

    //private static GameObject attackProj;
    public static int selectedTargetIndex;

    static Combat()
    {

    }

    public static SquadManager getTarget()
    {
        return targetsInRange[selectedTargetIndex].GetComponent<SquadManager>();
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
                    if (!Physics.Raycast(myPos, dir, distance, Controller.detectCover))
                        targets.Add(hitColliders[i].gameObject);
                }
            }
            i++;
        }

        targetsInRange = targets;
    }

    public static void fightTarget(GameObject me)
    {
        Hit myHits = detectHits(me,getTarget().transform.position);
        int damage = calculateDamage(me.GetComponent<SquadManager>(),myHits);
        getTarget().takeDamage(damage);
    }

    static int calculateDamage(SquadManager me, Hit myHits)
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

    public static Hit detectHits(GameObject me, Vector3 targetCenter)
    {
        Vector3 myPos = me.transform.position;
        Vector3 targetPos = targetCenter;
        Vector3 dir = (targetPos - myPos).normalized;
        float distance = Vector3.Distance(myPos, targetPos);

        Hit myHits = new Hit();

        //If not behind cover
        if (!Physics.Raycast(myPos, dir, distance, Controller.detectCover))
        {
            //Detect partial cover
            if (!Physics.Raycast(myPos, dir, distance, Controller.detectPartial))
            {
                myHits.dodgeChance = 2;
                myHits.hitChance = 4;
            }
            else
            {
                Debug.Log("I hit partial cover!");
                myHits.hasPartialCover = true;
                myHits.dodgeChance = 2;
                myHits.hitChance = 2;
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
        targetsInRange.Clear();
        selectedTargetIndex = -1;
    }

    public static void setupAoE()
    {
        reset();
    }

    public static void updateAoE()
    {

    }

}

//public class Combat : MonoBehaviour {

//	// Use this for initialization
//	void Start () {

//	}

//	// Update is called once per frame
//	void Update () {

//	}
//}
