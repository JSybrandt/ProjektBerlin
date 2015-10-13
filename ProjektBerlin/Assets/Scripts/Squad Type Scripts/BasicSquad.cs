﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BasicSquad : MonoBehaviour {

    Controller gameLogic;
    SquadManager squad;

    // Use this for initialization
    void Start () {

    }

    [RPC]
    public void basicInit()
    {
        squad = GetComponent<SquadManager>();
        gameLogic = GameObject.Find("GameLogic").GetComponent<Controller>();

        squad.size = 5;
        squad.attackDistance = 20;
        squad.movementDistance = 50;

        squad.unitTargets = new Transform[squad.size];

        for (int i = 0; i < squad.size; i++)
        {
            squad.unitTargets[i] = new GameObject().transform;
            squad.unitTargets[i].parent = transform;
            squad.unitTargets[i].localPosition = Quaternion.Euler(0, i * 360 / squad.size, 0) * Vector3.forward * 1.5f;
        }

        GameObject unitPrefab = (GameObject)Resources.Load("Unit");
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

        squad.unitAbility = new SquadManager.Ability(grenade);
        squad.squadAbility = new SquadManager.Ability(retreat);
        squad.unitAbilityUpdate = new SquadManager.AbilityUpdate(grenadeUpdate);
        squad.squadAbilityUpdate = new SquadManager.AbilityUpdate(retreatUpdate);

        squad.paintColor();
    }

    void grenade()
    {
        Combat.setupAoE(gameObject, 10, 5);
    }

    void grenadeUpdate()
    {
        bool activated = false;

        Combat.updateAoE(GetComponent<SquadManager>(), ref activated);

        if (activated)
        {

            List<GameObject> targets = Combat.getTargets();

            foreach(GameObject target in targets)
            {
                SquadManager enemy = target.GetComponent<SquadManager>();
				ShotsFired shot = new ShotsFired(this.transform.position,
				                                 enemy.transform.position,
				                                 enemy.getActiveUnitsCount(),
				                                 BalanceConstants.GRENADE_HIT_CHANCE,
				                                 enemy.dodgeChance,false);
				int damage = Combat.calculateDamage(shot);
                enemy.takeDamage(damage);
            }

            squad.skipAction();
            gameLogic.checkStateEndOfAction();
            gameLogic.updateUI();
        }
    }

    void retreat()
    {

    }

    void retreatUpdate()
    {

    }
	
	// Update is called once per frame
	//void Update () {
	
	//}
}
