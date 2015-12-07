using UnityEngine;
using System.Collections;

public static class BalanceConstants
{
    /// <summary>
    /// Sub category for ability stats
    /// </summary>
    public static class Ability
    {
        public static int SNIPE_POWER = 1;
        public static float SNIPE_HIT_CHANCE = 1f;
        public static float SNIPE_RADIUS = 60f;

        public static int GRENADE_POWER = 6;
        public static float GRENADE_HIT_CHANCE = 0.75f;
        public static float GRENADE_TARGET_RADIUS = 5f;
        public static float GRENADE_DAMAGE_RADIUS = 3f;
    }

    /// <summary>
    /// Sub category for squad/unit stats
    /// </summary>
    public static class Stats
    {
        //Basic: Default attack for infantry. Specialized units can set their own attack chance.
        public static int BASIC_POWER = 2;
        public static float BASIC_HIT_CHANCE = 0.66f;
        public static float BASIC_TARGET_RADIUS = 7f;
        public static float BASIC_DODGE_CHANCE = 0.33f;

        //Shotgun:
        public static int SHOTGUN_POWER = 4;
        //public static float SHOTGUN_ATTACK_HIT_CHANCE = 0.75f;
    }
}

public struct damageInfo
{
    int damage;
    bool killSpecial;
    public damageInfo(int dmg, bool kill)
    {
        damage = dmg;
        killSpecial = kill;
    }

    public int getDamage() { return damage; }
    public bool getKillSpecial() { return killSpecial; }
}

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
