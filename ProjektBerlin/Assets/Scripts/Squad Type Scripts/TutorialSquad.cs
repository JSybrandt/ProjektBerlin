using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TutorialSquad : MonoBehaviour {

    Tutorial gameLogic;
    TutorialManager squad;

    // Use this for initialization
    void Start () {

    }

    public void rifleInit()
    {
        squad = GetComponent<TutorialManager>();
        gameLogic = GameObject.Find("GameLogic").GetComponent<Tutorial>();

        squad.size = 5;
        squad.attackDistance = 20;
        squad.movementDistance = 40;
		squad.squadType = "Rifle";

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
            squad.units[i].GetComponent<UnitManager>().power = BalanceConstants.Stats.BASIC_POWER;
        }

        squad.units[squad.units.Length - 1].GetComponent<UnitManager>().power *= 2;
        squad.units[squad.units.Length - 1].GetComponent<UnitManager>().isSpecial = true;
        squad.units[squad.units.Length - 1].transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);

        squad.unitAbility = new TutorialManager.Ability(grenade);
        squad.squadAbility = new TutorialManager.Ability(retreat);
        squad.unitAbilityUpdate = new TutorialManager.AbilityUpdate(grenadeUpdate);
        squad.squadAbilityUpdate = new TutorialManager.AbilityUpdate(retreatUpdate);

        squad.paintColor();
    }

    void grenade()
    {
        TutorialCombat.setupAoE(gameObject, 10, 5);
    }

    void grenadeUpdate()
    {
        bool activated = false;

        TutorialCombat.updateAoE(GetComponent<TutorialManager>(), ref activated);

        if (activated)
        {

            List<GameObject> targets = TutorialCombat.getTargets();

            foreach(GameObject target in targets)
            {
                TutorialManager enemy = target.GetComponent<TutorialManager>();
				ShotsFired shot = new ShotsFired(this.transform.position,
				                                 enemy.transform.position,
				                                 enemy.getActiveUnitsCount(),
				                                 BalanceConstants.Ability.GRENADE_HIT_CHANCE,
				                                 enemy.dodgeChance,false);
				int damage = TutorialCombat.calculateDamage(shot);

                //enemy.nView.RPC("takeDamage", RPCMode.All, damage, false);
                enemy.takeDamage(damage,false);
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
