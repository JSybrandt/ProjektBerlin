using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class TutorialManager : MonoBehaviour
{
    //Ability is using an action on an ability
    public delegate void Ability();
    //AbilityUpdate is for the unit to handle the special action when the ability is selected
    public delegate void AbilityUpdate();
    //Button for ability is first clicked of Ability
    public delegate void AbilityInit();

    //FLAGS: 
    private bool dead = false;
    public bool retaliation = false; //TODO: Add retaliation? 
    public bool inCover = false;
    public bool behindWall = false;
    private bool prevCover = false;
    private bool targeted = false;
    private bool selected = false;

    //Serialization stuff, really useful link:
    // http://www.paladinstudios.com/2013/07/10/how-to-create-an-online-multiplayer-game-with-unity/
    private float lastSynchronizationTime = 0f;
    private float syncDelay = 0f;
    private float syncTime = 0f;
    private Vector3 syncStartPosition = Vector3.zero;
    private Vector3 syncEndPosition = Vector3.zero;

    [HideInInspector]
    public int size = 0;

    public Texture tex;
    public Texture selectorGreen;
    public Texture selectorRed;
    public Texture greenTexture;
    public Texture redTexture;
    [HideInInspector]
    private Color offColor = new Color(103, 0, 0); //Crimson
    public Color myColor = Color.red;

    //private Texture offColor = (Texture)Resources.Load("greenTexture"); //Crimson
    //public Texture myColor = (Texture)Resources.Load("greenTexture");

    private float unitDistanceFromCenter = 1.5f;

    public const int MAX_ACTIONS = 2;
    private int _numActions = MAX_ACTIONS;
    private Vector3 positionAtActionStart;
    private bool _midMovement = false;

    [HideInInspector]
    public float dodgeChance = BalanceConstants.Stats.BASIC_DODGE_CHANCE;
    [HideInInspector]
    public float hitChance = BalanceConstants.Stats.BASIC_HIT_CHANCE;

    public bool midMovement { get { return _midMovement; } }
    public int numActions { get { return _numActions; } }

    [HideInInspector]
    public string squadType;

    //Added lights for showing targeted.
    private GameObject myLight;
    [HideInInspector]
    public Light lightPiece;

    [HideInInspector]
    public Projector moveProj;
    [HideInInspector]
    public Projector attackProj;
    [HideInInspector]
    public NetworkView nView;

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
    private int myNumber;

    public AudioSource walk;
    public AudioClip walkSound;
    public bool hasCurTime;
    private DateTime curTime;
    private DateTime firstTime;
    private DateTime dTime;

    // Use this for initialization
    public void init(string squadTag)
    {
        walk = GetComponent<AudioSource>();
        walkSound = walk.clip;
        hasCurTime = false;

        myLight = new GameObject();
        myLight.transform.position = transform.position;
        lightPiece = myLight.AddComponent<Light>();
        lightPiece.color = Color.red;
        lightPiece.intensity = 8;
        tag = squadTag;

        attackDistance = 2;
        movementDistance = 20;
        //offColor = myColor / 2;


        moveProj = GameObject.Find("MoveRadius").GetComponent<Projector>();
        attackProj = GameObject.Find("AttackRadius").GetComponent<Projector>();
        nView = GetComponent<NetworkView>();

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
            TimeSpan seconds;
            if (!hasCurTime)
            {
                firstTime = DateTime.Now;
                walk.PlayOneShot(walkSound);
                hasCurTime = true;
            }

            if (hasCurTime)
            {
                curTime = DateTime.Now;
                seconds = curTime - firstTime;
                if (seconds.TotalSeconds > 1.1)
                    hasCurTime = false;
            }

            float h = Terrain.activeTerrain.SampleHeight(transform.position) + FLOOR_DISPACEMENT;
            transform.position = new Vector3(transform.position.x, h, transform.position.z);
            if ((positionAtActionStart - transform.position).magnitude >= movementDistance)
            {
                //endMovement();
                transform.position = prevPosition;
            }
            if (transform.position.x > 62 || transform.position.x < -63 || transform.position.z > 100 || transform.position.z < -97)
                transform.position = prevPosition;
        }
        else
        {
            if (rb != null)
            {
                rb.Sleep();
                walk.Stop();
            }
        }
        prevPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
		if (_midMovement) {
			Quaternion rot = Camera.main.transform.rotation;
			rot.x=rot.z=0;
			for (int i = 0; i < units.Length; i++)
			{
				units[i].transform.rotation = rot;
			}
		}
        for (int i = 0; i < units.Length; i++)
        {
            units[i].transform.position = unitTargets[i].position;
            transform.rotation = Quaternion.identity;
            //TODO: This should be targetting instead of teleporting
        }


        //Updates associated light
        //myLight.transform.position = transform.position;

    }

    public void startMovement()
    {
        if (numActions > 0)
        {
            positionAtActionStart = transform.position;
            moveProj.transform.position = new Vector3(transform.position.x, 9, transform.position.z);
            moveProj.orthographicSize = movementDistance + 2;
            moveProj.gameObject.SetActive(true);

            attackProj.transform.position = new Vector3(transform.position.x, 9, transform.position.z);
            attackProj.orthographicSize = attackDistance;
            attackProj.gameObject.SetActive(true);
            attackProj.enabled = true;

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

            moveProj.gameObject.SetActive(false);
            attackProj.gameObject.SetActive(false);

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
            moveProj.gameObject.SetActive(false);
        }
        else throw new UnityException("Attempted to skip an action when squad had none.");
    }

    public void resetActions()
    {
        if (!dead)
        {
            _numActions = MAX_ACTIONS;
            _midMovement = false;
            activeSquadColor(false);
            moveProj.gameObject.SetActive(false);
        }
    }

    public void undoMove()
    {
        if (_midMovement)
        {
            _midMovement = false;
            transform.position = positionAtActionStart;
            inCover = prevCover;
            moveProj.gameObject.SetActive(false);
            attackProj.gameObject.SetActive(false);
        }
        else throw new UnityException("Attempted to undo a move when squad had not moved");
    }

    public void takeDamage(int damage, bool killSpecial)
    {
        lightPiece.enabled = false;
        if (damage > 0)
        {
            int remainingToKill = damage;
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
                //Network.RemoveRPCsInGroup(0);
                //Network.Destroy(gameObject);
                //gameObject.SetActive(false);
                gameObject.tag = "Dead";
                _numActions = 0;

                //lightPiece.enabled = false;
            }
        }

    }

    //Checks all units of the squad to see if they are active or not.
    public bool isDead()
    {
        if (!dead)
            foreach (GameObject unit in units)
            {
                if (unit.activeInHierarchy)
                {
                    return false;
                }
            }
        _numActions = 0;
        return dead = true;
    }

    public void enableLight()
    {
        lightPiece.enabled = true;
    }

    public void disableLight()
    {
        lightPiece.enabled = false;
    }

    public void enableTarget()
    {
        targeted = true;
    }

    public void disableTarget()
    {
        targeted = false;
    }

    public void enableSelect()
    {
        selected = true;
    }

    public void disableSelect()
    {
        selected = false;
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

    /// <summary>
    /// Used to set the main color of a unit
    /// </summary>
    /// <param name="c"></param>
    public void setColor(Color c)
    {
        myColor = c;
        offColor = myColor / 2;
    }

    /// <summary>
    /// //Called by sub squad scripts once units are intialized
    /// </summary>
    public void paintColor()
    {
        foreach (GameObject g in units)
        {
            foreach (Transform child in g.transform)
            {
                if (child.name == "UnitBase")
                {
                    child.GetComponent<Renderer>().material.color = myColor;
                    break;
                }
            }
        }
    }

    public void activeSquadColor(bool active)
    {

        foreach (GameObject g in units)
        {
            if (g.activeInHierarchy)
                foreach (Transform child in g.transform)
                {
                    if (child.name == "UnitBase")
                    {
                        if (active)
                            child.GetComponent<Renderer>().material.color = myColor;
                        else
                            child.GetComponent<Renderer>().material.color = offColor;
                        break;
                    }
                }
        }
    }

    void OnGUI()
    {
        if (dead)
            return;

        if (tex != null && (inCover || behindWall))
        {
            float sizeMod = 15;
            Vector3 guiPosition = Camera.main.WorldToScreenPoint(transform.position);
            float camDistance = Camera.main.GetComponent<CameraController>().distance;
            guiPosition.y = Screen.height - guiPosition.y;
            Rect rect = new Rect((guiPosition.x - tex.width / (camDistance / sizeMod) / 2.0f), (guiPosition.y - tex.height / (camDistance / sizeMod) / 2.0f), tex.width / (camDistance / sizeMod), tex.height / (camDistance / sizeMod));
            GUI.DrawTexture(rect, tex);
        }

        if (selectorRed != null && targeted)
        {
            float sizeMod = 15;
            Vector3 guiPosition = Camera.main.WorldToScreenPoint(transform.position);
            float camDistance = Camera.main.GetComponent<CameraController>().distance;
            guiPosition.y = Screen.height - guiPosition.y;
            Rect rect = new Rect((guiPosition.x - selectorRed.width / (camDistance / sizeMod) / 2.0f), (guiPosition.y - selectorRed.height / (camDistance / sizeMod) / 2.0f), selectorRed.width / (camDistance / sizeMod), selectorRed.height / (camDistance / sizeMod));
            GUI.DrawTexture(rect, selectorRed);
        }

        if (selectorGreen != null && selected)
        {
            float sizeMod = 15;
            Vector3 guiPosition = Camera.main.WorldToScreenPoint(transform.position);
            float camDistance = Camera.main.GetComponent<CameraController>().distance;
            guiPosition.y = Screen.height - guiPosition.y;
            Rect rect = new Rect((guiPosition.x - selectorGreen.width / (camDistance / sizeMod) / 2.0f), (guiPosition.y - selectorGreen.height / (camDistance / sizeMod) / 2.0f), selectorGreen.width / (camDistance / sizeMod), selectorGreen.height / (camDistance / sizeMod));
            GUI.DrawTexture(rect, selectorGreen);
        }
    }

}