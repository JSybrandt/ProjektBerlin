/// <summary>
/// Hits are classes meant to be collected by the Squad Manager from its Units when an attack is made.
/// </summary>
public class Hit
{
    //public Hit() { }
    public Hit(int hit=0, int dodge=0, bool partial = false)
    { hitChance = hit; dodgeChance = dodge; hasPartialCover = partial; }

    public int hitChance=0;             //Chance of the attacking unit to hit
    public int dodgeChance=0;           //Chance of the defending unit to dodge
    public bool hasPartialCover=false;  //If the hit passed through partial cover
};