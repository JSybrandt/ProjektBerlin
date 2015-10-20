using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

enum TurnStage
{
    None,
    Moving,
    Combat,
    InBetween
}

enum AttackType
{
    Basic,
    Unit,
    Squad
}

public class Controller : MonoBehaviour
{
    //FLAGS:
    private bool finalTurn = false;

    public const int NUM_PLAYERS = 2;
    [HideInInspector]
    public int numPlayers = NUM_PLAYERS;

    public float turnSpeed = 20;
    public float zoomSpeed = 20;

    private GameObject[] squads;

    //Attacking variables
    [HideInInspector]
    public List<GameObject> targetsInRange;

    //DON'T HIDE LAYER MASKS: Uses inspector.
    public LayerMask detectCover;
    public LayerMask detectPartial;
    public LayerMask detectWall;
    [HideInInspector]
    public Projector attackProj;
    [HideInInspector]
    public int selectedTargetIndex;
    [HideInInspector]
    public Projector changeUnit;

    private int selectedSquadIndex;
    private Light selectedLight;

    private Rigidbody selectedRB; //used to move selected squad

    private Vector3 lightOffset = new Vector3(0, 2, 0);

    private TurnStage currentStage = TurnStage.None;
    private AttackType currentAttack = AttackType.Basic;

    [HideInInspector]
    public GameObject marker;

    [HideInInspector]
    public int currentPlayersTurn = 0;
    private bool isRoundOver = false;
    public bool isOtherRoundOver = false;

	private ArrayList allSquads;//taken on init from LoadGame
    private bool isTurn = false;
    private static bool isRunning = false;
	public static bool getIsRunning(){
		return isRunning;
	}
    public NetworkLogic nLogic;
    public NetworkView nLogicView;

	public AudioSource[] sounds;
	public AudioSource click;
	public AudioSource move;
	public AudioSource attack;

	//used in updateUI
	Canvas movCanvas;
	Canvas comCanvas;
	Canvas firstActCanvas;
	Canvas secondActCanvas;
	Canvas netCanvas;
	Canvas waitCanvas;

    //called by loadgame
    public void init()
    {
		sounds = GetComponents<AudioSource>();
		click = sounds [0];
		move = sounds [1];
		attack = sounds [2];

		movCanvas = GameObject.Find("MovementCanvas").GetComponent<Canvas>();
		comCanvas = GameObject.Find("CombatCanvas").GetComponent<Canvas>();
		firstActCanvas = GameObject.Find("FirstActionCanvas").GetComponent<Canvas>();
		secondActCanvas = GameObject.Find("SecondActionCanvas").GetComponent<Canvas>();
		netCanvas = GameObject.Find ("NetworkCanvas").GetComponent<Canvas> ();
		waitCanvas = GameObject.Find ("WaitingCanvas").GetComponent<Canvas> ();

        Combat.gameLogic = this;

        marker = new GameObject();
        marker.transform.position = Vector3.zero;

		allSquads = GetComponent<LoadGame> ().getAllSquads();

        selectedLight = GameObject.Find("SelectedLight").GetComponent<Light>();
        if (selectedLight == null) throw new MissingReferenceException("Need SelectedLight");
        targetsInRange = new List<GameObject>();
        selectedTargetIndex = -1;

        attackProj = GameObject.Find("AttackRadius").GetComponent<Projector>();
        changeUnit = GameObject.Find("ChangeUnit").GetComponent<Projector>();

		//needed for unit movement to work out, no one should fly away
		Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Squad"),LayerMask.NameToLayer("Squad"));
		Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Unit"),LayerMask.NameToLayer("Squad"));
		Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Unit"),LayerMask.NameToLayer("Unit"));
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Unit"), LayerMask.NameToLayer("PartialCover"));
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Squad"), LayerMask.NameToLayer("PartialCover"));
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("PartialCover"), LayerMask.NameToLayer("Squad"));
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("PartialCover"), LayerMask.NameToLayer("Unit"));

        updateUI();
    }

    public void updateSquadList(string tag)
    {
        squads = GameObject.FindGameObjectsWithTag(tag);
        selectedSquadIndex = 0;
        selectNextAvailableSquad();
        if (squads.Length == 0) throw new UnityException("Failed to find squad.");
		getMainCamController ().setCameraTarget (squads [selectedSquadIndex].transform.position);
        setLight();
    }

    private void setLight()
    {
        selectedLight.transform.position = squads[selectedSquadIndex].transform.position + lightOffset;
    }

    private void checkChangeSquad()
    {
        if (Input.GetButtonUp("R1"))
        {
            selectNextAvailableSquad();
			click.Play();
            
            if (selectedRB != null) selectedRB.velocity = Vector3.zero;
			//Camera.main.GetComponent<CameraController>().setCameraTarget(squads[selectedSquadIndex].transform.position);


        }
        if (Input.GetButtonUp("L1"))
        {
			click.Play();
			selectPrevAvailableSquad();

            if (selectedRB != null) selectedRB.velocity = Vector3.zero;
			//Camera.main.GetComponent<CameraController>().setCameraTarget(squads[selectedSquadIndex].transform.position);

        }

        selectedRB = squads[selectedSquadIndex].GetComponent<Rigidbody>();

        if (selectedRB != null) selectedRB.velocity = Vector3.zero;

    }

	private CameraController getMainCamController(){
		return Camera.main.GetComponent<CameraController> ();
	}

    private SquadManager getSelectedManager()
    {
        return squads[selectedSquadIndex].GetComponent<SquadManager>();
    }

    private void checkNewAction()
    {
        //start move
        if (Input.GetAxis("DpadH") == -1)
        {
            if (getSelectedManager().numActions > 0)
            {
				move.Play();
                changeUnit.enabled = false;
                currentStage = TurnStage.Moving;
                getSelectedManager().startMovement();
				updateUI();
            }
        }
        //start start combat
        if (Input.GetAxis("DpadH") == 1)
        {
            if (getSelectedManager().numActions > 0)
            {
				attack.Play();
                currentStage = TurnStage.Combat;
                Combat.findTargets(selectedRB.gameObject);
                updateUI();
            }
        }
        /*//skip
        if (Input.GetAxis("DpadV") == -1)
        {
            if (getSelectedManager().numActions > 0)
            {
                currentStage = TurnStage.InBetween;
                getSelectedManager().skipAction();
            }
            if (getSelectedManager().numActions == 0)
            {
                currentStage = TurnStage.None;
            }
			checkStateEndOfAction();
        }
		*/
    }

	private void selectNextAvailableSquad(){
		for(int i =0; i<squads.Length; i++)
		{
			selectedSquadIndex++;
			selectedSquadIndex%=squads.Length;
			if(squads[selectedSquadIndex].activeInHierarchy && getSelectedManager().numActions>0)
			{
                Vector3 myPos = getSelectedManager().transform.position;
                getMainCamController().setCameraTarget(squads[selectedSquadIndex].transform.position);
                changeUnit.transform.position = new Vector3(myPos.x,9,myPos.z);
                //changeUnit.transform.position.y = 9;
				break;
			}
		}
	}
	private void selectPrevAvailableSquad(){
		for(int i =0; i<squads.Length; i++)
		{
			selectedSquadIndex--;
			if(selectedSquadIndex<0)selectedSquadIndex=squads.Length-1;
			if(squads[selectedSquadIndex].activeInHierarchy && getSelectedManager().numActions>0)
			{
                Vector3 myPos = getSelectedManager().transform.position;
                getMainCamController().setCameraTarget(squads[selectedSquadIndex].transform.position);
                changeUnit.transform.position = new Vector3(myPos.x, 9, myPos.z);
                break;
			}
		}
	}

    public void checkStateEndOfAction()
    {
        Combat.reset();
        currentAttack = AttackType.Basic;
        attackProj.enabled = false;
        changeUnit.enabled = false;

		//if (GameObject.FindGameObjectsWithTag ("Player" + ((currentPlayersTurn + 1) % NUM_PLAYERS) + "Squad").Length == 0) {
		//	Debug.Log("GAME OVER! PLAYER "+ (currentPlayersTurn+1) +" victory!");
		//	Application.Quit();
		//}

        if (getSelectedManager ().numActions == SquadManager.MAX_ACTIONS) {
			currentStage = TurnStage.None;
		}
		else if(getSelectedManager ().numActions == 0) {
            getSelectedManager().nView.RPC("activeSquadColor", RPCMode.All, false);
            nextTurn();
		}
        else
            currentStage = TurnStage.InBetween;
   
		updateUI();
    }

    /// <summary>
    /// Called by network logic to communicate who's turn it is
    /// </summary>
    /// <param name="turn"></param>
    public void setTurn(bool turn)
    {
        isTurn = turn;
        if (isTurn)
        {
            changeUnit.enabled = true;
            changeUnit.material.color = Color.green;
            selectedLight.enabled = true;
            if(isRunning)
                selectNextAvailableSquad();
        }
        updateUI();
    }

    float GetSquadAngle()
    {
        return 90 - Mathf.Rad2Deg * Mathf.Atan2(transform.forward.z, transform.forward.x); // Left handed CW. z = angle 0, x = angle 90
    }

    public bool hasActiveSquads()
    {
        foreach(GameObject g in allSquads)
        {
            if (g.GetComponent<SquadManager>().numActions > 0 && g.activeInHierarchy)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Handles checking if the round is over and ending the last turn of the round
    /// </summary>
    /// <returns></returns>
    bool checkRoundComplete()
    {
        if (!isRoundOver)
        {
            foreach (GameObject g in allSquads)
            {
                if (g.GetComponent<SquadManager>().numActions > 0 && g.activeInHierarchy)
                    return isRoundOver = false;
            }

            //This is here so it is only called once per round
            nLogicView.RPC("otherRoundOver",RPCMode.Others);
            nLogicView.RPC("setTurn", RPCMode.Others, true);
            setTurn(false);
        }

        return isRoundOver = true;
    }

	void nextTurn(){
        if (checkRoundComplete() && isOtherRoundOver)
        {
            nextRound();
            if (Network.isServer) //Server starts next round
            {
                nLogicView.RPC("setTurn", RPCMode.Others, false);
                setTurn(true);
            }
        }
        else if(!isRoundOver)
        {
            //currentPlayersTurn = (currentPlayersTurn + 1) % NUM_PLAYERS;
            //updateSquadList ("Player" + currentPlayersTurn + "Squad");
            currentStage = TurnStage.None;
            nLogicView.RPC("setTurn", RPCMode.Others, true);
            setTurn(false);
        }
	}

    void checkRound()
    {
        if (checkRoundComplete() && isOtherRoundOver)
        {
            nextRound();
            nLogicView.RPC("setTurn", RPCMode.Others, true);
            setTurn(false);
        }
    }

    public void begin()
    {
        isRunning = true;
		updateUI ();
    }

    //call at end of turn
    void nextRound()
    {
        foreach (GameObject g in allSquads)
        {
            if(g.activeInHierarchy)
                g.GetComponent<SquadManager>().resetActions();
        }
        isRoundOver = false;
        isOtherRoundOver = false;
        //currentPlayersTurn = (currentPlayersTurn + 1) % NUM_PLAYERS;
        //updateSquadList("Player" + currentPlayersTurn + "Squad");
        currentStage = TurnStage.None;
    }

    // Update is called once per frame
    void Update()
    {
		if (!isRunning)
			return;
        if (squads == null || !isTurn)
        {
            checkRound();
            return;
        }
        if (squads.Length > 0)
        {
            if (currentStage == TurnStage.None)
            {
                //skip turn button
                if (Input.GetButtonDown("Select")) { nextTurn(); }
                if (isRoundOver) nextTurn();
                checkChangeSquad();
                checkNewAction();
            }
            else if (currentStage == TurnStage.InBetween)
            {
                checkNewAction();
            }
            else if (currentStage == TurnStage.Moving)
            {

                //if the squad is no longer moving (triggered if max distance is met)
                if (!getSelectedManager().midMovement)
                {
                    //if we have another action
                    if (getSelectedManager().numActions > 0)
                    {
                        currentStage = TurnStage.InBetween;
                    }
                    else currentStage = TurnStage.None;
                }
                //user undo
                else if (Input.GetButtonDown("Circle")) //B
                {
                    getSelectedManager().undoMove();
                    checkStateEndOfAction();
					getMainCamController().setCameraTarget(squads[selectedSquadIndex].transform.position);
                    changeUnit.enabled = true;
                }
                //user ends early
                else if (Input.GetButtonDown("Cross"))  //A
                {
                    getSelectedManager().endMovement();
                    changeUnit.enabled = true;
                    checkStateEndOfAction();
                }
                else
                {
                    selectedRB = squads[selectedSquadIndex].GetComponent<Rigidbody>();
                    float v = Input.GetAxis("JoystickLV");
                    float h = Input.GetAxis("JoystickLH");
                    selectedRB.velocity = (Quaternion.Euler(0, getMainCamController().angle, 0) * new Vector3(h, 0, v).normalized) * 20;
					getMainCamController().setCameraTarget(squads[selectedSquadIndex].transform.position,true);
                }
            }
            else if (currentStage == TurnStage.Combat)
            {
                //TODO: enable combat in squad
                //skip

                if (Input.GetAxis("DpadV") == -1)
                {
                    getSelectedManager().skipAction();
                    checkStateEndOfAction();
                }
                if (currentAttack == AttackType.Basic)
                {
					if (Combat.UpdateTarget(selectedRB.GetComponent<SquadManager>()))   //A
                    {
						getMainCamController().freezeCamera (2);
                        Debug.Log("I shot someone!");
                        Combat.fightTarget(selectedRB.gameObject,getSelectedManager().getPower());
                        getSelectedManager().skipAction();
                        checkStateEndOfAction();
                        updateUI();
                    }
                    if (Input.GetButtonDown("Circle"))  //B
                    {
                        if (getSelectedManager().numActions == 2) currentStage = TurnStage.None;
                        if (getSelectedManager().numActions == 1) currentStage = TurnStage.InBetween;
                        //getSelectedManager().skipAction();
                        checkStateEndOfAction();
                        getMainCamController().setCameraTarget(squads[selectedSquadIndex].transform.position, true);
                    }
                    if (Input.GetButtonUp("Square"))
                    {
                        currentAttack = AttackType.Unit;
                        Combat.reset();
                        attackProj.enabled = false;
                        Debug.Log("Unit Ability");
                        getSelectedManager().unitAbility();
                    }
                    if (Input.GetButtonUp("Triangle"))
                    {
                        currentAttack = AttackType.Squad;
                        Combat.reset();
                        attackProj.enabled = false;
                        changeUnit.enabled = true;
                        getSelectedManager().squadAbility();
                        Debug.Log("Squad Ability");
                    }
                }
                else if(currentAttack == AttackType.Squad && getSelectedManager() != null) // && Input.GetButtonUp("Triangle")
                {
                    getSelectedManager().squadAbilityUpdate();
                }
                else if(currentAttack == AttackType.Unit && getSelectedManager() != null) // && Input.GetButtonUp("Square")
                {
                    getSelectedManager().unitAbilityUpdate();
                }

                if(Input.GetButtonUp("Circle")) //Reset to basic ability
                {
                    
                    currentAttack = AttackType.Basic;
                    Combat.findTargets(selectedRB.gameObject);
                }
            }
            setLight();
        }
    }

	public void updateUI(bool showNetScreen=false){


		waitCanvas.enabled = netCanvas.enabled = movCanvas.enabled = comCanvas.enabled = firstActCanvas.enabled = secondActCanvas.enabled = false;
		if (showNetScreen)
			netCanvas.enabled = true;
		else {
			if (isTurn && isRunning) {
                selectedLight.enabled = true;

                switch (currentStage) {
				case TurnStage.Combat:
					comCanvas.enabled = true;
					break;
				case TurnStage.InBetween:
					secondActCanvas.enabled = true;
					break;
				case TurnStage.Moving:
					movCanvas.enabled = true;
					break;
				case TurnStage.None:
					firstActCanvas.enabled = true;
					break;
				}
			} else {
                changeUnit.enabled = false;
                selectedLight.enabled = false;
				waitCanvas.enabled = true;
			}
		}
	}

}
