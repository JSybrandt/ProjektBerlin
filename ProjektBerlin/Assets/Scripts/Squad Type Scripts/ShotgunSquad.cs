using UnityEngine;
using System.Collections;

public class ShotgunSquad : MonoBehaviour {

    Controller gameLogic;
    SquadManager squad;
    bool isInit = false;

    // Use this for initialization
    void Start()
    {

    }

    [RPC]
    public void shotgunInit()
    {
        squad = GetComponent<SquadManager>();
        gameLogic = GameObject.Find("GameLogic").GetComponent<Controller>();

        squad.size = 4;
        squad.attackDistance = 20;
        squad.movementDistance = 30;
		squad.squadType = "Shotgun";

        squad.unitTargets = new Transform[squad.size];

        for (int i = 0; i < squad.size; i++)
        {
            squad.unitTargets[i] = new GameObject().transform;
            squad.unitTargets[i].parent = transform;
            squad.unitTargets[i].localPosition = Quaternion.Euler(0, i * 360 / squad.size, 0) * Vector3.forward * 1.5f;
        }

        GameObject unitPrefab = (GameObject)Resources.Load("UnitShotgun");
        if (unitPrefab == null)
            throw new MissingReferenceException("Failed to find Unit Prefab.");

        squad.units = new GameObject[squad.size];
        for (int i = 0; i < squad.size; i++)
        {
            squad.units[i] = (GameObject)Instantiate(unitPrefab, squad.unitTargets[i].position, Quaternion.identity);
            squad.units[i].transform.position = squad.unitTargets[i].position;
            squad.units[i].GetComponent<UnitManager>().power = BalanceConstants.Stats.SHOTGUN_POWER;
        }

        squad.units[squad.units.Length - 1].GetComponent<UnitManager>().power *= 2;
        squad.units[squad.units.Length - 1].GetComponent<UnitManager>().isSpecial = true;
		squad.units[squad.units.Length - 1].transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);

        squad.unitAbility = new SquadManager.Ability(smokeScreen);
        squad.squadAbility = new SquadManager.Ability(shotBlast);
        squad.unitAbilityUpdate = new SquadManager.AbilityUpdate(smokeScreenUpdate);
        squad.squadAbilityUpdate = new SquadManager.AbilityUpdate(shotBlastUpdate);

        squad.paintColor();

        isInit = true;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void shotBlast()
    {
        Combat.findTargets(gameObject, 60);
        //gameLogic.targetsInRange = squad.getTargets(gameLogic.currentPlayersTurn, gameLogic.numPlayers, Controller.detectCover, Controller.detectPartial, 60);
        //gameLogic.updateUI();
        Debug.Log("Sniper: Number of targets within range: " + gameLogic.targetsInRange.Count.ToString());
    }

    void shotBlastUpdate()
    {

        if (Combat.UpdateTarget(squad))
        {
            ShotsFired blast = new ShotsFired(this.transform.position,
                                              Combat.getTarget().transform.position,
                                              squad.getPower(),
                                              squad.hitChance,
                                              Combat.getTarget().GetComponent<SquadManager>().dodgeChance,
                                              false);
            int damage = Combat.calculateDamage(blast);

            //Combat.getTarget().GetComponent<SquadManager>().takeDamage(damage,true);
            Combat.getTarget().GetComponent<NetworkView>().RPC("takeDamage", RPCMode.All, damage, true);
            squad.skipAction();
            gameLogic.checkStateEndOfAction();
            gameLogic.updateUI();
        }
    }

    void smokeScreen()
    {
        Combat.setupAoE(gameObject, 20, 0);
    }

    void smokeScreenUpdate()
    {
        bool activated = false;

        Combat.updateAoE(squad, ref activated);

        if (activated)
        {
            //List<GameObject> targets = Combat.getTargets();

            squad.skipAction();
            gameLogic.checkStateEndOfAction();
            gameLogic.updateUI();
        }
    }
}
