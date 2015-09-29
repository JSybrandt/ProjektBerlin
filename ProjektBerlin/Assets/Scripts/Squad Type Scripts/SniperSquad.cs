﻿using UnityEngine;
using System.Collections;

public class SniperSquad : MonoBehaviour {

    Controller gameLogic;
    SquadManager squad;

    // Use this for initialization
    void Start () {

    }

    public void init()
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
        bool activated = false;

        Combat.UpdateTarget(GetComponent<SquadManager>(),ref activated);

        if (activated)
        {
            Combat.getTarget().takeDamage(1, true);
            squad.skipAction();
            gameLogic.checkStateEndOfAction();
        }
    }

    void smokeScreen()
    {

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