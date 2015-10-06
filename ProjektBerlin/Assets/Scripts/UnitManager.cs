using UnityEngine;
using System.Collections.Generic;

public class UnitManager : MonoBehaviour
{

    private int _power = 2;
    public int power
    {
        get { return _power; }
        set { _power = Mathf.Max(0, value); }
    }

    private int hit = 2;
    public int _hit { get { return hit; } set { hit = Mathf.Max(0, value); } }

    private int dodge = 2;
    public int _dodge { get { return dodge; } set { dodge = Mathf.Max(0, value); } }

    [HideInInspector]
    public bool isSpecial = false;

    public List<ShotsFired> detectHits(List<GameObject> enemyUnits, int activePlayer, int numPlayers, LayerMask detectCover, LayerMask detectPartial)
    {
        List<ShotsFired> myHits = new List<ShotsFired>();

        foreach (GameObject enemyUnit in enemyUnits)
        {
            for (int j = 0; j < numPlayers; j++)
            {
                string playerTarget = "Player" + j.ToString() + "Squad";
                if (j != activePlayer && enemyUnit.tag == playerTarget)
                {
                    Vector3 myPos = transform.position;
                    Vector3 targetPos = enemyUnit.transform.position;
                    Vector3 dir = (targetPos - myPos).normalized;
                    float distance = Vector3.Distance(myPos, targetPos);

                    //Detect full cover
                    if (!Physics.Raycast(myPos, dir, distance, detectCover))
                    {
                        //Detect partial cover
                        if (!Physics.Raycast(myPos, dir, distance, detectPartial))
                        {
                            Debug.Log("I hit partial cover!");
                            myHits.Add(new ShotsFired(2, 4, true));
                        }
                        else
                        {
                            myHits.Add(new ShotsFired(4, 4, false));
                        }
                    }
                } //Is target
            } //Players loop
        } //Units loop
        return myHits;
    }
}

