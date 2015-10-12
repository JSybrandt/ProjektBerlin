using UnityEngine;
/*
a ShotsFired should be returned for each squad hit by a particular attack/ability from a squad/unit.
EX: a grenade that hits two squads should return two Hit objects.
*/

/// <summary>
/// Hits are classes meant to be collected by the Squad Manager from its Units when an attack is made.
/// </summary>
public class ShotsFired
{
    //public Hit() { }
	public ShotsFired(Vector3 source, Vector3 dest, int pow,float hit, float dodge, bool partial)
    { 
		this.source = source;
		this.destination = dest;
		this.power = pow;
		this.hitChance = hit; 
		this.dodgeChance = dodge; 
		this.hasPartialCover = partial; 
	}

    public float hitChance;             //Chance of the attacking unit to hit
    public float dodgeChance;           //Chance of the defending unit to dodge
    public bool hasPartialCover;  //If the hit passed through partial cover
	public int power; //number of dice rolled to damage
	public Vector3 source, destination;
};