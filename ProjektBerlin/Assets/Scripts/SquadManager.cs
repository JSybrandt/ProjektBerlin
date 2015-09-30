using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SquadManager : MonoBehaviour
{

    //Ability is using an action on an ability
    public delegate void Ability();
    //AbilityUpdate is for the unit to handle the special action when the ability is selected
    public delegate void AbilityUpdate();
    //Button for ability is first clicked of Ability
    public delegate void AbilityInit();

    [HideInInspector]
    public int size = 0;

    private float unitDistanceFromCenter = 1.5f;

    public const int MAX_ACTIONS = 2;
    private int _numActions = MAX_ACTIONS;
    private Vector3 positionAtActionStart;
    private bool _midMovement = false;

    //TODO: Add return fire?
    private bool retailation = false;

    public bool midMovement { get { return _midMovement; } }
    public int numActions { get { return _numActions; } }

    //Added lights for showing targeted.
    private GameObject myLight;
    private Light lightPiece;

    [HideInInspector]
    public GameObject moveProj;
    [HideInInspector]
    public GameObject attackProj;

    [HideInInspector]
    public float movementDistance;
    [HideInInspector]
    public float attackDistance;

    //TODO: This might need to become a collection of game objects
    [HideInInspector]
    public Transform[] unitTargets;
    [HideInInspector]
    public GameObject[] units;

    private const float MAX_UNIT_HEIGHT = 0.5f;
    private const float FLOOR_DISPACEMENT = 1f;
    private Vector3 prevPosition; //used to revert after colliding w/ terrain. 

    private Rigidbody rb;

    public Ability unitAbility;
    public Ability squadAbility;
    public AbilityUpdate unitAbilityUpdate;
    public AbilityUpdate squadAbilityUpdate;
    public AbilityInit unitAbilityInit;
    public AbilityInit squadAbilityInit;

    // Use this for initialization
    public void init(float atkRadius = 20, float mvRadius = 50)
    {

        myLight = new GameObject();
        myLight.transform.position = transform.position;
        lightPiece = myLight.AddComponent<Light>();
        lightPiece.color = Color.red;
        lightPiece.intensity = 8;

        attackDistance = atkRadius;
        movementDistance = mvRadius;

        moveProj = GameObject.Find("MoveRadius");
        attackProj = GameObject.Find("AttackRadius");

        lightPiece.enabled = false;

        rb = GetComponent<Rigidbody>();
        if (rb == null) throw new MissingComponentException("Need Rigidbody");
        prevPosition = transform.position;
    }

    //once every physics step
    void FixedUpdate()
    {

        if (_midMovement && rb.velocity.magnitude > 0)
        {
            float h = Terrain.activeTerrain.SampleHeight(transform.position) + FLOOR_DISPACEMENT;
            transform.position = new Vector3(transform.position.x, h, transform.position.z);
            if ((positionAtActionStart - transform.position).magnitude >= movementDistance)
            {
                //endMovement();
                transform.position = prevPosition;
            }
			if(transform.position.x > 62 || transform.position.x < -63|| transform.position.z > 100 || transform.position.z < -97)
				transform.position = prevPosition;
        }
        else
        {
            rb.Sleep();
        }
        prevPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < size; i++)
        {
            units[i].transform.position = unitTargets[i].position;
            transform.rotation = Quaternion.identity;
            //TODO: This should be targetting instead of teleporting
        }


        //Updates associated light
        myLight.transform.position = transform.position;

    }

    public void startMovement()
    {
        if (numActions > 0)
        {
            positionAtActionStart = transform.position;
            moveProj.transform.position = new Vector3(transform.position.x, 9, transform.position.z);
            moveProj.GetComponent<Projector>().orthographicSize = movementDistance + 3;
            moveProj.SetActive(true);
            _midMovement = true;
        }
        else throw new UnityException("Attempted to start an action when squad had none.");
    }
    public void endMovement()
    {
        if (_midMovement)
        {
            _midMovement = false;
            _numActions--;
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            moveProj.SetActive(false);
        }
        else throw new UnityException("Attempted to end an actions before starting one.");
    }

    public void skipAction()
    {
        if (_numActions > 0)
        {
            _numActions--;
            moveProj.SetActive(false);
        }
        else throw new UnityException("Attempted to skip an action when squad had none.");
    }

    public void resetActions()
    {
        _numActions = MAX_ACTIONS;
        _midMovement = false;
        moveProj.SetActive(false);
    }

    public void undoMove()
    {
        if (_midMovement)
        {
            _midMovement = false;
            transform.position = positionAtActionStart;
            moveProj.SetActive(false);
        }
        else throw new UnityException("Attempted to undo a move when squad had not moved");
    }

    public void takeDamage(int numUnitsKilled, bool killSpecial = false)
    {
        int remainingToKill = numUnitsKilled;

        Debug.Log("I'm dieing!");
        if(killSpecial)
            foreach (GameObject unit in units)
            {
                if (remainingToKill <= 0)
                    break;
                else if (unit.GetComponent<UnitManager>().isSpecial && unit.activeInHierarchy) {
                    unit.SetActive(false);
                    remainingToKill--;
                }
            }

        foreach (GameObject unit in units)
        {
            if (remainingToKill <= 0)
                break;
            else if (unit.activeInHierarchy)
            {
                unit.SetActive(false);  //Might not be correct
                remainingToKill--;
            }
        }

        if (isDead())
        {
            //Disable this squad
            gameObject.SetActive(false);
            //lightPiece.enabled = false;

        }
        lightPiece.enabled = false;
    }

    //Checks all units of the squad to see if they are active or not.
    public bool isDead()
    {
        foreach (GameObject unit in units)
        {
            if (unit.activeInHierarchy)
            {
                return false;
            }
        }

        return true;
    }

    public void enableLight()
    {
        lightPiece.enabled = true;
    }

    public void disableLight()
    {
        lightPiece.enabled = false;
    }

    public List<GameObject> getTargets(int activePlayer, int numPlayers, LayerMask detectCover, LayerMask detectPartial, float attack = 0)
    {

        if (attack == 0)
            attack = attackDistance;

        Vector3 myPos = transform.position;

        attackProj.GetComponent<Projector>().orthographicSize = attack; //Should be set by unit
        attackProj.transform.position = new Vector3(myPos.x, 9, myPos.z);
        attackProj.GetComponent<Projector>().enabled = true;

        Collider[] hitColliders = Physics.OverlapSphere(myPos, attack); //Needs to figure out layers
        List<GameObject> targets = new List<GameObject>();

        Debug.Log("Number of objects in range: " + hitColliders.Length);

        int i = 0;
        while (i < hitColliders.Length)
        {
            for (int j = 0; j < numPlayers; j++)
            {
                string playerTarget = "Player" + j.ToString() + "Squad";
                if (j != activePlayer && hitColliders[i].tag == playerTarget)
                {
                    Vector3 targetPos = hitColliders[i].gameObject.transform.position;
                    Vector3 dir = (targetPos - myPos).normalized;
                    float distance = Vector3.Distance(myPos, targetPos);

                    //Detect full cover
                    if (!Physics.Raycast(myPos, dir, distance, detectCover))
                        targets.Add(hitColliders[i].gameObject);
                }
            }
            i++;
        }

        return targets;
    }

    public Hit detectHits(Vector3 targetCenter, LayerMask detectCover, LayerMask detectPartial)
    {
        Vector3 myPos = transform.position;
        Vector3 targetPos = targetCenter;
        Vector3 dir = (targetPos - myPos).normalized;
        float distance = Vector3.Distance(myPos, targetPos);

        Hit myHits = new Hit();

        //If not behind cover
        if (!Physics.Raycast(myPos, dir, distance, detectCover))
        {
            //Detect partial cover
            if (!Physics.Raycast(myPos, dir, distance, detectPartial))
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

    public List<GameObject> getActiveUnits()
    {
        List<GameObject> myUnits = new List<GameObject>();

        foreach (GameObject u in units)
        {
            if (u.activeInHierarchy)
            {
                myUnits.Add(u);
            }
        }

        return myUnits;
    }

    //public List<Hit> squadHits(GameObject targetSquad, int activePlayer, int numPlayers, LayerMask detectCover, LayerMask detectPartial)
    //{
    //    List<Hit> myHits = getTargets(targetSquad.transform.position,activePlayer,numPlayers,detectCover,detectPartial);

    //    //List<GameObject> enemyUnits = targetSquad.GetComponent<SquadManager>().getActiveUnits();

    //    //foreach (GameObject u in getActiveUnits())
    //    //{
    //    //    myHits.AddRange(u.GetComponent<UnitManager>().detectHits(enemyUnits,activePlayer,numPlayers,detectCover,detectPartial));
    //    //}

    //    return myHits;
    //}

    public void fightTarget(GameObject targetSquad, LayerMask detectCover, LayerMask detectPartial)
    {
        Hit myHits = detectHits(targetSquad.transform.position, detectCover, detectPartial);
        int damage = calculateDamage(myHits);
        targetSquad.GetComponent<SquadManager>().takeDamage(damage);
    }

    int calculateDamage(Hit myHits)
    {
        int hits = 0;
        for (int i = 0; i < getPower(); i++)
        {
            if (Random.Range(1, 6) <= myHits.hitChance) hits++;
        }
        int damage = 0;
        for (int i = 0; i < hits; i++)
        {
            if (Random.Range(1, 6) <= myHits.dodgeChance) damage++;
        }

        Debug.Log("Attack:" + getPower() + " Hit Chance: " + myHits.hitChance + " Hits:" + hits + " Dodge Chance: " + myHits.dodgeChance + " Damage:" + damage);
        return damage;
    }

    public int getPower()
    {
        int sum = 0;
        foreach (GameObject u in units)
        {
            if (u.activeInHierarchy)
            {
                sum += u.GetComponent<UnitManager>().power;
            }
        }
        return sum;
    }

    public float getMovementRadius()
    {
        return movementDistance;
    }

    public float getAttackRadius()
    {
        return attackDistance;
    }

    public void setColor(Color c)
    {
        foreach (GameObject g in units)
        {
            g.GetComponent<Renderer>().material.color = c;
        }
    }
}