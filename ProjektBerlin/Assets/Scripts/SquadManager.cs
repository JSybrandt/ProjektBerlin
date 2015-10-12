using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class SquadManager : NetworkBehaviour
{

    //Ability is using an action on an ability
    public delegate void Ability();
    //AbilityUpdate is for the unit to handle the special action when the ability is selected
    public delegate void AbilityUpdate();
    //Button for ability is first clicked of Ability
    public delegate void AbilityInit();

    [HideInInspector]
    public int size = 0;

    public Texture tex;

    private float unitDistanceFromCenter = 1.5f;

    public const int MAX_ACTIONS = 2;
    private int _numActions = MAX_ACTIONS;
    private Vector3 positionAtActionStart;
    private bool _midMovement = false;

    [HideInInspector]
    public float dodgeChance = BalanceConstants.BASIC_DODGE_CHANCE;


    //TODO: Add return fire?
    public bool retaliation = false;
    public bool inCover = false;
    public bool behindWall = false;

    public bool midMovement { get { return _midMovement; } }
    public int numActions { get { return _numActions; } }

    //Added lights for showing targeted.
    private GameObject myLight;
    [HideInInspector]
    public Light lightPiece;

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
    private bool prevCover = false;

    private Rigidbody rb;

    public Ability unitAbility;
    public Ability squadAbility;
    public AbilityUpdate unitAbilityUpdate;
    public AbilityUpdate squadAbilityUpdate;
    //public AbilityInit unitAbilityInit;
    //public AbilityInit squadAbilityInit;

    // Use this for initialization
    [Command]
    public void CmdInit()
    {
        myLight = new GameObject();
        myLight.transform.position = transform.position;
        lightPiece = myLight.AddComponent<Light>();
        lightPiece.color = Color.red;
        lightPiece.intensity = 8;

        moveProj = GameObject.Find("MoveRadius");
        attackProj = GameObject.Find("AttackRadius");

        lightPiece.enabled = false;

        rb = GetComponent<Rigidbody>();
        if (rb == null) throw new MissingComponentException("Need Rigidbody");
        prevPosition = transform.position;
        if (!isServer)
            Debug.Log("Is Client Squad Manager");
        if (isServer)
            Debug.Log("Is Server Squad Manager");
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
            prevCover = inCover;
            inCover = false;
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

            Collider[] hitColliders = Physics.OverlapSphere(transform.position, 2, GameObject.Find("GameLogic").GetComponent<Controller>().detectPartial); //Needs to figure out layers
            if (hitColliders.Length > 0)
                inCover = true;
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
            inCover = prevCover;
            moveProj.SetActive(false);
        }
        else throw new UnityException("Attempted to undo a move when squad had not moved");
    }

    public void takeDamage(int numUnitsKilled, bool killSpecial = false)
    {
        if (numUnitsKilled > 0)
        {
            int remainingToKill = numUnitsKilled;
            Debug.Log("I'm dieing!");
            if (killSpecial)
                foreach (GameObject unit in units)
                {
                    if (remainingToKill <= 0)
                        break;
                    else if (unit.GetComponent<UnitManager>().isSpecial && unit.activeInHierarchy)
                    {
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
                _numActions = 0;
                //lightPiece.enabled = false;

            }
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

    public int getActiveUnitsCount()
    {
        int alive = 0;

        foreach (GameObject u in units)
        {
            if (u.activeInHierarchy)
            {
                alive++;
            }
        }

        return alive;
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

    void OnGUI()
    {
        if (tex != null && (inCover || behindWall))
        {
            float sizeMod = 15;
            Vector3 guiPosition = Camera.main.WorldToScreenPoint(transform.position);
            float camDistance = Camera.main.GetComponent<CameraController>().distance;
            guiPosition.y = Screen.height - guiPosition.y;
            Rect rect = new Rect((guiPosition.x - tex.width / (camDistance / sizeMod) / 2.0f), (guiPosition.y - tex.height / (camDistance / sizeMod) / 2.0f), tex.width / (camDistance/ sizeMod), tex.height / (camDistance/ sizeMod));
            GUI.DrawTexture(rect, tex);
        }
    }
}