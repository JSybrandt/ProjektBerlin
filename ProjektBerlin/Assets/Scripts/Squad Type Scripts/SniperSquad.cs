using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SniperSquad : MonoBehaviour {

    Controller gameLogic;
    SquadManager squad;

    // Use this for initialization
    void Start () {

    }

    [RPC]
    public void sniperInit()
    {
        squad = GetComponent<SquadManager>();
        gameLogic = GameObject.Find("GameLogic").GetComponent<Controller>();

        squad.size = 2;
        squad.attackDistance = 30;
        squad.movementDistance = 20;

        squad.unitTargets = new Transform[squad.size];

        for (int i = 0; i < squad.size; i++)
        {
            squad.unitTargets[i] = new GameObject().transform;
            squad.unitTargets[i].parent = transform;
            squad.unitTargets[i].localPosition = Quaternion.Euler(0, i * 360 / squad.size, 0) * Vector3.forward * 1.5f;
        }

        GameObject unitPrefab = (GameObject)Resources.Load("UnitSniper");
        if (unitPrefab == null)
            throw new MissingReferenceException("Failed to find Unit Prefab.");

        squad.units = new GameObject[squad.size];
        for (int i = 0; i < squad.size; i++)
        {
            squad.units[i] = (GameObject)Instantiate(unitPrefab, squad.unitTargets[i].position, Quaternion.identity);
            squad.units[i].transform.position = squad.unitTargets[i].position;
        }

        squad.units[squad.units.Length - 1].GetComponent<UnitManager>().power = 4;
        squad.units[squad.units.Length - 1].GetComponent<UnitManager>().isSpecial = true;
        squad.units[squad.units.Length - 1].transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);

        squad.unitAbility = new SquadManager.Ability(sniperShot);
        squad.squadAbility = new SquadManager.Ability(smokeScreen);
        squad.unitAbilityUpdate = new SquadManager.AbilityUpdate(sniperShotUpdate);
        squad.squadAbilityUpdate = new SquadManager.AbilityUpdate(smokeScreenUpdate);

        squad.paintColor();
    }

    // Update is called once per frame
    void Update () {
	
	}

    void sniperShot()
    {
        Combat.findTargets(gameObject, 60);
        //gameLogic.targetsInRange = squad.getTargets(gameLogic.currentPlayersTurn, gameLogic.numPlayers, Controller.detectCover, Controller.detectPartial, 60);
        //gameLogic.updateUI();
        Debug.Log("Sniper: Number of targets within range: " + gameLogic.targetsInRange.Count.ToString());
    }

    void sniperShotUpdate()
    {
		
		if (Combat.UpdateTarget(GetComponent<SquadManager>()))
        {
            ShotsFired snipe = new ShotsFired(this.transform.position,
			                                  Combat.getTarget().transform.position,
			                                  BalanceConstants.SNIPE_POWER,
			                                  BalanceConstants.SNIPE_HIT_CHANCE,
			                                  Combat.getTarget().dodgeChance,
			                                  false);
            int damage = Combat.calculateDamage(snipe);
            Combat.getTarget().takeDamage(damage, true);
            squad.skipAction();
            gameLogic.checkStateEndOfAction();
            gameLogic.updateUI();
        }
    }

    void smokeScreen()
    {
        bool activated = false;

        Combat.updateAoE(GetComponent<SquadManager>(), ref activated);

        if (activated)
        {
            //List<GameObject> targets = Combat.getTargets();

            squad.skipAction();
            gameLogic.checkStateEndOfAction();
            gameLogic.updateUI();
        }
    }

    void smokeScreenUpdate()
    {
        if (Input.GetButtonUp("Cross"))
        {
            //squad.skipAction();
            gameLogic.checkStateEndOfAction();
        }
    }
}
